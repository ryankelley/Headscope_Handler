using System;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace HeadScopeMessageHandler
{
    public class HeadscopeRequest
    {
        private readonly JObject requestObject = new JObject();

        public HeadscopeRequest(HttpRequestMessage httpRequest)
        {
            if (httpRequest.RequestUri == null)
                throw new ArgumentException("Request Uri is required for a Headscope Request");
            requestObject["method"] = httpRequest.Method.ToString();
            requestObject["url"] = httpRequest.RequestUri.OriginalString;
            var jheaders = new JObject();
            HeadScopeData.AddHeaders(httpRequest.Headers, jheaders);
            if (httpRequest.Content != null)
            {
                HeadScopeData.AddHeaders(httpRequest.Content.Headers, jheaders);
                requestObject["body"] = httpRequest.Content.ReadAsStringAsync().Result;
            }
            if (!jheaders.Properties().Any())
                return;
            requestObject["headers"] = jheaders;
        }

        public JObject ToJObject()
        {
            return requestObject;
        }

        public void AddProperty(string key, string value)
        {
            requestObject[key] = value;
        }
    }
}