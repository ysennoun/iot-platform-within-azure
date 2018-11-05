#r "Microsoft.Azure.Documents.Client"
#r "Microsoft.Azure.WebJobs.Extensions.Http"
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

public static DocumentClient GetCustomClient()
{
    DocumentClient customClient = new DocumentClient(
        new Uri("https://connected-bar-cosmos.documents.azure.com:443/"),
        "3eBsUC8Bc81hCTsDbqftTacte4Co3z84GJWvvXryMb0D2YZGsp1k6Ea36YtA0Xz0YljKLUhvN2uZp3dWyHLtww==",
        new ConnectionPolicy
        {
            ConnectionMode = ConnectionMode.Direct,
            ConnectionProtocol = Protocol.Tcp,
            // Customize retry options for Throttled requests
            RetryOptions = new RetryOptions()
            {
                MaxRetryAttemptsOnThrottledRequests = 10,
                MaxRetryWaitTimeInSeconds = 30
            }
        });

    return customClient;
}

public static Uri GetCollectionUri() {
    return UriFactory.CreateDocumentCollectionUri("connected-bar-cosmosdb", "connected-bar-collection");
}

//https://blog.siliconvalve.com/2017/10/24/azure-api-management-200-ok-response-but-no-backend-traffic/