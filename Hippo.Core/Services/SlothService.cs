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
        private readonly JsonSerializerOptions _serializerOptions;

        public SlothService(AppDbContext dbContext, ISecretsService secretsService, IOptions<SlothSettings> slothSettings, IHttpClientFactory clientFactory, IHistoryService historyService)
        {
            _dbContext = dbContext;
            _secretsService = secretsService;
            _slothSettings = slothSettings.Value;
            _clientFactory = clientFactory;
            _historyService = historyService;
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
                var apiKey = await _secretsService.GetSecret(financialDetail.SecretAccessKey.ToString());

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

        async Task<bool> ISlothService.ProcessPayments()
        {
            //Ok, so I want to loop through all the payments that are created and send them to sloth
            //I need to group them by clusterId because each cluster will have a team and different API key and source
            //If an error happens for a cluster, I want to log it and continue processing the other clusters but I also want to email a notification to the team
            // so keep track of them and mail after. The actual errors can be logged.
            // if a txn fails continue to the next one, but log it
            // if a txn is successful, update the status in the db, and set the kfs tracking number as well as the id from sloth into the payment.FinancialSystemId
            // if all txns are successful, return true, else return false


            var wereThereErrors = false;

            var paymentGroups = await _dbContext.Payments.Where(a => a.Status == Payment.Statuses.Created).GroupBy(a => a.Order.ClusterId).ToListAsync();
            foreach (var group in paymentGroups)
            {
                var clusterId = group.Key;
                var financialDetail = await _dbContext.FinancialDetails.SingleAsync(a => a.ClusterId == clusterId);
                var apiKey = await _secretsService.GetSecret(financialDetail.SecretAccessKey.ToString());

                using var client = _clientFactory.CreateClient();
                client.BaseAddress = new Uri($"{_slothSettings.ApiUrl}");
                client.DefaultRequestHeaders.Add("X-Auth-Token", apiKey);
                foreach (var payment in group)
                {
                    var order = await _dbContext.Orders.Include(a => a.Billings).Include(a => a.MetaData).Include(a => a.Cluster).SingleAsync(a => a.Id == payment.OrderId);

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

                        await _historyService.OrderUpdated(order, null, $"Payment sent for processing. Amount: {Math.Round(payment.Amount)}");
                        
                        await _dbContext.SaveChangesAsync();


                    }
                    else
                    {
                        Log.Error($"Error processing payment: {payment.Id} for Order: {payment.OrderId} Name: {payment.Order.Name}");
                        Log.Error($"Error: {response.ReasonPhrase}");
                        wereThereErrors = true;
                    }
                }

            }

            

            return !wereThereErrors;
        }

        private TransactionViewModel CreateTransaction(FinancialDetail financialDetail, Order order, Payment payment)
        {
            var slothTransaction = new TransactionViewModel
            {
                AutoApprove = financialDetail.AutoApprove,
                MerchantTrackingNumber = $"{payment.OrderId}-{payment.Id}",
                MerchantTrackingUrl = $"{_slothSettings.HippoBaseUrl}/{order.Cluster.Name}/order/details/{payment.OrderId}",
                Description = $"Order: {payment.OrderId} Name: {order.Name}",
                Source = financialDetail.FinancialSystemApiSource,
                SourceType = "Recharge",
            };

            slothTransaction.AddMetadata("OrderId", payment.OrderId.ToString());
            slothTransaction.AddMetadata("PaymentId", payment.Id.ToString());
            slothTransaction.AddMetadata("Cluster", order.Cluster.Name);
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

        Task<bool> ISlothService.UpdatePayments()
        {
            throw new NotImplementedException();
        }
    }
}
