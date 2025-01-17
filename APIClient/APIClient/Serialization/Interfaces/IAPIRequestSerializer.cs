using System.Net.Http;
using APIClient.Models.Interfaces;

namespace APIClient.Serialization.Interfaces
{
    public interface IAPIRequestSerializer
    {
        /// <summary>
        /// HTTP 요청 메시지로 직렬화
        /// </summary>
        /// <param name="request"><see cref="IAPIRequest"/>: 요청 객체</param>
        /// <param name="baseUrl">기본 URL</param>
        /// <returns><see cref="HttpRequestMessage"/>: 직렬화된 요청 메시지</returns>
        HttpRequestMessage SerializeRequest(IAPIRequest request, string baseUrl);
    }
}