using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Flow.Sdk.Nodes;
using Flow.Sdk.Types;
using Google.Protobuf;
using Grpc.Core;

namespace Flow.Sdk
{
    public class FlowClient
    {
        private Access.AccessAPI.AccessAPIClient client;
        public string CurrentChannel { get; private set; }

        private FlowClient(Access.AccessAPI.AccessAPIClient client)
        {
            this.client = client;
        }

        public static FlowClient Create(string target)
        {
            //TODO: Update this to not be insecure
            var channel = new Channel(target, ChannelCredentials.Insecure);
            var client = new Access.AccessAPI.AccessAPIClient(channel);
            var flowClient = new FlowClient(client);
            flowClient.CurrentChannel = target;
            return flowClient;
        }

        /// <summary>
        /// This will find the correct spork for the blockheight and allow it to auto change channels
        /// </summary>
        /// <param name="blockHeight"></param>
        public void ChangeChannel(ulong blockHeight)
        {
            var target = blockHeight switch
            {
                _ when blockHeight >= Sporks.MainNet.ROOT_HEIGHT => Sporks.MainNet.NODE,
                _ when blockHeight >= Sporks.MainNet5.ROOT_HEIGHT && blockHeight < Sporks.MainNet.ROOT_HEIGHT => Sporks.MainNet5.NODE,
                _ when blockHeight >= Sporks.MainNet4.ROOT_HEIGHT && blockHeight < Sporks.MainNet5.ROOT_HEIGHT => Sporks.MainNet4.NODE,
                _ when blockHeight >= Sporks.MainNet3.ROOT_HEIGHT && blockHeight < Sporks.MainNet4.ROOT_HEIGHT => Sporks.MainNet3.NODE,
                _ when blockHeight >= Sporks.MainNet2.ROOT_HEIGHT && blockHeight < Sporks.MainNet3.ROOT_HEIGHT => Sporks.MainNet2.NODE,
                _ when blockHeight >= Sporks.MainNet1.ROOT_HEIGHT && blockHeight < Sporks.MainNet2.ROOT_HEIGHT => Sporks.MainNet1.NODE,
            };
            if (target == CurrentChannel)
                return;

            var channel = new Channel(target, ChannelCredentials.Insecure);
            this.client = new Access.AccessAPI.AccessAPIClient(channel);
            CurrentChannel = target;
        }

        public async Task Ping()
        {
            await client.PingAsync(new Access.PingRequest());
        }

        public async Task<Access.BlockResponse> GetLatestBlockAsync(bool isSealed = true, CallOptions options = new CallOptions())
        {
            var request = new Access.GetLatestBlockRequest() { IsSealed = isSealed };
            var result = await client.GetLatestBlockAsync(request, options);
            return result;
        }

        public async Task<Access.EventsResponse> GetEventsForHeightRangeAsync(string eventType, ulong startHeight, ulong endHeight, CallOptions options = new CallOptions())
        {
            var request = new Access.GetEventsForHeightRangeRequest() { Type = eventType, StartHeight = startHeight, EndHeight = endHeight };

            var result = await client.GetEventsForHeightRangeAsync(request, options);
            return result;
        }

        public async Task<Access.ExecuteScriptResponse> ExecuteScriptAtBlockHeightAsync(ulong blockHeight, byte[] cadenceScript, IEnumerable<FlowValueType> args, CallOptions options = new CallOptions())
        {
            var scriptByteString = ByteString.CopyFrom(cadenceScript);
            var request = new Access.ExecuteScriptAtBlockHeightRequest() { BlockHeight = blockHeight, Script = scriptByteString };
            foreach (var arg in args)
                request.Arguments.Add(arg.ToByteString());

            return await client.ExecuteScriptAtBlockHeightAsync(request, options);
        }

        public async Task<Access.ExecuteScriptResponse> ExecuteScriptAtBlockIdAsync(ByteString blockId, byte[] cadenceScript, IEnumerable<FlowValueType> args, CallOptions options = new CallOptions())
        {
            var scriptByteString = ByteString.CopyFrom(cadenceScript);
            var request = new Access.ExecuteScriptAtBlockIDRequest() { BlockId = blockId, Script = scriptByteString };
            foreach (var arg in args)
                request.Arguments.Add(arg.ToByteString());

            return await client.ExecuteScriptAtBlockIDAsync(request, options);
        }

        public async Task<Access.BlockResponse> GetBlockByHeightAsync(ulong blockHeight, CallOptions options = new CallOptions())
        {
            var request = new Access.GetBlockByHeightRequest() { Height = blockHeight };
            return await client.GetBlockByHeightAsync(request, options);
        }

        public async Task<Access.TransactionResponse> GetTransactionAsync(ByteString transactionId)
        {
            var result = await client.GetTransactionAsync(new Access.GetTransactionRequest() { Id = transactionId });
            return result;
        }

        public async Task<Access.AccountResponse> GetAccountAsync(string address, ulong blockHeight)
        {
            var bytes = Encoding.ASCII.GetBytes(address);
            var byteString = ByteString.CopyFrom(bytes);
            var result = await client.GetAccountAtBlockHeightAsync(new Access.GetAccountAtBlockHeightRequest() { BlockHeight = blockHeight, Address = byteString });
            return result;
        }
    }
}
