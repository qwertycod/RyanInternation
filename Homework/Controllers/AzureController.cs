using Azure;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text.Json;

namespace Homework.Controllers
{
    public class AzureController : Controller
    {
        [HttpGet()]      
        public IActionResult GetFormatters([FromServices] FormatterCollection<IOutputFormatter> formatters)
        {
            return Ok(formatters.Select(f => f.GetType().Name));
        }

        [HttpGet("TestEventHub")]
        public async Task<string> TestEventHub(int id, int deviceId)
        {
            var fullyQualifiedNamespace = "test200625r.servicebus.windows.net";
            var eventHubName = "test200625r";
            var keyName = "RootManageSharedAccessKey";
            var key = "";

            var credential = new AzureNamedKeyCredential(keyName, key);
            var client = new EventHubProducerClient(fullyQualifiedNamespace, eventHubName, credential);

            using EventDataBatch eventBatch = await client.CreateBatchAsync();

            eventBatch.TryAdd(new EventData(JsonSerializer.Serialize(new
            {
                id = $"sensor{id}-002-20250624T124838Z",
                deviceId = $"sensor-{deviceId}",
                temperature = 23.5,
                timestamp = DateTime.UtcNow
            })));

            await client.SendAsync(eventBatch);

            return "Ok";
        }
    }
}
