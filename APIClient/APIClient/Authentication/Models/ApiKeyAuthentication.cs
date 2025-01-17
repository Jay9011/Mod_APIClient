using System.Net.Http;
using APIClient.Authentication.Interface;

namespace APIClient.Authentication.Models
{
    public class ApiKeyAuthentication : IAPIAuthentication
    {
        public AuthSchemeType SchemeType => AuthSchemeType.ApiKey;

        private readonly string _apiKey;
        private readonly string _headerName;

        public ApiKeyAuthentication(string apiKey, string headerName = "X-API-KEY")
        {
            _apiKey = apiKey;
            _headerName = headerName;
        }

        public void ApplyAuthentication(HttpRequestMessage request)
        {
            request.Headers.Add(_headerName, _apiKey);
        }
    }
}