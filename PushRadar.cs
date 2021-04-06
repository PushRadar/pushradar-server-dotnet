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
        private static readonly string version = "3.1.0";
        private readonly string apiEndpoint = "https://api.pushradar.com/v3";
        private string secretKey = null;

        public PushRadar(string secretKey)
        {
            if (secretKey == null || secretKey.Trim() == "" || !secretKey.StartsWith("sk_"))
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

        public async Task<bool> BroadcastAsync(string channelName, object data)
        {
            if (channelName == null || channelName.Trim() == "")
            {
                throw new Exception("Channel name empty. Please provide a channel name.");
            }

            this.ValidateChannelName(channelName);

            var response = await this.DoHTTPRequestAsync("POST", this.apiEndpoint + "/broadcasts", new Dictionary<string, object>
            {
                { "channel", channelName },
                { "data", JsonConvert.SerializeObject(data) }
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

        public string Auth(string channelName, string socketID)
        {
            if (channelName == null || channelName.Trim() == "")
            {
                throw new Exception("Channel name empty. Please provide a channel name.");
            }

            if (!(channelName.StartsWith("private-") || channelName.StartsWith("presence-")))
            {
                throw new Exception("Channel authentication can only be used with private and presence channels.");
            }

            if (socketID == null || socketID.Trim() == "")
            {
                throw new Exception("Socket ID empty. Please pass through a socket ID.");
            }

            var task = Task.Run(() => this.DoHTTPRequestAsync("GET", this.apiEndpoint + "/channels/auth?channel=" + Uri.EscapeDataString(channelName) + 
                "&socketID=" + Uri.EscapeDataString(socketID),
                new Dictionary<string, object>()));

            task.Wait();

            var response = task.Result;

            if ((int)response["status"] == 200)
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>((string)response["body"])["token"];
            }
            else
            {
                throw new Exception("There was a problem receiving a channel authentication token. Server returned: " + response["body"]);
            }

        }

        public async Task<bool> RegisterClientDataAsync(string socketID, object clientData)
        {
            if (socketID == null || socketID.Trim() == "")
            {
                throw new Exception("Socket ID empty. Please pass through a socket ID.");
            }

            var response = await this.DoHTTPRequestAsync("POST", this.apiEndpoint + "/client-data", new Dictionary<string, object>
            {
                { "socketID", socketID },
                { "clientData", JsonConvert.SerializeObject(clientData) }
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

        private async Task<Dictionary<string, object>> DoHTTPRequestAsync(string method, string url, Dictionary<string, object> data)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-PushRadar-Library", "pushradar-server-dotnet " + PushRadar.version);
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + this.secretKey);
            HttpRequestMessage request = new HttpRequestMessage(method.ToLower() == "post" ? HttpMethod.Post : HttpMethod.Get, url);

            if (method.ToLower() == "post")
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            }

            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            return new Dictionary<string, object>
            {
                { "body", body },
                { "status", response.StatusCode }
            };
        }
    }
}