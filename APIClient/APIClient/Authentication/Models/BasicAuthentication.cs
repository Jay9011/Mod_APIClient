using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using APIClient.Authentication.Interface;

namespace APIClient.Authentication.Models
{
    public class BasicAuthentication : IAPIAuthentication
    {
        public AuthSchemeType SchemeType => AuthSchemeType.Basic;

        private readonly string _credential;
        
        public BasicAuthentication(string credential)
        {
            _credential = credential;
        }

        public static BasicAuthentication FromUserPass(string username, string password)
        {
            var credential = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            return new BasicAuthentication(credential);
        }

        public void ApplyAuthentication(HttpRequestMessage request)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _credential);
        }
    }
}