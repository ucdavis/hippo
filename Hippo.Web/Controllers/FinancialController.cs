using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models;
using Hippo.Core.Models.OrderModels;
using Hippo.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using static Hippo.Core.Models.SlothModels.TransferViewModel;


namespace Hippo.Web.Controllers;



[Authorize(Policy = AccessCodes.FinancialAdminAccess)]
public class FinancialController : SuperController
{
    private AppDbContext _dbContext;

    private ISecretsService _secretsService;
    private IAggieEnterpriseService _aggieEnterpriseService;
    private ISlothService _slothService;

    public FinancialController(AppDbContext dbContext, ISecretsService secretsService, IAggieEnterpriseService aggieEnterpriseService, ISlothService slothService)
    {
        _dbContext = dbContext;
        _secretsService = secretsService;
        _aggieEnterpriseService = aggieEnterpriseService;
        _slothService = slothService;
    }

    [HttpGet]

    public async Task<IActionResult> FinancialDetails()
    {
        var cluster = await _dbContext.Clusters.AsNoTracking().SingleAsync(c => c.Name == Cluster);
        var existingFinancialDetail = await _dbContext.FinancialDetails.SingleOrDefaultAsync(fd => fd.ClusterId == cluster.Id);
        var clusterModel = new FinancialDetailModel
        {
            FinancialSystemApiKey = string.Empty,
            FinancialSystemApiSource = existingFinancialDetail?.FinancialSystemApiSource,
            ChartString = existingFinancialDetail?.ChartString,
            AutoApprove = existingFinancialDetail?.AutoApprove ?? false,
            MaskedApiKey = "NOT SET"
        };

        if (existingFinancialDetail != null)
        {
            var apiKey = await _secretsService.GetSecret(existingFinancialDetail.SecretAccessKey);
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                var sb = new StringBuilder();
                for (var i = 0; i < apiKey.Length; i++)
                {
                    if (i < 4 || i >= apiKey.Length - 4)
                    {
                        sb.Append(apiKey[i]);
                    }
                    else
                    {
                        sb.Append('*');
                    }
                }

                clusterModel.MaskedApiKey = sb.ToString();

                clusterModel.IsSlothValid = await _slothService.TestApiKey(cluster.Id);
            }
        }



        return Ok(clusterModel);
    }

    [HttpPost]

    public async Task<IActionResult> UpdateFinancialDetails([FromBody] FinancialDetailModel model)
    {
        //Possibly use the secret service to set the FinancialSystemApiKey
        var cluster = await _dbContext.Clusters.SingleAsync(c => c.Name == Cluster);
        var existingFinancialDetail = await _dbContext.FinancialDetails.SingleOrDefaultAsync(fd => fd.ClusterId == cluster.Id);
        if (existingFinancialDetail == null)
        {
            existingFinancialDetail = new FinancialDetail
            {
                ClusterId = cluster.Id,
                SecretAccessKey = Guid.NewGuid().ToString(),

            };
        }
        var validateChartString = await _aggieEnterpriseService.IsChartStringValid(model.ChartString, Directions.Credit);
        if (!validateChartString.IsValid)
        {
            return BadRequest($"Invalid Chart String Errors: {validateChartString.Message}");
        }
        if (!string.IsNullOrWhiteSpace(model.FinancialSystemApiKey))
        {
            await _secretsService.SetSecret(existingFinancialDetail.SecretAccessKey, model.FinancialSystemApiKey);
        }
        //var xxx = await _secretsService.GetSecret(existingFinancialDetail.SecretAccessKey);
        existingFinancialDetail.FinancialSystemApiSource = model.FinancialSystemApiSource;
        existingFinancialDetail.ChartString = validateChartString.ChartString;
        existingFinancialDetail.AutoApprove = model.AutoApprove;

        if (existingFinancialDetail.Id == 0)
        {
            await _dbContext.FinancialDetails.AddAsync(existingFinancialDetail);
        }
        else
        {
            _dbContext.FinancialDetails.Update(existingFinancialDetail);
        }

        await _dbContext.SaveChangesAsync();

        var clusterModel = new FinancialDetailModel
        {
            FinancialSystemApiKey = string.Empty,
            FinancialSystemApiSource = existingFinancialDetail?.FinancialSystemApiSource,
            ChartString = existingFinancialDetail?.ChartString,
            AutoApprove = existingFinancialDetail?.AutoApprove ?? false,
            MaskedApiKey = "NOT SET"
        };
        var apiKey = await _secretsService.GetSecret(existingFinancialDetail?.SecretAccessKey);
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            var sb = new StringBuilder();
            for (var i = 0; i < apiKey.Length; i++)
            {
                if (i < 4 || i >= apiKey.Length - 4)
                {
                    sb.Append(apiKey[i]);
                }
                else
                {
                    sb.Append('*');
                }
            }

            clusterModel.MaskedApiKey = sb.ToString();

            clusterModel.IsSlothValid = await _slothService.TestApiKey(cluster.Id);
        }

        return Ok(clusterModel);

    }
}

