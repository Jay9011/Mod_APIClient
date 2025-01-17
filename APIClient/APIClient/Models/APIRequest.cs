using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using APIClient.Authentication.Interface;
using APIClient.Models.Interfaces;

namespace APIClient.Models
{
    public class APIRequest : IAPIRequest
    {
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public string Endpoint { get; set; }
        public IAPIAuthentication Authentication { get; set; }
        public IDictionary<string, string> QueryParameters { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, string> PathParameters { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public object Body { get; set; }
        public int TimeoutMilliseconds { get; set; } = 30000;
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
        public int RetryCount { get; set; } = 3;
        
        /// <summary>
        /// API 요청 생성
        /// </summary>
        /// <param name="endpoint">API 엔드포인트 (상대경로)</param>
        /// <param name="method">HTTP 메서드</param>
        /// <param name="body">요청 본문</param>
        /// <param name="authentication"><see cref="IAPIAuthentication"/>: 인증 정보</param>
        /// <returns><see cref="APIRequest"/>: API 요청</returns>
        public static APIRequest Create(string endpoint, HttpMethod method = null, object body = null, IAPIAuthentication authentication = null)
        {
            return new APIRequest
            {
                Endpoint = endpoint,
                Method = method ?? HttpMethod.Get,
                Body = body,
                Authentication = authentication
            };
        }
        
        /// <summary>
        /// API 요청에 쿼리 파라미터 추가
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns><see cref="APIRequest"/>: API 요청</returns>
        public APIRequest AddQueryParameter(string key, string value)
        {
            QueryParameters[key] = value;
            return this;
        }
        
        /// <summary>
        /// API 요청에 경로 파라미터 추가
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns><see cref="APIRequest"/>: API 요청</returns>
        public APIRequest AddPathParameter(string key, string value)
        {
            PathParameters[key] = value;
            return this;
        }
        
        /// <summary>
        /// API 요청에 헤더 추가
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns><see cref="APIRequest"/>: API 요청</returns>
        public APIRequest AddHeader(string key, string value)
        {
            Headers[key] = value;
            return this;
        }
        
        /// <summary>
        /// API 요청에 인증 정보 추가
        /// </summary>
        /// <param name="authentication"><see cref="IAPIAuthentication"/>: 인증 정보</param>
        /// <returns><see cref="APIRequest"/>: API 요청</returns>
        public APIRequest WithAuthentication(IAPIAuthentication authentication)
        {
            Authentication = authentication;
            return this;
        }
        
        /// <summary>
        /// API 요청에 타임아웃 추가
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns><see cref="APIRequest"/>: API 요청</returns>
        public APIRequest WithTimeout(int milliseconds)
        {
            TimeoutMilliseconds = milliseconds;
            return this;
        }
        
        /// <summary>
        /// API 요청에 재시도 횟수 추가
        /// </summary>
        /// <param name="count"></param>
        /// <returns><see cref="APIRequest"/>: API 요청</returns>
        public APIRequest WithRetryCount(int count)
        {
            RetryCount = count;
            return this;
        }
    }
}