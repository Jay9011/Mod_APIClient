using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using APIClient.Models.Interfaces;
using APIClient.Serialization.Interfaces;
using Newtonsoft.Json;

namespace APIClient.Serialization
{
    public class APIRequestSerializer : IAPIRequestSerializer
    {
        private readonly JsonSerializerSettings _jsonSettings;
        private static readonly Regex PathParamPattern = new Regex(@"\{([^{}]+)\}", RegexOptions.Compiled);

        public APIRequestSerializer(JsonSerializerSettings jsonSettings = null)
        {
            _jsonSettings = jsonSettings?? new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };
        }

        public HttpRequestMessage SerializeRequest(IAPIRequest request, string baseUrl)
        {
            var endpoint = request.Endpoint;
            
            ValidateAndReplacePathParameters(ref endpoint, request.PathParameters);

            // 쿼리 파라미터가 있으면 추가
            if (request.QueryParameters?.Any() == true)
            {
                var query = string.Join("&", request.QueryParameters
                    .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
                
                endpoint += $"?{query}";
            }
            
            var url = $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
            
            var httpRequest = new HttpRequestMessage(request.Method, url);

            if (request.Headers?.Any() == true)
            {
                foreach (var header in request.Headers)
                {
                    httpRequest.Headers.Add(header.Key, header.Value);
                }
            }
            
            // 인증 정보 적용
            request.Authentication?.ApplyAuthentication(httpRequest);

            if (request.Body != null && request.Method != HttpMethod.Get)
            {
                var json = JsonConvert.SerializeObject(request.Body, _jsonSettings);
                httpRequest.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            }

            return httpRequest;
        }

        /// <summary>
        /// 경로 파라미터를 검증하고 치환한다.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="parameters"><see cref="Dictionary{TKey,TValue}"/></param>
        /// <exception cref="APIRequestSerializationException">: 직렬화 예외</exception>
        private void ValidateAndReplacePathParameters(ref string endpoint, IDictionary<string,string> parameters)
        {
            // 엔드포인트에서 모든 경로 파라미터를 추출
            var requiredParams = PathParamPattern.Matches(endpoint)
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ToList();
            
            // 필수 파라미터가 없으면 예외 발생
            if (parameters == null && requiredParams.Any())
            {
                throw new APIRequestSerializationException($"Path parameters are required but none were provided. Required parameters: {string.Join(", ", requiredParams)}",
                    requiredParams);
            }
            
            // 누락된 파라미터 체크후 없으면 예외 발생
            var missingParams = requiredParams
                .Where(param => !parameters.ContainsKey(param))
                .ToList();
            
            if (missingParams.Any())
            {
                throw new APIRequestSerializationException(
                    $"Missing required path parameters: {string.Join(", ", missingParams)}",
                    missingParams);
            }
            
            // 경로 파라미터를 실제 값으로 치환하고 남은 파라미터가 있는지 확인 후 예외 발생
            var remainingParams = PathParamPattern.Matches(endpoint)
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ToList();

            if (remainingParams.Any())
            {
                throw new APIRequestSerializationException(
                    $"Path parameters remain after substitution: {string.Join(", ", remainingParams)}",
                    remainingParams);
            }
        }
    }
}