using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;

namespace StartshipsStops.web.api
{
    public class ApiClient
    {
        public string Url { get; set; }
        public ApiClient(string url)
        {
            this.Url = url;
        }

        public string GetApiResponse(string method, Dictionary<string, string> body = null, Dictionary<string, string> headers = null)
        {
            using (var httpClient = new HttpClient())
            {
                Uri uri = new Uri(string.Format("{0}/{1}", this.Url, method));
                var req = new HttpRequestMessage(HttpMethod.Get, uri);
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        req.Headers.Add(item.Key, item.Value);
                    }
                }
                if (body != null)
                {
                    req.Content = new FormUrlEncodedContent(body);
                }


                var response = httpClient.GetStringAsync(uri).Result;//httpClient.SendAsync(req).Result.ToString();

                return response;
            }
        }
   
    }
}
