using System.Net.Http;
using APIClient.Authentication.Interface;

namespace APIClient.Authentication.Models
{
    public class CookieAuthentication : IAPIAuthentication
    {
        public AuthSchemeType SchemeType => AuthSchemeType.Cookie;
        
        private readonly string _cookieName;
        private readonly string _cookieValue;

        public CookieAuthentication(string cookieName, string cookieValue)
        {
            _cookieName = cookieName;
            _cookieValue = cookieValue;
        }

        public void ApplyAuthentication(HttpRequestMessage request)
        {
            request.Headers.Add("Cookie", $"{_cookieName}={_cookieValue}");
        }
    }
}