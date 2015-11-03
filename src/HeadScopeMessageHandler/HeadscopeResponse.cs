using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace HeadScopeMessageHandler
{
    public class HeadscopeResponse
    {
        private readonly JObject _innerResponse = new JObject();

        public HeadscopeResponse(HttpResponseMessage httpResponse)
        {
            _innerResponse["status"] = (int)httpResponse.StatusCode;
            var jheaders = new JObject();
            HeadScopeData.AddHeaders(httpResponse.Headers, jheaders);

            //NOTE: PHI ALERT - Need to be cautious if we were to send body
            if (httpResponse.Content != null)
            {
                HeadScopeData.AddHeaders((HttpHeaders)httpResponse.Content.Headers, jheaders);
                _innerResponse["body"] = (JToken)httpResponse.Content.ReadAsStringAsync().Result;
            }
            if (!Enumerable.Any<JProperty>(jheaders.Properties()))
                return;
            _innerResponse["headers"] = jheaders;
        }

        public JToken ToJObject()
        {
            return _innerResponse;
        }
    }
}