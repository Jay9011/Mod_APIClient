using System.Collections.Generic;

namespace APIClient.Configuration.Interfaces
{
    public interface IAPIConnectionInfo
    {
        /// <summary>
        /// API 기본 URL
        /// </summary>
        string BaseUrl { get; }
        /// <summary>
        /// REST API 엔드포인트 
        /// </summary>
        Dictionary<string, string> Endpoints { get; }
        /// <summary>
        /// REST API 엔드포인트 반환
        /// </summary>
        /// <param name="endpointName">엔드포인트 구분자 이름</param>
        /// <returns><see cref="string"/>: Endpoint</returns>
        string GetEndpoint(string endpointName);
    }
}