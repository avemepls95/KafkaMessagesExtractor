using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KafkaMessagesExtractor.DomainModel;
using KafkaMessagesExtractor.ResponseModel;
using Mapster;
using Newtonsoft.Json;
using RestSharp;
using Serilog;

namespace KafkaMessagesExtractor
{
    public class KafkaClient
    {
        private readonly ILogger _logger;

        public KafkaClient(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<DomainItem[]> FindAsync(
            SearchParameters searchParameters,
            OutputMessagesContainer outputMessagesContainer,
            CancellationToken cancellationToken)
        {
            var client = new RestClient(searchParameters.Url);
            var request = new RestRequest($"/topic/{searchParameters.TopicName}/messages", Method.GET);
            request.AddParameter("partition", searchParameters.Partition);
            request.AddParameter("offset", searchParameters.Offset);
            request.AddParameter("count", searchParameters.Count);
            request.AddParameter("keyFormat", "DEFAULT");
            request.AddParameter("format", "DEFAULT");

            var restSharpResponse = await client.ExecuteAsync<List<ResponseItem>>(request, cancellationToken).ConfigureAwait(false);

            var result = new List<DomainItem>(restSharpResponse.Data.Count);
            foreach (var responseItem in restSharpResponse.Data)
            {
                responseItem.Message = JsonConvert.DeserializeObject<ResponseMessage>(responseItem.Message.ToString());

                var domainItem = new DomainItem
                {
                    Partition = responseItem.Partition,
                    Offset = responseItem.Offset,
                    Message = new DomainMessage
                    {
                        Data = responseItem.Message.Data,
                        MetaData = JsonConvert.SerializeObject(responseItem.Message.MetaData)
                    },
                    Key = responseItem.Key,
                    Timestamp = responseItem.Timestamp
                };

                result.Add(domainItem);
            }

            return result.ToArray();
        }

        public async Task<DomainMessagesMetaDataItem[]> GetMessagesMetaData(
            string url,
            string topicName,
            CancellationToken cancellationToken)
        {
            var client = new RestClient(url);
            var request = new RestRequest($"/topic/{topicName}/messages", Method.GET);

            var restSharpResponse = await client
                .ExecuteAsync<List<ResponseMessagesMetaDataItem>>(request, cancellationToken)
                .ConfigureAwait(false);

            var result = restSharpResponse.Data.Adapt<DomainMessagesMetaDataItem[]>();
            return result;
        }
    }
}