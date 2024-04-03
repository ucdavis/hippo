using System.Text.Json;
using Hippo.Core.Domain;
using Hippo.Core.Models;

namespace Hippo.Core.Extensions;

public static class RequestExtensions
{
    public static Request WithAccountRequestData(this Request request, AccountRequestDataModel data)
    {
        if (!AccountRequestDataModel.ValidActions.Contains(request.Action))
            throw new ArgumentException($"Invalid data type ({nameof(AccountRequestDataModel)}) for action {request.Action}");

        request.Data = JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        return request;
    }

    public static AccountRequestDataModel GetAccountRequestData(this Request request)
    {
        if (!AccountRequestDataModel.ValidActions.Contains(request.Action))
            throw new InvalidOperationException($"Cannot get {nameof(AccountRequestDataModel.SshKey)} from {nameof(request.Data)} for action {request.Action}");

        var data = string.IsNullOrWhiteSpace(request.Data)
            ? new ()
            : JsonSerializer.Deserialize<AccountRequestDataModel>(request.Data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return data;
    }
}