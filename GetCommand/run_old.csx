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


private static DocumentClient client = GetCustomClient();
private static DocumentClient GetCustomClient()
{
    DocumentClient customClient = new DocumentClient(
        //new Uri(ConfigurationManager.AppSettings["https://connected-bar-cosmos.documents.azure.com:443/"]),
        //ConfigurationManager.AppSettings["3eBsUC8Bc81hCTsDbqftTacte4Co3z84GJWvvXryMb0D2YZGsp1k6Ea36YtA0Xz0YljKLUhvN2uZp3dWyHLtww=="],
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

public class Command
{
    public string id { get; set; }
    public Int32 sendTime { get; set; }
    public Int32 receptionTime { get; set; }
    public string table { get; set; }
    public string drink { get; set; }
}

public static async Task Run(
    string myQueueItem,
    TraceWriter log)
{
    Uri collectionUri = UriFactory.CreateDocumentCollectionUri("connected-bar-cosmosdb", "connected-bar-collection");
    IDocumentQuery<Command> query = client.CreateDocumentQuery<Command>(collectionUri)
        .Where(p => p.drink == "jus")
        .AsDocumentQuery();

    while (query.HasMoreResults)
    {
        foreach (Command result in await query.ExecuteNextAsync())
        {
            log.Info("azerty");
            log.Info(result.drink.ToString());
        }
    }
}