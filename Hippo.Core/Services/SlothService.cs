using AngleSharp.Dom;
using Hippo.Core.Data;
using Hippo.Core.Models.Settings;
using Hippo.Core.Models.SlothModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Net.Http;
using Hippo.Core.Domain;
using Serilog;
using System.Net;
using Hippo.Core.Models.Email;


namespace Hippo.Core.Services
{
    public interface ISlothService
    {
        //Move money
        //Check Txns
        //Test API key
        Task<bool> TestApiKey(int clusterId);
        Task<bool> ProcessPayments(); //All clusters?
        Task<bool> UpdatePayments(); //All clusters?
    }
    public class SlothService : ISlothService
    {
        private readonly AppDbContext _dbContext;
        private ISecretsService _secretsService;
        private readonly SlothSettings _slothSettings;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHistoryService _historyService;
        private readonly INotificationService _notificationService;
        private readonly JsonSerializerOptions _serializerOptions;

        public SlothService(AppDbContext dbContext, ISecretsService secretsService, IOptions<SlothSettings> slothSettings, IHttpClientFactory clientFactory, IHistoryService historyService, INotificationService notificationService)
        {
            _dbContext = dbContext;
            _secretsService = secretsService;
            _slothSettings = slothSettings.Value;
            _clientFactory = clientFactory;
            _historyService = historyService;
            _notificationService = notificationService;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
            };
        }




        public async Task<bool> TestApiKey(int clusterId)
        {
            try
            {
                var financialDetail = await _dbContext.FinancialDetails.SingleAsync(a => a.ClusterId == clusterId);
                var apiKey = await _secretsService.GetSecret(financialDetail.SecretAccessKey);

                using var client = _clientFactory.CreateClient();
                client.BaseAddress = new Uri($"{_slothSettings.ApiUrl}Sources/");
                client.DefaultRequestHeaders.Add("X-Auth-Token", apiKey);
                var response = await client.GetAsync("");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var slothResponse = JsonSerializer.Deserialize<List<SourceModel>>(content, _serializerOptions);
                    if (slothResponse.Any(a => a.Name == financialDetail.FinancialSystemApiSource))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ProcessPayments()
        {
            //Ok, so I want to loop through all the payments that are created and send them to sloth
            //I need to group them by clusterId because each cluster will have a team and different API key and source
            //If an error happens for a cluster, I want to log it and continue processing the other clusters but I also want to email a notification to the team
            // so keep track of them and mail after. The actual errors can be logged.
            // if a txn fails continue to the next one, but log it
            // if a txn is successful, update the status in the db, and set the kfs tracking number as well as the id from sloth into the payment.FinancialSystemId
            // if all txns are successful, return true, else return false
            try
            {

                var wereThereErrors = false;
                var allPayments = await _dbContext.Payments.Include(a => a.Order).ThenInclude(a => a.Cluster).Where(a => a.Status == Payment.Statuses.Created).ToListAsync();

                var paymentGroups = allPayments.GroupBy(a => a.Order.ClusterId);

                //var paymentGroups = await _dbContext.Payments.Include(a => a.Order).ThenInclude(a => a.Cluster).Where(a => a.Status == Payment.Statuses.Created).GroupBy(a => a.Order.ClusterId).ToListAsync();
                foreach (var group in paymentGroups)
                {
                    var clusterId = group.Key;
                    var financialDetail = await _dbContext.FinancialDetails.SingleAsync(a => a.ClusterId == clusterId);
                    var apiKey = await _secretsService.GetSecret(financialDetail.SecretAccessKey);

                    using var client = _clientFactory.CreateClient();
                    client.BaseAddress = new Uri($"{_slothSettings.ApiUrl}");
                    client.DefaultRequestHeaders.Add("X-Auth-Token", apiKey);
                    foreach (var payment in group)
                    {
                        var order = await _dbContext.Orders.Include(a => a.Billings).Include(a => a.MetaData).Include(a => a.Cluster).SingleAsync(a => a.Id == payment.OrderId);

                        //TODO: ? Check if the txn is already on sloth? To do this sloth would need to be changed, or I'd need to write the the processor tracking number

                        var slothTransaction = CreateTransaction(financialDetail, order, payment);
                        Log.Information(JsonSerializer.Serialize(slothTransaction, _serializerOptions)); //MAybe don't need?

                        var response = await client.PostAsync("Transactions", new StringContent(JsonSerializer.Serialize(slothTransaction, _serializerOptions), Encoding.UTF8, "application/json"));
                        if (response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            Log.Information("Sloth Success Response", content);
                            var slothResponse = JsonSerializer.Deserialize<SlothResponseModel>(content, _serializerOptions);
                            payment.FinancialSystemId = slothResponse.Id;
                            payment.TrackingNumber = slothResponse.KfsTrackingNumber;
                            payment.Status = Payment.Statuses.Processing;

                            //Do this when the payment has been completed?
                            //order.BalanceRemaining -= Math.Round(payment.Amount, 2); //Should I update the order here?

                            await _historyService.OrderUpdated(order, null, $"Payment sent for processing. Amount: {Math.Round(payment.Amount, 2).ToString("C")}");

                            await _dbContext.SaveChangesAsync();


                        }
                        else
                        {
                            Log.Error($"Error processing payment: {payment.Id} for Order: {payment.OrderId} Name: {payment.Order.Name}");
                            Log.Error($"Error: {response.ReasonPhrase}");
                            await _historyService.OrderPaymentFailure(order, $"Error processing payment: {payment.Id} for Order: {payment.OrderId} Error: {response.ReasonPhrase}");
                            await _dbContext.SaveChangesAsync();
                            wereThereErrors = true;
                        }
                    }

                }



                return !wereThereErrors;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing payments");
                return false;
            }
        }

        private TransactionViewModel CreateTransaction(FinancialDetail financialDetail, Order order, Payment payment)
        {
            var slothTransaction = new TransactionViewModel
            {
                AutoApprove = false, //financialDetail.AutoApprove,
                MerchantTrackingNumber = $"{payment.OrderId}-{payment.Id}",
                ProcessorTrackingNumber = $"{payment.OrderId}-{payment.Id}",
                ValidateFinancialSegmentStrings = true,
                MerchantTrackingUrl = $"{_slothSettings.HippoBaseUrl}{order.Cluster.Name}/order/details/{payment.OrderId}",
                Description = $"Order: {payment.OrderId}-{payment.Id} Name: {order.Name}",
                Source = financialDetail.FinancialSystemApiSource,
                SourceType = "Recharge",
            };

            slothTransaction.AddMetadata("OrderId", payment.OrderId.ToString());
            slothTransaction.AddMetadata("PaymentId", payment.Id.ToString());
            slothTransaction.AddMetadata("Cluster", order.Cluster.Name);
            slothTransaction.AddMetadata("Balance Remaining (Including this)", order.BalanceRemaining.ToString("C"));
            if (order.IsRecurring)
            {
                slothTransaction.AddMetadata("Recurring", "True");
                slothTransaction.AddMetadata("Payment Cycle", order.InstallmentType);
                slothTransaction.AddMetadata("Installment Amount", order.InstallmentAmount.ToString("C"));

            }
            if (!string.IsNullOrWhiteSpace(order.ExternalReference))
            {
                slothTransaction.AddMetadata("ExternalReference", order.ExternalReference);
            }
            slothTransaction.AddMetadata("OrderName", order.Name);
            slothTransaction.AddMetadata("Product", order.ProductName);
            slothTransaction.AddMetadata("Category", order.Category);
            foreach (var meta in order.MetaData)
            {
                slothTransaction.AddMetadata(meta.Name, meta.Value);
            }

            var transfer = new TransferViewModel
            {
                Amount = Math.Round(payment.Amount, 2),
                Description = $"Order: {payment.OrderId} Name: {order.Name}",
                FinancialSegmentString = financialDetail.ChartString,
                Direction = TransferViewModel.Directions.Credit,
            };
            slothTransaction.Transfers.Add(transfer);

            foreach (var billing in order.Billings)
            {
                var debitTransfer = new TransferViewModel
                {
                    Amount = Math.Round(payment.Amount * (billing.Percentage / 100m), 2),
                    Description = $"Order: {payment.OrderId} Name: {order.Name}",
                    FinancialSegmentString = billing.ChartString,
                    Direction = TransferViewModel.Directions.Debit,
                };
                if (debitTransfer.Amount > 0)
                {
                    slothTransaction.Transfers.Add(debitTransfer);
                }
            }

            var difference = Math.Round(payment.Amount, 2) - slothTransaction.Transfers.Where(a => a.Direction == TransferViewModel.Directions.Debit).Sum(a => a.Amount);
            if (difference != 0)
            {
                //TODO: Test this
                Log.Error($"The total debits do not match the total credit for Order: {payment.OrderId} Name: {order.Name}");
                Log.Error($"Total Credit: {Math.Round(payment.Amount, 2)} Difference: {difference}");
                //Adjust the biggest debit
                var biggestDebit = slothTransaction.Transfers.Where(a => a.Direction == TransferViewModel.Directions.Debit).OrderByDescending(a => a.Amount).First();
                biggestDebit.Amount += difference;
            }

            return slothTransaction;
        }

        public async Task<bool> UpdatePayments()
        {
            var wereThereErrors = false;


            var allPayments = await _dbContext.Payments.Include(a => a.Order).ThenInclude(a => a.Cluster).Where(a => a.Status == Payment.Statuses.Processing).ToListAsync();

            var paymentGroups = allPayments.GroupBy(a => a.Order.ClusterId);

            foreach (var group in paymentGroups)
            {
                var clusterId = group.Key;
                var financialDetail = await _dbContext.FinancialDetails.SingleAsync(a => a.ClusterId == clusterId);
                var apiKey = await _secretsService.GetSecret(financialDetail.SecretAccessKey);

                using var client = _clientFactory.CreateClient();
                client.BaseAddress = new Uri($"{_slothSettings.ApiUrl}Transactions/");
                client.DefaultRequestHeaders.Add("X-Auth-Token", apiKey);
                foreach (var payment in group)
                {
                    if (string.IsNullOrWhiteSpace(payment.FinancialSystemId))
                    {
                        wereThereErrors = true;
                        Log.Error($"Error processing payment: {payment.Id} for Order: {payment.OrderId} Name: {payment.Order.Name} Error: Missing FinancialSystemId");
                        continue;
                    }
                    var order = await _dbContext.Orders.Include(a => a.PrincipalInvestigator).Include(a => a.Payments).Include(a => a.Billings).Include(a => a.MetaData).Include(a => a.Cluster).SingleAsync(a => a.Id == payment.OrderId);

                    var response = await client.GetAsync(payment.FinancialSystemId);
                    if (response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var slothResponse = JsonSerializer.Deserialize<SlothResponseModel>(content, _serializerOptions);

                        switch (slothResponse.Status)
                        {
                            case SlothStatuses.Completed:
                                payment.Status = Payment.Statuses.Completed;

                                await _historyService.OrderUpdated(order, null, $"Payment completed. Amount: {Math.Round(payment.Amount, 2).ToString("C")}");
                                order.BalanceRemaining -= Math.Round(payment.Amount, 2); //I'm going to do all the updates of this here

                                if (!order.IsRecurring)
                                {
                                    var totalPayments = order.Payments.Where(a => a.Status == Payment.Statuses.Completed).Sum(a => a.Amount);
                                    if (order.Total <= totalPayments)
                                    {
                                        order.Status = Order.Statuses.Completed;
                                        order.BalanceRemaining = 0;
                                        await _historyService.OrderUpdated(order, null, $"Order paid in full.");
                                        order.NextPaymentDate = null;
                                    }
                                }
                                try
                                {
                                    //write content to the order history? (See what it looks like)
                                    await _historyService.OrderPaymentCompleted(order, content);

                                    //Ucky, but email/core projects were not talking to each other like I expected.
                                    var debits =  slothResponse.Transfers.Where(a => a.Direction == TransferViewModel.Directions.Debit);
                                    var emailDebits = new List<EmailTransferResponseModel>();
                                    foreach (var debit in debits)
                                    {
                                        emailDebits.Add(new EmailTransferResponseModel
                                        {
                                            Amount = debit.Amount,
                                            FinancialSegmentString = debit.FinancialSegmentString,
                                            Direction = debit.Direction,
                                        });
                                        
                                    }
                                    var emailModel = new EmailOrderPaymentModel
                                    {
                                        Subject = "Payment Completed",
                                        Header = $"Payment completed for Order: {order.Name}",
                                        Transfers = emailDebits,
                                    };
                                    await _notificationService.OrderPaymentNotification(order, new string[] { order.PrincipalInvestigator.Email }, emailModel);
                                }
                                catch (Exception ex)
                                {
                                    Log.Error(ex, "Error tring to notify about payment");
                                }
                                await _dbContext.SaveChangesAsync();
                                break;
                            case SlothStatuses.Processing:
                            case SlothStatuses.PendingApproval:
                            case SlothStatuses.Scheduled:
                                //Do nothing
                                break;
                            case SlothStatuses.Rejected:
                                // do nothing? Should get fixed in sloth
                                break;
                            case SlothStatuses.Cancelled:
                                //Need to do something here
                                payment.Status = Payment.Statuses.Cancelled;
                                //order.BalanceRemaining += Math.Round(payment.Amount, 2); //Add the amount back to the balance remaining
                                await _historyService.OrderUpdated(order, null, $"Payment CANCELLED. Amount: {Math.Round(payment.Amount, 2).ToString("C")}");
                                await _dbContext.SaveChangesAsync();
                                break;
                            default:
                                wereThereErrors = true;
                                Log.Error($"Error processing payment: {payment.Id} for Order: {payment.OrderId} Name: {payment.Order.Name} Error: {slothResponse.Status}");
                                await _historyService.OrderPaymentFailure(order, $"Error processing payment: {payment.Id} for Order: {payment.OrderId} Error: {slothResponse.Status}");
                                await _dbContext.SaveChangesAsync();
                                break;
                        }

                    }
                    else
                    {
                        wereThereErrors = true;
                        Log.Error($"Error processing payment: {payment.Id} for Order: {payment.OrderId} Name: {payment.Order.Name}");
                        Log.Error($"Error: {response.ReasonPhrase}");
                        await _historyService.OrderPaymentFailure(order, $"Error processing payment: {payment.Id} for Order: {payment.OrderId} Error: {response.ReasonPhrase}");
                        await _dbContext.SaveChangesAsync();
                    }
                }

            }

            return !wereThereErrors;
        }
    }
}
