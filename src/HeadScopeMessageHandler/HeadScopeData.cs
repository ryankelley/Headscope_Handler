using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace HeadScopeMessageHandler
{
    public class HeadScopeData
    {
        private readonly IDictionary<string, string> _extraProperties;

        private static readonly HashSet<string> _SpaceDelimitedHeaders =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "User-Agent",
                "Server"
            };
        public HeadScopeData(IDictionary<string, string> extraProperties = null)
        {
            _extraProperties = extraProperties;
        }

        public HeadscopeRequest Request { get; set; }

        public HeadscopeResponse Response { get; set; }

        public HttpContent ToHttpContent()
        {
            var jsonObject = new JObject();
            if (Request != null)
                jsonObject["request"] = Request.ToJObject();
            if (this.Response != null)
                jsonObject["response"] = this.Response.ToJObject();

            if (_extraProperties != null && _extraProperties.Count > 0)
                foreach (var kvp in _extraProperties)
                    jsonObject[kvp.Key] = kvp.Value;


            var stringContent = new StringContent(jsonObject.ToString());
            stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return stringContent;
        }

        public static void AddHeaders(HttpHeaders httpHeaders, JObject jheaders)
        {
            foreach (var keyValuePair in httpHeaders)
            {
                if (keyValuePair.Value.Count() > 1)
                {
                    var separator = _SpaceDelimitedHeaders.Contains(keyValuePair.Key) ? " " : ", ";
                    jheaders.Add(new JProperty(keyValuePair.Key, string.Join(separator, keyValuePair.Value)));
                }
                else
                    jheaders.Add(new JProperty(keyValuePair.Key, keyValuePair.Value.First()));
            }
        }
    }
}