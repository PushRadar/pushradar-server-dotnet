using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PushRadar
{
    public class PushRadar
    {
        private static readonly string version = "3.0.0-alpha.1";
        private readonly string apiEndpoint = "https://api.pushradar.com/v3";
        private string secretKey = null;

        public PushRadar(string secretKey)
        {
            secretKey = secretKey.Trim();

            if (secretKey == "" || !secretKey.StartsWith("sk_"))
                throw new Exception("Please provide your PushRadar secret key. You can find it on the API page of your dashboard.");

            this.secretKey = secretKey;
        }

        private void ValidateChannelName(string channelName)
        {
            if (!Regex.IsMatch(channelName, "^[-a-zA-Z0-9_=@,.;]+$"))
            {
                throw new Exception("Invalid channel name: " + channelName + ". Channel names cannot contain spaces, and must consist of only " +
                    "upper and lowercase letters, numbers, underscores, equals characters, @ characters, commas, periods, semicolons, and " +
                    "hyphens (A-Za-z0-9_=@,.;-).");
            }
        }

        public bool Broadcast(string channelName, object data)
        {
            channelName = channelName.Trim();
            if (channelName == "")
            {
                throw new Exception("Channel name empty. Please provide a channel name.");
            }

            this.ValidateChannelName(channelName);

            var response = this.DoHTTPRequest("POST", this.apiEndpoint + "/broadcasts", new Dictionary<string, object>
            {
                { "channel", channelName },
                { "data", data }
            });

            if ((int)response["status"] == 200)
            {
                return true;
            }
            else
            {
                throw new Exception("An error occurred while calling the API. Server returned: " + response["body"]);
            }
        }

        public string Auth(string channelName)
        {
            channelName = channelName.Trim();
            if (channelName == "")
            {
                throw new Exception("Channel name empty. Please provide a channel name.");
            }

            if (!channelName.StartsWith("private-"))
            {
                throw new Exception("Channel authentication can only be used with private channels.");
            }

            Dictionary<string, object> response = this.DoHTTPRequest("GET", this.apiEndpoint + "/channels/auth?channel=" + Uri.EscapeDataString(channelName),
                new Dictionary<string, object>());

            if ((int)response["status"] == 200)
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>((string)response["body"])["token"];
            }
            else
            {
                throw new Exception("There was a problem receiving a channel authentication token. Server returned: " + response["body"]);
            }

        }

        private Dictionary<string, object> DoHTTPRequest(string method, string url, Dictionary<string, object> data)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-PushRadar-Library", "pushradar-server-dotnet " + PushRadar.version);
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + this.secretKey);
            HttpRequestMessage request = new HttpRequestMessage(method.ToLower() == "post" ? HttpMethod.Post : HttpMethod.Get, url);

            if (method.ToLower() == "post")
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            }

            var task = Task.Run(() => client.SendAsync(request));
            task.Wait();

            var response = task.Result;

            var task2 = Task.Run(() => response.Content.ReadAsStringAsync());
            task2.Wait();

            return new Dictionary<string, object>
            {
                { "body", task2.Result },
                { "status", response.StatusCode }
            };
        }
    }
}