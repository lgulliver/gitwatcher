using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Azureish
{
    public static class CommitWatcher
    {
        [FunctionName("CommitWatcher")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            var commitId = data.head_commit.id.ToString().Substring(0,8);
            var author = data.head_commit.author.username;
            var commitMessage = data.head_commit.message.ToString().Substring(0,20);
                        
            string responseMessage = $"{commitId} - {author} - {commitMessage}";

            return new OkObjectResult(responseMessage);
        }
    }
}
