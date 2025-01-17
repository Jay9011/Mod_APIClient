using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using APIClient.Authentication.Interface;
using APIClient.Models.Interfaces;

namespace APIClient.Models
{
    public class APIRequestParameters : IAPIRequest
    {
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public string Endpoint { get; set; }
        public IAPIAuthentication Authentication { get; set; }
        public IDictionary<string, string> QueryParameters { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, string> PathParameters { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public object Body { get; set; }
        public int TimeoutMilliseconds { get; set; } = 30000;
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
        public int RetryCount { get; set; } = 3;
    }
}