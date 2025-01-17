using System.Net.Http;
using APIClient.Authentication.Interface;

namespace APIClient.Authentication.Models
{
    public class CustomAuthentication : IAPIAuthentication
    {
        public AuthSchemeType SchemeType => AuthSchemeType.Custom;
        
        private readonly string _headerKey;
        private readonly string _headerValue;

        public CustomAuthentication(string headerKey, string headerValue)
        {
            _headerKey = headerKey;
            _headerValue = headerValue;
        }

        public void ApplyAuthentication(HttpRequestMessage request)
        {
            request.Headers.Add(_headerKey, _headerValue);
        }
    }
}