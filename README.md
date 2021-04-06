<h1> CSharp Flow SDK </h1>

<h2> Creating a Client </h2>
To create a new FlowClient there is a Create factory method that takes in a target string that represents the spork endpoint you would like to connect to. In this exmplae we will use the static Spork class under the Flow.Sdk.Nodes namespace that contains all the possible main net sporks.This will need to be maintained as sporks are updated, but if you are reading from the latest block the MainNet will always continue to work.

```csharp 
var flowClient = FlowClient.Create(Sporks.MainNet.NODE); 
```

You can test if the Client is working by calling the Ping function.

```csharp 
await flowClient.Ping()
```

<h2> Client Change Channel </h2>
While searching through Flow if you are unsure which Spork the block height may belong to you can call ChangeChannel before any function with an execution at BlockHeight. The client will check the range of the sporks and change to the correct Spork channel if needed. If you are in the correct spork it will continue to use the channel you created.

```csharp
//Looping through blockheight to get a block 1 by 1
var blockHeight = 12345678
flowClient.ChangeChannel(blockHeight);
flowClient.GetEventsForHeightRangeAsync("YOUR_EVENT", blockHeight, blockHeight);
```

<h2> Protobuf </h2>
All the flow protobuffs are stored under the Proto folder.

<h2> Reading Events </h2>
  
When reading events off Flow you can use the ```FlowCompositeTypeConverter``` in JSON.net. This will break down [Cadence Composite Types](https://docs.onflow.org/cadence/json-cadence-spec#composites-struct-resource-event-contract-enum) to a Flow SDK ```CompositeType```. ```Composite Types``` consist of the Composite Type under the ```Type``` property, ```Id``` which is the token fully qualified type identifier, and a ```Dictionary<string,string>```. They Keys in the dictionary are the property names and the Values are the cadence values from the Block Event.

```csharp
 var response = await client.GetEventsForHeightRangeAsync(MOMENT_LISTED_EVENT, startBlockHeight, endBlockHeight);
 foreach (var block in response.Results.ToList())
  {
    foreach (var @event in block.Events)
    {
      //Decode the Protobuf BysteString
      var momentEvent = @event.Payload.ToString(Encoding.Default);

      //We need to deserialize with our custom serializer for composite types
      JsonSerializerOptions options = new();
      options.Converters.Add(new FlowCompositeTypeConverter());
      var momentComposite = JsonSerializer.Deserialize<CompositeType>(momentEvent, options);     
    } 
  }
```

In the above example you can now access Event data either via key or by using the built in value type converts that will convert Flow Composite Types to Flow.SDK.ValueTypes.

**By Key**
```csharp
momentComposite.Fields.FirstOrDefault(x => x.Key == "id").Value)
```
**Straight into a SDK ValueType**
```csharp
UInt64Type.FromJson(momentComposite.Fields.FirstOrDefault(x => x.Key == "id").Value);
```

<h2> Executing Cadence Scripts With Variables</h2>

```ExecuteScriptAtBlockHeightAsync``` allows you to execute your Cadence script at the desired Blockheight. If your Cadence script has arguments the last parameter of ```ExecuteScriptAtBlockHeightAsync``` will take a list of ```FlowValueType```. You can pass in as many as you like but please keep in mind **ORDER MATTERS**. Put the arguments in the list the same order they need to be injected into the script.The Value Types have a built in Cadence Serializer that will make sure your args are in the correct format. The script bytes will also be converted to the proper Google Protobuf ByteString. 

```csharp
 //Convert your script into a byte array
 var scriptBytes = Encoding.ASCII.GetBytes("Your raw Cadence script");
 //Create our Flow.SDK.ValueTypes
 var address = new AddressType("MyAddress");
 var momentId = new UInt64Type(123456789);
 
 //Execute the script
 var momentData = await client.ExecuteScriptAtBlockHeightAsync(blockHeight, scriptBytes, new List<FlowValueType>() { address, momentId });

```
