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


namespace Hippo.Core.Services
{
    public interface ISlothService
    {
        //Move money
        //Check Txns
        //Test API key
        Task<bool> TestApiKey(int clusterId);
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
    }
}
