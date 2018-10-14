# iot-platform-within-azure

How to create an IoT platform with Microsoft Azure Cloud.

## Architecture 

Let's create a simple IoT platform with the following services :
- IoT Hub
- Functions
- CosmosDB

Futhermore, it is also possible to create a simulated device within Azure Cloud.

## Installation of Azure cli

    // On Mac 
    brew update && brew install azure-cli

## Connection to Azure Cloud with Azure cli

    az login

## Creation of services

### Add the IOT Extension for Azure CLI. You only need to install this the first time. You need it to create the device identity. 
    
    az extension add --name azure-cli-iot-ext
    
### Set the values for the resource names

    //list location codes
    az account list-locations
    // we select 'francecentral'
    location="westeurope" # location in france, iot hub is not available
    resourceGroup="ConnectedBarResources"
    iotHubConsumerGroup="ConnectedBarHubConsumers"
    iotCosmosName='connected-bar-cosmos'
    iotCosmosDBName="connected-bar-cosmosdb"
    iotCosmosCollectionName='connected-bar-collection'
    iotStorageName="connectedbarstorage"
    iotFunctionAppName="connected-bar-function-app"
    iotDeviceName="ConnectedBarDevice"
    
### Create the resource group to be used for all the resources for this tutorial.
    
    az group create --name $resourceGroup --location $location
    
        {
          "id": "/subscriptions/c6d07ac4-279c-4570-abac-e8e739a477d6/resourceGroups/ConnectedBarResources",
          "location": "francecentral",
          "managedBy": null,
          "name": "ConnectedBarResources",
          "properties": {
            "provisioningState": "Succeeded"
          },
          "tags": null
        }
    
    // The IoT hub name must be globally unique, so add a random number to the end.
    iotHubName="ConnectedBarHub$RANDOM"
    echo "IoT hub name = " $iotHubName
    
### Create the IoT hub.
   
    az iot hub create --name $iotHubName --resource-group $resourceGroup --sku F1 --location $location
    
### Add a consumer group to the IoT hub.
    
    az iot hub consumer-group create --hub-name $iotHubName --name $iotHubConsumerGroup
    
### Create the IoT device identity to be used for testing.
    
    az iot hub device-identity create --device-id $iotDeviceName --hub-name $iotHubName
    
    // Retrieve the information about the device identity, then copy the primary key to
    // "primaryKey": "d3X+L/r1vW6pFBCdqSAjfw7OO9RlQCE2NHQc5/JwK9w="
    //   Notepad. You need this to run the device simulation during the testing phase.
    az iot hub device-identity show-connection-string --hub-name $iotHubName --device-id $iotDeviceName --output table
    
    HostName=ConnectedBarHub9449.azure-devices.net;DeviceId=ConnectedBarDevice;SharedAccessKey=d3X+L/r1vW6pFBCdqSAjfw7OO9RlQCE2NHQc5/JwK9w=
    
    az iot hub show-connection-string --hub-name $iotHubName --output table
    HostName=ConnectedBarHub9449.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=SwxoCxBhn0pZYHfYaN+XRzLQ98vdcrNowwlc1zZaUAs=
### Create a CosmosDB in the resource group


#### Create a DocumentDB API Cosmos DB account

    az cosmosdb create \
        --name $iotCosmosName \
        --kind GlobalDocumentDB \
        --resource-group $resourceGroup \
        --max-interval 10 \
        --max-staleness-prefix 200 

#### Create a database 

    az cosmosdb database create \
        --name $iotCosmosName \
        --db-name $iotCosmosDBName \
        --resource-group $resourceGroup

#### Create a collection

    az cosmosdb collection create \
        --collection-name $iotCosmosCollectionName \
        --name $iotCosmosName \
        --db-name $iotCosmosDBName \
        --resource-group $resourceGroup
        
### Create a service bus too connect Iot Hub with a Function App

    serviceBusbNameSpace="iotServiceBusForConnectedBar"
    serviceBusName="iotServiceBus"
    
#### Create a Sevice Bus namespace
    
    az servicebus namespace create \
        --name $serviceBusbNameSpace \
        --resource-group $resourceGroup \
        -l $location
    
#### Create a Service Bus Queue

    az servicebus queue create \
        --name $serviceBusName \
        --namespace-name $serviceBusbNameSpace \
        --resource-group $resourceGroup
            
#### Get namespace connection string

    az servicebus namespace authorization-rule keys list \
            --resource-group $resourceGroup \
            --namespace-name $serviceBusbNameSpace \
            --name RootManageSharedAccessKey

        {
          "aliasPrimaryConnectionString": null,
          "aliasSecondaryConnectionString": null,
          "keyName": "RootManageSharedAccessKey",
          "primaryConnectionString": "Endpoint=sb://iotservicebusforconnectedbar.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3iDkINGJMkvJhamKojBtIQFH7oF73S1T4pnaOlkchtA=",
          "primaryKey": "3iDkINGJMkvJhamKojBtIQFH7oF73S1T4pnaOlkchtA=",
          "secondaryConnectionString": "Endpoint=sb://iotservicebusforconnectedbar.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=d09ch2h6IN5Wqtj2YMQx78z/rzJZbyX9Ux+08O9OIzk=",
          "secondaryKey": "d09ch2h6IN5Wqtj2YMQx78z/rzJZbyX9Ux+08O9OIzk="
        }
        
     subscriptionId="/subscriptions/c6d07ac4-279c-4570-abac-e8e739a477d6"
     serviceBusConnectionString="Endpoint=sb://iotservicebusforconnectedbar.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3iDkINGJMkvJhamKojBtIQFH7oF73S1T4pnaOlkchtA=;EntityPath=iotservicebus"
    //ajout entityPath à la fin (important)
    //[my namespace].servicebus.windows.net/[event hub name]/publishers/[my publisher name]

### Create a serverless function app in the resource group

First, let's create a storage account in the resource group associated to the function app.

    az storage account create \
      --name $iotStorageName \
      --location $location \
      --resource-group $resourceGroup \
      --sku Standard_LRS

Then, let's create the function app

    az functionapp create \
      --name $iotFunctionAppName \
      --resource-group $resourceGroup \
      --storage-account $iotStorageName \
      --consumption-plan-location $location
    
## Create the strategy for routing messages through The service Bus 

### Create endpoint for route from service bus

    endpointName="endpointServiceBus"
    az iot hub routing-endpoint create \
       --connection-string $serviceBusConnectionString \
       --endpoint-name $endpointName \
       --endpoint-resource-group $resourceGroup \
       --endpoint-subscription-id $subscriptionId \
       --endpoint-type servicebusqueue \
       --hub-name $iotHubName
       
    
### Create route

    routeName="route-to-"$endpointName
    az iot hub route create \
       --en $endpointName \
       --hub-name $iotHubName \
       --name $routeName \
       --source devicemessages
   
