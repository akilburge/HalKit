﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HalKit.Http;
using HalKit.Models;
using HalKit.Resources;
using HalKit.Services;

namespace HalKit
{
    public class HalClient : IHalClient
    {
        private const string HalJsonMediaType = "application/hal+json";

        private readonly IHttpConnection _httpConnection;
        private readonly IHalKitConfiguration _configuration;
        private readonly ILinkResolver _linkResolver;

        public HalClient(IHalKitConfiguration configuration)
            : this(new HttpConnection(new DelegatingHandler[] { }, configuration),
                   configuration,
                   new LinkResolver())
        {
        }

        public HalClient(IHttpConnection httpConnection,
                         IHalKitConfiguration configuration,
                         ILinkResolver linkResolver)
        {
            Requires.ArgumentNotNull(httpConnection, "httpConnection");
            Requires.ArgumentNotNull(configuration, "configuration");
            Requires.ArgumentNotNull(linkResolver, "linkResolver");
            if (configuration.RootEndpoint == null)
            {
                throw new ArgumentException("configuration must have a RootEndpoint");
            }

            _httpConnection = httpConnection;
            _configuration = configuration;
            _linkResolver = linkResolver;
        }

        public Task<RootResource> GetRootAsync()
        {
            return GetRootAsync(new Dictionary<string, string>());
        }

        public Task<RootResource> GetRootAsync(IDictionary<string, string> parameters)
        {
            return GetRootAsync(parameters, new Dictionary<string, IEnumerable<string>>());
        }

        public Task<RootResource> GetRootAsync(
            IDictionary<string, string> parameters,
            IDictionary<string, IEnumerable<string>> headers)
        {
            return GetAsync<RootResource>(
                new Link {HRef = _configuration.RootEndpoint.OriginalString},
                parameters,
                headers);
        }

        public Task<T> GetAsync<T>(Link link)
        {
            return GetAsync<T>(link, new Dictionary<string, string>());
        }

        public Task<T> GetAsync<T>(Link link, IDictionary<string, string> parameters)
        {
            return GetAsync<T>(link, parameters, new Dictionary<string, IEnumerable<string>>());
        }

        public Task<T> GetAsync<T>(
            Link link,
            IDictionary<string, string> parameters,
            IDictionary<string, IEnumerable<string>> headers)
        {
            return SendRequestAndGetBodyAsync<T>(
                link,
                HttpMethod.Get,
                null,
                parameters,
                headers);
        }

        public Task<T> PostAsync<T>(Link link, object body)
        {
            return PostAsync<T>(link, body, new Dictionary<string, string>());
        }

        public Task<T> PostAsync<T>(Link link, object body, IDictionary<string, string> parameters)
        {
            return PostAsync<T>(link, body, parameters, new Dictionary<string, IEnumerable<string>>());
        }

        public Task<T> PostAsync<T>(
            Link link,
            object body,
            IDictionary<string, string> parameters,
            IDictionary<string, IEnumerable<string>> headers)
        {
            return SendRequestAndGetBodyAsync<T>(
                link,
                HttpMethod.Post,
                body,
                parameters,
                headers);
        }

        public Task<T> PutAsync<T>(Link link, object body)
        {
            return PutAsync<T>(link, body, new Dictionary<string, string>());
        }

        public Task<T> PutAsync<T>(Link link, object body, IDictionary<string, string> parameters)
        {
            return PutAsync<T>(link, body, parameters, new Dictionary<string, IEnumerable<string>>());
        }

        public Task<T> PutAsync<T>(
            Link link,
            object body,
            IDictionary<string, string> parameters,
            IDictionary<string, IEnumerable<string>> headers)
        {
            return SendRequestAndGetBodyAsync<T>(
                link,
                HttpMethod.Put,
                body,
                parameters,
                headers);
        }

        public Task<T> PatchAsync<T>(Link link, object body)
        {
            return PatchAsync<T>(link, body, new Dictionary<string, string>());
        }

        public Task<T> PatchAsync<T>(Link link, object body, IDictionary<string, string> parameters)
        {
            return PatchAsync<T>(link, body, parameters, new Dictionary<string, IEnumerable<string>>());
        }

        public Task<T> PatchAsync<T>(
            Link link,
            object body,
            IDictionary<string, string> parameters,
            IDictionary<string, IEnumerable<string>> headers)
        {
            return SendRequestAndGetBodyAsync<T>(
                link,
                new HttpMethod("Patch"),
                body,
                parameters,
                headers);
        }

        public Task<IApiResponse> DeleteAsync(Link link)
        {
            return DeleteAsync(link, new Dictionary<string, string>());
        }

        public Task<IApiResponse> DeleteAsync(Link link, IDictionary<string, string> parameters)
        {
            return DeleteAsync(link, parameters, new Dictionary<string, IEnumerable<string>>());
        }

        public async Task<IApiResponse> DeleteAsync(
            Link link,
            IDictionary<string, string> parameters,
            IDictionary<string, IEnumerable<string>> headers)
        {
            var response = await SendRequestAsync<object>(
                            link,
                            HttpMethod.Delete,
                            null,
                            parameters,
                            headers);
            return response;
        }

        public IHalKitConfiguration Configuration
        {
            get { return _configuration; }
        }

        public IHttpConnection HttpConnection
        {
            get { return _httpConnection; }
        }

        private async Task<T> SendRequestAndGetBodyAsync<T>(
            Link link,
            HttpMethod method,
            object body,
            IDictionary<string, string> parameters,
            IDictionary<string, IEnumerable<string>> headers)
        {
            var response = await SendRequestAsync<T>(link, method, body, parameters, headers);
            return response.BodyAsObject;
        }

        private async Task<IApiResponse<T>> SendRequestAsync<T>(
            Link link,
            HttpMethod method,
            object body,
            IDictionary<string, string> parameters,
            IDictionary<string, IEnumerable<string>> headers)
        {
            Requires.ArgumentNotNull(link, "link");
            Requires.ArgumentNotNull(method, "method");

            headers = headers ?? new Dictionary<string, IEnumerable<string>>();
            if (!headers.ContainsKey("Accept"))
            {
                headers.Add("Accept", new[] { HalJsonMediaType });
            }

            return await _httpConnection.SendRequestAsync<T>(
                        _linkResolver.ResolveLink(link, parameters),
                        method,
                        body,
                        headers).ConfigureAwait(_configuration);
        }
    }
}