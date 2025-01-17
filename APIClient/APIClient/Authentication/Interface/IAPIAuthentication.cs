using System.Net.Http;
using APIClient.Authentication.Models;

namespace APIClient.Authentication.Interface
{
    public interface IAPIAuthentication
    {
        AuthSchemeType SchemeType { get; }
        /// <summary>
        /// 인증 정보를 request에 적용
        /// </summary>
        /// <param name="request"><see cref="HttpRequestMessage"/>: 리퀘스트</param>
        void ApplyAuthentication(HttpRequestMessage request);
    }
}