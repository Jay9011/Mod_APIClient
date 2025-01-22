using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using APIClient.Models;
using APIClient.Models.Interfaces;
using APIClient.Serialization.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace APIClient.Serialization
{
    public class APIRequestSerializer : IAPIRequestSerializer
    {
        private readonly JsonSerializerSettings _jsonSettings;
        private static readonly Regex PathParamPattern = new Regex(@"\{([^{}]+)\}", RegexOptions.Compiled);

        private static readonly Dictionary<RequestContentType, ContentTypeInfo> ContentTypeInfos =
            new Dictionary<RequestContentType, ContentTypeInfo>
            {
                [RequestContentType.Json] = new ContentTypeInfo { MediaType = "application/json" },
                [RequestContentType.FormData] = new ContentTypeInfo { MediaType = "multipart/form-data" },
                [RequestContentType.UrlEncoded] = new ContentTypeInfo { MediaType = "application/x-www-form-urlencoded" },
                [RequestContentType.Binary] = new ContentTypeInfo { MediaType = "application/octet-stream", CharSet = null },
                [RequestContentType.GraphQL] = new ContentTypeInfo { MediaType = "application/graphql" }
            };
        
        public APIRequestSerializer(JsonSerializerSettings jsonSettings = null)
        {
            _jsonSettings = jsonSettings?? new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = null
                }
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
                httpRequest.Content = CreateRequestContent(request.Body);
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

            foreach (var param in parameters)
            {
                endpoint = endpoint.Replace($"{{{param.Key}}}", param.Value);
            }
        }
        
        /// <summary>
        /// <see cref="RequestBody"/> 타입에 따라 콘텐츠 생성
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        private HttpContent CreateRequestContent(object body)
        {
            if (body is RequestBody requestBody)
            {
                return CreateContentByType(requestBody);
            }
            
            return CreateJsonContent(body);
        }

        /// <summary>
        /// <see cref="RequestContentType"/> ContentType에 따라 콘텐츠 생성
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        private HttpContent CreateContentByType(RequestBody body)
        {
            switch (body.ContentType)
            {
                case RequestContentType.Raw:
                    return CreateRawContent(body.Content?.ToString(), body.RawContentType);
                case RequestContentType.FormData:
                    return CreateFormDataContent(body.FormData, body.BinaryData);
                case RequestContentType.UrlEncoded:
                    return CreateUrlEncodedContent(body.FormData);
                case RequestContentType.XML:
                    break;
                case RequestContentType.Json:
                    return CreateJsonContent(body.Content);
                case RequestContentType.Binary:
                    return CreateBinaryContent(body.Content as byte[]);
                case RequestContentType.Multipart:
                    break;
                case RequestContentType.GraphQL:
                    return CreateGraphQLContent(body.GraphQLQuery, body.GraphQLVariables);
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported content type: {body.ContentType}");
            }

            return null;
        }

        /// <summary>
        /// Raw 콘텐츠 생성
        /// </summary>
        /// <param name="content"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        private StringContent CreateRawContent(string content, string contentType)
        {
            return new StringContent(content ?? "", Encoding.UTF8, contentType);
        }

        /// <summary>
        /// Json 콘텐츠 생성
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private StringContent CreateJsonContent(object content)
        {
            var json = JsonConvert.SerializeObject(content, _jsonSettings);
            return new StringContent(json, Encoding.UTF8, ContentTypeInfos[RequestContentType.Json].MediaType);
        }
        
        /// <summary>
        /// Multipart Form Data 콘텐츠 생성
        /// </summary>
        /// <param name="formData"></param>
        /// <param name="binaryData"></param>
        /// <returns></returns>
        private MultipartFormDataContent CreateFormDataContent(IDictionary<string,string> formData, IDictionary<string,byte[]> binaryData = null)
        {
            var content = new MultipartFormDataContent();

            if (formData != null)
            {
                foreach (var pair in formData)
                {
                    content.Add(new StringContent(pair.Value), pair.Key);
                }
            }

            if (binaryData != null)
            {
                foreach (var pair in binaryData)
                {
                    var fileContent = new ByteArrayContent(pair.Value);
                    content.Add(fileContent, pair.Key, pair.Key);
                }
            }

            return content;
        }

        /// <summary>
        /// Binary 콘텐츠 생성
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private ByteArrayContent CreateBinaryContent(byte[] content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var byteContent = new ByteArrayContent(content);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(ContentTypeInfos[RequestContentType.Binary].MediaType);

            return byteContent;
        }
        
        /// <summary>
        /// URL Encoded 콘텐츠 생성
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        private HttpContent CreateUrlEncodedContent(IDictionary<string, string> formData)
        {
            return new FormUrlEncodedContent(formData ?? new Dictionary<string, string>());
        }
        
        /// <summary>
        /// GraphQL 콘텐츠 생성
        /// </summary>
        /// <param name="query"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        private StringContent CreateGraphQLContent(string query, object variables)
        {
            var graphQlRequest = new
            {
                query = query,
                variables = variables
            };

            return CreateJsonContent(graphQlRequest);
        }
    }
}