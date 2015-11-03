using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace HeadScopeMessageHandler
{
    public class HeadscopeMessageHandler : DelegatingHandler
    {
        public static Dictionary<string, long> _tenantIds = new Dictionary<string, long>();
        private readonly Func<HttpRequestMessage, IDictionary<string, string>> _addExtraAttributes;
        private readonly Func<HttpRequestMessage, HttpResponseMessage, string> _requestFilter;
        private readonly string _appKey;
        private readonly HttpClient _headscopeClient;

        /// <summary>
        ///     Initializes a new HeadscopeMessageHandler
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="baseUrl"></param>
        /// <param name="appKey"></param>
        /// <param name="requestFilter"></param>
        /// <param name="addExtraAttributes"></param>
        public HeadscopeMessageHandler(string apiKey, string appKey, string baseUrl,
            Func<HttpRequestMessage, HttpResponseMessage, string> requestFilter = null,
            Func<HttpRequestMessage, IDictionary<string, string>> addExtraAttributes = null)
        {
            if(string.IsNullOrEmpty(baseUrl))
                throw new ArgumentNullException(nameof(baseUrl), "baseUrl is required to make requests");


            _appKey = appKey;
            _requestFilter = requestFilter;
            _addExtraAttributes = addExtraAttributes;
            _headscopeClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
            _headscopeClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", apiKey);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var headscopeRequest = new HeadscopeRequest(request);
            var response = await base.SendAsync(request, cancellationToken);
            HttpResponseMessage httpResponseMessage;
            try
            {
                var app = _appKey;
                if (_requestFilter != null)
                    app = _requestFilter(request, response);

                if (string.IsNullOrWhiteSpace(app))
                {
                    httpResponseMessage = response;
                }
                else
                {
                    var data = _addExtraAttributes != null ? new HeadScopeData(_addExtraAttributes(request)) : new HeadScopeData();
                    data.Request = headscopeRequest;
                    data.Response = new HeadscopeResponse(response);

                    ExectaskIgnoringFails(_headscopeClient.SendAsync(new HeadscopePrepare().Update(_appKey, data).BuildRequest()));
                    httpResponseMessage = response;
                }

            }
            catch (Exception)
            {
                httpResponseMessage = response;
                // Swallow all exceptions here, because we don't care if something goes wrong
            }
            return httpResponseMessage;
        }

        private void ExectaskIgnoringFails(Task<HttpResponseMessage> task)
        {
            int num;
            task.ContinueWith((Action<Task<HttpResponseMessage>>)(delegate (Task<HttpResponseMessage> t)
            {
                num = t.IsFaulted ? 1 : 0;
            }));
        }
    }
}
