using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using APIClient.Client.Interfaces;
using APIClient.Client.Retry;
using APIClient.Configuration.Interfaces;
using APIClient.Models;
using APIClient.Models.Interfaces;
using APIClient.Serialization;
using APIClient.Serialization.Interfaces;

namespace APIClient.Client
{
    public class CoreAPIClient : ICoreAPI, IDisposable
    {
        private readonly IAPISetup _apiSetup;
        private readonly HttpClient _httpClient;
        private readonly IAPIRequestSerializer _requestSerializer;
        private readonly IAPIResponseDeserializer _responseDeserializer;
        private readonly IRetryPolicy _retryPolicy;
        private bool _disposed;

        public CoreAPIClient(
            IAPISetup apiSetup,  
            IAPIRequestSerializer requestSerializer = null, 
            IAPIResponseDeserializer responseDeserializer = null,
            HttpClient httpClient = null, 
            IRetryPolicy retryPolicy = null)
        {
            _apiSetup = apiSetup ?? throw new ArgumentNullException(nameof(apiSetup));
            _httpClient = httpClient ?? new HttpClient();
            _requestSerializer = requestSerializer ?? new APIRequestSerializer();
            _responseDeserializer = responseDeserializer ?? new APIResponseDeserializer();
            _retryPolicy = retryPolicy ?? RetryPolicy.Default;
        }

        public Task<APIResponse<T>> GetAsync<T>(string endpointName, IAPIRequest parameters = null)
        {
            var request = CreateRequest(endpointName, HttpMethod.Get, parameters);
            return SendAsync<T>(request);
        }

        public Task<APIResponse<T>> PostAsync<T>(string endpointName, IAPIRequest parameters = null)
        {
            var request = CreateRequest(endpointName, HttpMethod.Post, parameters);
            return SendAsync<T>(request);
        }

        public Task<APIResponse<T>> PutAsync<T>(string endpointName, IAPIRequest parameters = null)
        {
            var request = CreateRequest(endpointName, HttpMethod.Put, parameters);
            return SendAsync<T>(request);
        }

        public Task<APIResponse<T>> DeleteAsync<T>(string endpointName, IAPIRequest parameters = null)
        {
            var request = CreateRequest(endpointName, HttpMethod.Delete, parameters);
            return SendAsync<T>(request);
        }

        public async Task<APIResponse<T>> SendAsync<T>(APIRequest request)
        {
            CheckDisposed();

            try
            {
                var connectionInfo = _apiSetup.GetConnectionInfo();
                var httpRequest = _requestSerializer.SerializeRequest(request, connectionInfo.BaseUrl);
                
                using (var cts = new CancellationTokenSource(request.TimeoutMilliseconds))
                {
                    using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, request.CancellationToken))
                    {
                        var response = await _retryPolicy.ExecuteAsync(
                            async () => await _httpClient.SendAsync(httpRequest, linkedCts.Token));

                        return await _responseDeserializer.DeserializeResponse<T>(response);
                    }
                }
            }
            catch (OperationCanceledException e) when(!request.CancellationToken.IsCancellationRequested)
            {
                return APIResponse<T>.CreateError("Request timed out", System.Net.HttpStatusCode.RequestTimeout);
            }
            catch (Exception e)
            {
                return APIResponse<T>.CreateError($"Request failed: {e.Message}", System.Net.HttpStatusCode.InternalServerError);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                }

                _disposed = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private APIRequest CreateRequest(string endpointName, HttpMethod method, IAPIRequest apiRequest)
        {
            CheckDisposed();

            var connectionInfo = _apiSetup.GetConnectionInfo();
            var endpoint = connectionInfo.GetEndpoint(endpointName);

            var request = new APIRequest
            {
                Method = method,
                Endpoint = endpoint,
                TimeoutMilliseconds = apiRequest?.TimeoutMilliseconds ?? 30000,
                CancellationToken = apiRequest?.CancellationToken ?? CancellationToken.None,
                RetryCount = apiRequest?.RetryCount ?? 3
            };

            if (apiRequest != null)
            {
                if (apiRequest.Headers?.Count > 0)
                {
                    foreach (var header in apiRequest.Headers)
                    {
                        request.Headers[header.Key] = header.Value;
                    }
                }

                if (apiRequest.QueryParameters?.Count > 0)
                {
                    foreach (var query in apiRequest.QueryParameters)
                    {
                        request.QueryParameters[query.Key] = query.Value;
                    }
                }

                if (apiRequest.PathParameters?.Count > 0)
                {
                    foreach (var path in apiRequest.PathParameters)
                    {
                        request.PathParameters[path.Key] = path.Value;
                    }
                }

                request.Body = apiRequest.Body;
                request.Authentication = apiRequest.Authentication;
            }

            return request;
        }
        
        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(CoreAPIClient));
            }
        }
    }
}