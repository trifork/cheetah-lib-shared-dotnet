using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Cheetah.Kafka.Test.Util
{
    public class KafkaTopicDeleter : IAsyncDisposable
    {
        readonly IAdminClient _adminClient;
        readonly IEnumerable<string> _topicsToDelete;

        public KafkaTopicDeleter(IAdminClient adminClient, params string[] topicsToDelete)
        {
            _adminClient = adminClient;
            _topicsToDelete = topicsToDelete;
        }

        public async ValueTask DisposeAsync()
        {
            await _adminClient.DeleteTopicsAsync(_topicsToDelete);
        }
    }
}
