using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace Azureish
{
    public static class CommitWatcher
    {
        [FunctionName("HandleCommitEvent")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [SignalR(HubName = "commitHub")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            var commitId = data.head_commit.id.ToString().Substring(0, 8);
            var author = data.head_commit.author.username;
            var commitMessage = data.head_commit.message.ToString();

            string responseMessage = $"{commitId} - {author} - {commitMessage}";            

            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "gitMessage",
                    Arguments = new[] { new GitMessage { Message = responseMessage } }
                }
            );

            return new OkObjectResult(responseMessage);
        }
        
        [FunctionName("BeginNegotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest request,
            ILogger log,
            [SignalRConnectionInfo(HubName = "commitHub")] SignalRConnectionInfo connectionInfo)
        {
            log.LogInformation("Negotiating connection");

            return connectionInfo;

        }

        private class GitMessage {
            public string Message {get; set;}
        }
    }
}
