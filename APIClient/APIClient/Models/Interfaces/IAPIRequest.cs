using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using APIClient.Authentication.Interface;

namespace APIClient.Models.Interfaces
{
    public interface IAPIRequest
    {
        /// <summary>
        /// HTTP 메서드
        /// </summary>
        HttpMethod Method { get; }
        
        /// <summary>
        /// API 엔드포인트 (상대경로)
        /// </summary>
        string Endpoint { get; }
        
        /// <summary>
        /// 인증 정보
        /// </summary>
        IAPIAuthentication Authentication { get; }
        
        /// <summary>
        /// 쿼리 파라미터
        /// </summary>
        IDictionary<string, string> QueryParameters { get; }
        
        /// <summary>
        /// 경로 파라미터
        /// </summary>
        IDictionary<string, string> PathParameters { get; }
        
        /// <summary>
        /// 요청 헤더
        /// </summary>
        IDictionary<string, string> Headers { get; }
        
        /// <summary>
        /// 요청 부분
        /// </summary>
        object Body { get; }
        
        /// <summary>
        /// 요청 타임아웃 (밀리초)
        /// </summary>
        int TimeoutMilliseconds { get; }
        
        /// <summary>
        /// 취소 토큰
        /// </summary>
        CancellationToken CancellationToken { get; }
        
        /// <summary>
        /// 재시도 횟수
        /// </summary>
        int RetryCount { get; }
    }
}