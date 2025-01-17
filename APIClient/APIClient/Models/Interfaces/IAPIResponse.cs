using System.Collections.Generic;
using System.Net;

namespace APIClient.Models.Interfaces
{
    public interface IAPIResponse
    {
        /// <summary>
        /// HTTP 상태 코드
        /// </summary>
        HttpStatusCode StatusCode { get; }
        
        /// <summary>
        /// 응답 헤더
        /// </summary>
        IDictionary<string, string> Headers { get; }
        
        /// <summary>
        /// 응답 본문 (원본)
        /// </summary>
        string RawContent { get; }
        
        /// <summary>
        /// 성공 여부
        /// </summary>
        bool IsSuccess { get; }
        
        /// <summary>
        /// 에러 메시지
        /// </summary>
        string ErrorMessage { get; }
    }
    
    public interface IAPIResponse<T> : IAPIResponse
    {
        /// <summary>
        /// 응답 본문 (역직렬화)
        /// </summary>
        T Data { get; }
    }
}