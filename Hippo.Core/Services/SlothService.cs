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
using Octokit;


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
        private readonly JsonSerializerOptions _serializerOptions;

        public SlothService(AppDbContext dbContext, ISecretsService secretsService, IOptions<SlothSettings> slothSettings, IHttpClientFactory clientFactory)
        {
            _dbContext = dbContext;
            _secretsService = secretsService;
            _slothSettings = slothSettings.Value;
            _clientFactory = clientFactory;
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
            catch (Exception )
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


            var paymentGroups = await _dbContext.Payments.Include(a => a.Order.Cluster).Include(a => a.Order.Billings).Include(a => a.Order.MetaData).Where(a => a.Status == Payment.Statuses.Created).GroupBy(a => a.Order.ClusterId).ToListAsync();
            foreach (var group in paymentGroups)
            {
                var clusterId = group.Key;
                var financialDetail = await _dbContext.FinancialDetails.SingleAsync(a => a.ClusterId == clusterId);
                var apiKey = await _secretsService.GetSecret(financialDetail.SecretAccessKey.ToString());

                using var client = _clientFactory.CreateClient();
                client.BaseAddress = new Uri($"{_slothSettings.ApiUrl}Transactions/");
                client.DefaultRequestHeaders.Add("X-Auth-Token", apiKey);
                foreach(var payment in group)
                {
                    var slothTransaction = new TransactionViewModel
                    {
                        AutoApprove = financialDetail.AutoApprove,
                        MerchantTrackingNumber = $"{payment.OrderId}-{payment.Id}",
                        MerchantTrackingUrl = $"{_slothSettings.HippoBaseUrl}/{payment.Order.Cluster.Name}/order/details/{payment.OrderId}",
                        Description = $"Order: {payment.OrderId} Name: {payment.Order.Name}",
                        Source = financialDetail.FinancialSystemApiSource,
                        SourceType = "Recharge",
                    };

                    slothTransaction.AddMetadata("OrderId", payment.OrderId.ToString());
                    slothTransaction.AddMetadata("PaymentId", payment.Id.ToString());
                    slothTransaction.AddMetadata("Cluster", payment.Order.Cluster.Name);
                    if(!string.IsNullOrWhiteSpace(payment.Order.ExternalReference))
                    {
                        slothTransaction.AddMetadata("ExternalReference", payment.Order.ExternalReference);
                    }                    
                    slothTransaction.AddMetadata("OrderName", payment.Order.Name);
                    slothTransaction.AddMetadata("Product", payment.Order.ProductName);
                    slothTransaction.AddMetadata("Category", payment.Order.Category);                    
                    foreach(var meta in payment.Order.MetaData)
                    {
                        slothTransaction.AddMetadata(meta.Name, meta.Value);
                    }

                    var transfer = new TransferViewModel
                    {
                        Amount = Math.Round(payment.Amount, 2),
                        Description = $"Order: {payment.OrderId} Name: {payment.Order.Name}",
                        FinancialSegmentString = financialDetail.ChartString,
                        Direction = TransferViewModel.Directions.Credit,
                    };
                    slothTransaction.Transfers.Add(transfer);

                    foreach(var billing in payment.Order.Billings)
                    {
                        var debitTransfer = new TransferViewModel
                        {
                            Amount = Math.Round(payment.Amount * (billing.Percentage/100m), 2),
                            Description = $"Order: {payment.OrderId} Name: {payment.Order.Name}",
                            FinancialSegmentString = billing.ChartString,
                            Direction = TransferViewModel.Directions.Debit,
                        };
                        slothTransaction.Transfers.Add(debitTransfer);
                    }

                    var debtitTotal = slothTransaction.Transfers.Where(a => a.Direction == TransferViewModel.Directions.Debit).Sum(a => a.Amount);
                    var difference = debtitTotal - Math.Round(payment.Amount, 2);
                    if(difference != 0)
                    {
                        Log.Error($"The total debits do not match the total credit for Order: {payment.OrderId} Name: {payment.Order.Name}");
                        Log.Error($"Total Debits: {debtitTotal} Total Credit: {Math.Round(payment.Amount, 2)} Difference: {difference}");
                        
                    }
                }

            }

            throw new NotImplementedException();
        }

        Task<bool> ISlothService.UpdatePayments()
        {
            throw new NotImplementedException();
        }
    }
}
