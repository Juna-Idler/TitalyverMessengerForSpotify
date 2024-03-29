﻿using System.Net;
using System.Text;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using SpotifyAPI.Web.Http;
using SpotifyAPI.Web;

namespace TitalyverMessengerForSpotify
{


//Request HeaderのAccept-Languageを設定するためだけのコピペクラス
    public class CustomHttpClient : IHTTPClient
    {
        public static string AcceptLanguage = "ja";

        private readonly HttpMessageHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;

        public CustomHttpClient()
        {
            _httpClient = new HttpClient();
        }

        public CustomHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public CustomHttpClient(IProxyConfig proxyConfig)
        {
//            Ensure.ArgumentNotNull(proxyConfig, nameof(proxyConfig));

            _httpMessageHandler = CreateMessageHandler(proxyConfig);
            _httpClient = new HttpClient(_httpMessageHandler);
        }

        public async Task<IResponse> DoRequest(IRequest request)
        {
//            Ensure.ArgumentNotNull(request, nameof(request));

            using HttpRequestMessage requestMsg = BuildRequestMessage(request);
            var responseMsg = await _httpClient
                    .SendAsync(requestMsg, HttpCompletionOption.ResponseContentRead)
                    .ConfigureAwait(false);

            return await BuildResponse(responseMsg).ConfigureAwait(false);
        }

        private static async Task<IResponse> BuildResponse(HttpResponseMessage responseMsg)
        {
//            Ensure.ArgumentNotNull(responseMsg, nameof(responseMsg));

            // We only support text stuff for now
            using var content = responseMsg.Content;
            var headers = responseMsg.Headers.ToDictionary(header => header.Key, header => header.Value.First());
            var body = await responseMsg.Content.ReadAsStringAsync().ConfigureAwait(false);
            var contentType = content.Headers?.ContentType?.MediaType;

            return new Response(headers)
            {
                ContentType = contentType,
                StatusCode = responseMsg.StatusCode,
                Body = body
            };
        }

        private static HttpRequestMessage BuildRequestMessage(IRequest request)
        {
//            Ensure.ArgumentNotNull(request, nameof(request));

            var fullUri = new Uri(request.BaseAddress, request.Endpoint).ApplyParameters(request.Parameters);
            var requestMsg = new HttpRequestMessage(request.Method, fullUri);
            foreach (var header in request.Headers)
            {
                requestMsg.Headers.Add(header.Key, header.Value);
            }
//
            requestMsg.Headers.Add("Accept-Language", AcceptLanguage);

            switch (request.Body)
            {
                case HttpContent body:
                    requestMsg.Content = body;
                    break;
                case string body:
                    requestMsg.Content = new StringContent(body, Encoding.UTF8, "application/json");
                    break;
                case Stream body:
                    requestMsg.Content = new StreamContent(body);
                    break;
            }

            return requestMsg;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
                _httpMessageHandler?.Dispose();
            }
        }

        public void SetRequestTimeout(TimeSpan timeout)
        {
            _httpClient.Timeout = timeout;
        }

        private static HttpMessageHandler CreateMessageHandler(IProxyConfig proxyConfig)
        {
            var proxy = new WebProxy
            {
                Address = new UriBuilder(proxyConfig.Host) { Port = proxyConfig.Port }.Uri,
                UseDefaultCredentials = true,
                BypassProxyOnLocal = proxyConfig.BypassProxyOnLocal
            };

            if (!string.IsNullOrEmpty(proxyConfig.User) || !string.IsNullOrEmpty(proxyConfig.Password))
            {
                proxy.UseDefaultCredentials = false;
                proxy.Credentials = new NetworkCredential(proxyConfig.User, proxyConfig.Password);
            }

            var httpClientHandler = new HttpClientHandler
            {
                PreAuthenticate = proxy.UseDefaultCredentials,
                UseDefaultCredentials = proxy.UseDefaultCredentials,
                UseProxy = true,
                Proxy = proxy,
            };
            if (proxyConfig.SkipSSLCheck)
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            }

            return httpClientHandler;
        }
    }
}