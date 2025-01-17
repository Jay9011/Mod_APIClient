using System.Net.Http;
using System.Net.Http.Headers;
using APIClient.Authentication.Interface;

namespace APIClient.Authentication.Models
{
    public class BearerTokenAuthentication : IAPIAuthentication
    {
        private readonly string _token;

        public AuthSchemeType SchemeType => AuthSchemeType.Bearer;

        public BearerTokenAuthentication(string token)
        {
            _token = token;
        }
        
        public void ApplyAuthentication(HttpRequestMessage request)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }
    }
}