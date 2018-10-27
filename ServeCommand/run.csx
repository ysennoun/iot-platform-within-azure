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

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    // parse query parameter
    string idCommand = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "idCommand", true) == 0)
        .Value;
    string typeItem = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "type", true) == 0)
        .Value;
    if(idCommand == null || typeItem == null)
         return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a idCommand and a type on the query string or in the request body");
    //: req.CreateResponse(HttpStatusCode.OK, "idCommand is " + idCommand + ", and type is " + typeItem);

    Uri collectionUri = UriFactory.CreateDocumentCollectionUri("connected-bar-cosmosdb", "connected-bar-collection");
    IDocumentQuery<Command> query = client.CreateDocumentQuery<Command>(collectionUri)
        .Where(p => p.id == idCommand)
        .AsDocumentQuery();
    string app = ";";
    while (query.HasMoreResults)
    {
        foreach (Command result in await query.ExecuteNextAsync())
        {
            log.Info(result.drink.ToString());
            app = app + result.drink.ToString() + ";";
        }
    }



    Microsoft.Azure.Documents.Document doc = client.CreateDocumentQuery<Microsoft.Azure.Documents.Document>(collectionUri)
            .Where(p => p.Id == idCommand)
            .AsEnumerable()
            .SingleOrDefault();

    //Update MyProperty1 of the command object
    Command com = (dynamic)doc;
    com.drink = "HAWAI 2 for xebicon";

    //replace document
    Microsoft.Azure.Documents.Document doc2 = (dynamic) await client.ReplaceDocumentAsync(doc.SelfLink, com);
    Command updated = (dynamic)doc2;

    return updated.drink != "HAWAI 2 for xebicon"
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Could not replace")
        : req.CreateResponse(HttpStatusCode.OK, "idCommand is " + idCommand + ", and type is " + typeItem + "; drink is " + updated.drink);
}

