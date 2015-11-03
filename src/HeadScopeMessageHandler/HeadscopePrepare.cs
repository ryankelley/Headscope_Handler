using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace HeadScopeMessageHandler
{
    public class HeadscopePrepare
    {
        public HeadscopePrepare()
        {
            Target = new Uri("api/requests/{appKey}/messages", UriKind.Relative);
        }

        public Uri Target { get; private set; }

        public HeadScopeData HeadScopeData { get; set; }

        public string AppKey { get; set; }

        public HeadscopePrepare Update(string appKey, HeadScopeData headScopeData)
        {
            var clone = CloneData();
            clone.AppKey = appKey;
            clone.HeadScopeData = headScopeData;
            return clone;
        }

        private HeadscopePrepare CloneData()
        {
            return new HeadscopePrepare
            {
                Target = Target
            };
        }

        public HttpRequestMessage BuildPOSTRequest(string appKey, HeadScopeData headscopeData)
        {
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Target.OriginalString.Replace("{appKey}", appKey), UriKind.Relative),
                Content = headscopeData.ToHttpContent()
            };
            httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return httpRequestMessage;
        }

        public HttpRequestMessage BuildRequest()
        {
            return BuildPOSTRequest(AppKey, HeadScopeData);
        }
    }
}