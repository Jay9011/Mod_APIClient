using System.Collections.Generic;
using System.Net;
using APIClient.Models.Interfaces;

namespace APIClient.Models
{
    public class APIResponse<T> : IAPIResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string RawContent { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public T Data { get; set; }
        
        /// <summary>
        /// 성공 응답 생성
        /// </summary>
        /// <param name="data"><see cref="T"/>반환시 변환할 타입 데이터</param>
        /// <param name="statusCode">응답 상태 코드</param>
        /// <returns><see cref="APIResponse{T}"/>: 성공 응답. 응답을 T 형태로 반환</returns>
        public static APIResponse<T> CreateSuccess(T data, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new APIResponse<T>
            {
                StatusCode = statusCode,
                IsSuccess = true,
                Data = data
            };
        }
        
        /// <summary>
        /// 에러 응답 생성
        /// </summary>
        /// <param name="errorMessage">에러 메시지</param>
        /// <param name="statusCode">응답 상태 코드</param>
        /// <returns><see cref="APIResponse{T}"/>: 에러 응답. 에러 메시지를 반환</returns>
        public static APIResponse<T> CreateError(string errorMessage, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            return new APIResponse<T>
            {
                StatusCode = statusCode,
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}