# iot-platform-within-azure

How to create an IoT platform within Microsoft Azure Cloud with Azure CLI.

## Architecture 


![Iot Architecture](img/architecture-iot.png)

Let's create a simple IoT platform with the following services :

- Azure IoT Hub
- Azure Functions
- Azure IoT Egde
- Azure CosmosDB
- Azure Service Bus
- Azure Storage

Futhermore, we will also create a simulated device within Azure Cloud to send and receive message.

## Installation of Azure cli

Let's install azure-cli 

    // For Mac 
    brew update && brew install azure-cli

## Connection to Azure Cloud with Azure cli

    az login

## Creation of services

### Add the IOT Extension for Azure CLI. You only need to install this the first time. You need it to create the device identity. 
    
    az extension add --name azure-cli-iot-ext
    
### Set the values for the resource names


    LOCATION="westeurope" # for location in france, iot hub is not available
    IOT_RESOURCE_GROUP="connected_bar_resources"
    IOT_HUB_CONSUMER_GROUP="connected_bar_hub_consumers"
    IOT_HUB_NAME="connected_bar_hub_$RANDOM"
    IOT_COSMOS_NAME="connected_bar_cosmos"
    IOT_COSMOS_DB_NAME="connected_bar_cosmosdb"
    IOT_COSMOS_COLLECtiON_NAME="connected_bar_collection"
    IOT_STORAGE_NAME="storage_for_connected_bar"
    IOT_FUNCTIONAPP_NAME="connected_bar_function_app"
    IOT_DEVICE_NAME="connected_bar_device"
    IOT_EDGE_DEVICE_NAME="myEdgeDevice"

    location="westeurope" # for location in france, iot hub is not available
    iotResourceGroup="ConnectedBarResources"
    iotHubConsumerGroup="ConnectedBarHubConsumers"
    iotCosmosName="connected-bar-cosmos"
    iotCosmosDBName="connected-bar-cosmosdb"
    iotCosmosCollectionName="connected-bar-collection"
    iotStorageName="storageforconnectedbar"
    iotFunctionAppName="connected-bar-function-app"
    iotDeviceName="ConnectedBarDevice"
    
### Create the resource group to be used for all the resources for this tutorial.
    
Within Azure services for the same project are taged by a resource group. Let's create one for our project.

    az group create --name $IOT_RESOURCE_GROUP--location $LOCATION
    az group create --name $iotResourceGroup--location $location
    
### Create the IoT hub
   
Let's create a free IoT Hub to interact with the external IoT world.

    // The IoT hub name must be globally unique, so add a random number to the end.
    az iot hub create --name IOT_HUB_NAME --resource-group $IOT_RESOURCE_GROUP --sku F1 --location $LOCATION
    
    iotHubName="ConnectedBarHub$RANDOM"
    echo "IoT hub name = " $iotHubName
    az iot hub create --name $iotHubName --resource-group $iotResourceGroup--sku F1 --location $location
    
### Add a consumer group to the IoT hub.
    
    az iot hub consumer-group create --hub-name $IOT_HUB_NAME --name IOT_HUB_CONSUMER_GROUP
    az iot hub consumer-group create --hub-name $iotHubName --name $iotHubConsumerGroup
    
### Create the IoT device identity to be used for testing.
    
    az iot hub device-identity create --device-id $IOT_DEVICE_NAME --hub-name $IOT_HUB_NAME
    az iot hub device-identity create --device-id $iotDeviceName --hub-name $iotHubName
    
Retrieve the information about the device identity, then copy the connection-string (hostname=...=) in your code.

    az iot hub device-identity show-connection-string --hub-name $IOT_HUB_NAME --device-id $IOT_DEVICE_NAME --output table
    az iot hub device-identity show-connection-string --hub-name $iotHubName --device-id $iotDeviceName --output table

### Create the IoT edge device identity.

    az iot hub device-identity create --hub-name $IOT_HUB_NAME --device-id $IOT_EDGE_DEVICE_NAME --edge-enabled

Retrieve the information about the device identity, then copy the connection-string (hostname=...=) in your code.

    az iot hub device-identity show-connection-string --device-id $IOT_EDGE_DEVICE_NAME --hub-name $IOT_HUB_NAME

### Create a CosmosDB in the resource group

Here we need to : 

- create a DocumentDB API Cosmos DB account
- create a database
- create a collection

#### Create a DocumentDB API Cosmos DB account

    az cosmosdb create \
        --name $IOT_COSMOS_NAME \
        --kind GlobalDocumentDB \
        --resource-group $IOT_RESOURCE_GROUP \
        --max-interval 10 \
        --max-staleness-prefix 200 

    az cosmosdb create \
        --name $iotCosmosName \
        --kind GlobalDocumentDB \
        --resource-group $iotResourceGroup\
        --max-interval 10 \
        --max-staleness-prefix 200 

#### Create a database 

    az cosmosdb database create \
        --name $IOT_COSMOS_NAME \
        --db-name $IOT_COSMOS_DB_NAME \
        --resource-group $IOT_RESOURCE_GROUP
        
    az cosmosdb database create \
        --name $iotCosmosName \
        --db-name $iotCosmosDBName \
        --resource-group $resourceGroup

#### Create a collection

    az cosmosdb collection create \
        --collection-name $IOT_COSMOS_COLLECtiON_NAME \
        --name $IOT_COSMOS_NAME \
        --db-name $IOT_COSMOS_DB_NAME \
        --resource-group $IOT_RESOURCE_GROUP
        
    az cosmosdb collection create \
        --collection-name $iotCosmosCollectionName \
        --name $iotCosmosName \
        --db-name $iotCosmosDBName \
        --resource-group $resourceGroup
        
#### Get the Azure Cosmos DB connection string.

    IOT_COSMOSDB_ENDPOINT=$(az cosmosdb show \
        --name $IOT_COSMOS_NAME \
        --resource-group $IOT_RESOURCE_GROUP \
        --query documentEndpoint \
        --output tsv)

    IOT_COSMOSDB_PRIMARY_KEY=$(az cosmosdb list-keys \
        --name $IOT_COSMOS_NAME \
        --resource-group $IOT_RESOURCE_GROUP \
        --query primaryMasterKey \
        --output tsv)


    iotCosmosdbEndpoint=$(az cosmosdb show \
        --name $iotCosmosName \
        --resource-group $resourceGroup \
        --query documentEndpoint \
        --output tsv)

    iotCosmosdbPrimaryKey=$(az cosmosdb list-keys \
        --name $iotCosmosName \
        --resource-group $resourceGroup \
        --query primaryMasterKey \
        --output tsv)
        
        
### Create a service bus too connect Iot Hub with a Function App

    IOT_SERVICEBUS_NAMESPACE="iot_service_bus_for_connected_bar"
    IOT_SERVICEBUS_NAME="iot_servicebus"
    
    iotServiceBusbNameSpace="iotServiceBusForConnectedBar"
    iotServiceBusName="iotServiceBus"
    
#### Create a Sevice Bus namespace
    
    az servicebus namespace create \
        --name $IOT_SERVICEBUS_NAMESPACE \
        --resource-group $IOT_RESOURCE_GROUP\
        -l $LOCATION
        
    az servicebus namespace create \
        --name $iotServiceBusbNameSpace \
        --resource-group $iotResourceGroup\
        -l $location
    
#### Create a Service Bus Queue

Here is the url of our service:

    [my namespace].servicebus.windows.net/[event hub name]/publishers/[my publisher name]

Let's create it : 

    az servicebus queue create \
        --name $IOT_SERVICEBUS_NAME \
        --namespace-name $IOT_SERVICEBUS_NAMESPACE \
        --resource-group $IOT_RESOURCE_GROUP
        
    az servicebus queue create \
        --name $iotServiceBusName \
        --namespace-name $iotServiceBusbNameSpace \
        --resource-group $iotResourceGroup
            
#### Get namespace connection string
        
Get the connection string for the namespace

        IOT_SERVICEBUS_CONNECTION_STRING=$(az servicebus namespace authorization-rule keys list \
           --resource-group $IOT_RESOURCE_GROUP \
           --namespace-name  $IOT_SERVICEBUS_NAMESPACE \
           --name RootManageSharedAccessKey \
           --query primaryConnectionString --output tsv)
           
        iotServiceBusConnectionString=$(az servicebus namespace authorization-rule keys list \
           --resource-group $iotResourceGroup\
           --namespace-name  $namespaceName \
           --name RootManageSharedAccessKey \
           --query primaryConnectionString --output tsv)

### Create a serverless function app in the resource group

First, let's create a storage account in the resource group associated to the function app.

    az storage account create \
      --name $IOT_STORAGE_NAME \
      --location $LOCATION \
      --resource-group $IOT_RESOURCE_GROUP \
      --sku Standard_LRS
      
    az storage account create \
      --name $iotStorageName \
      --location $location \
      --resource-group $iotResourceGroup\
      --sku Standard_LRS

Then, let's create the function app
https://github.com/Azure-Samples/functions-quickstart

    PROJECT_REPOSITORY_URL="https://github.com/ysennoun/iot-platform-within-azure.git"
    az functionapp create \
      --deployment-source-url $PROJECT_REPOSITORY_URL \
      --name $IOT_FUNCTIONAPP_NAME \
      --resource-group $IOT_RESOURCE_GROUP \
      --storage-account $IOT_STORAGE_NAME \
      --consumption-plan-location $LOCATION
      
    projectRepositoryUrl="https://github.com/ysennoun/iot-platform-within-azure.git"
    az functionapp create \
      --deployment-source-url $projectRepositoryUrl \
      --name $iotFunctionAppName \
      --resource-group $iotResourceGroup\
      --storage-account $iotStorageName \
      --consumption-plan-location $location
      
#### Configure function app settings to use the Azure Cosmos DB connection string.

    az functionapp config appsettings set \
        --name $IOT_FUNCTIONAPP_NAME \
        --resource-group $IOT_RESOURCE_GROUP \
        --setting CosmosDB_Endpoint=$IOT_COSMOSDB_ENDPOINT CosmosDB_Key=$IOT_COSMOSDB_PRIMARY_KEY
        
    az functionapp config appsettings set \
        --name $iotFunctionAppName \
        --resource-group $iotResourceGroup \
        --setting CosmosDB_Endpoint=$iotCosmosdbEndpoint CosmosDB_Key=$iotCosmosdbPrimaryKey
    
## Create the strategy for routing messages through The service Bus to Az Function

### Create endpoint for route from service bus

    endpointName="endpointServiceBus"
    az iot hub routing-endpoint create \
       --connection-string $serviceBusConnectionString \
       --endpoint-name $endpointName \
       --endpoint-resource-group $iotResourceGroup\
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
  