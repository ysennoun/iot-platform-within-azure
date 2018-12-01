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
    string cosmosEndpoint = System.Environment.GetEnvironmentVariable("CosmosDB_Endpoint");
    string cosmosKey = System.Environment.GetEnvironmentVariable("CosmosDB_Key");
    DocumentClient customClient = new DocumentClient(
        new Uri(cosmosEndpoint),
        cosmosKey,
        new ConnectionPolicy
        {
            ConnectionMode = ConnectionMode.Direct,
            ConnectionProtocol = Protocol.Tcp,
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