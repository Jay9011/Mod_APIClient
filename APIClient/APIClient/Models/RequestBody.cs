using System;
using System.Collections.Generic;
using APIClient.Models.Interfaces;

namespace APIClient.Models
{
    public class RequestBody
    {
        public RequestContentType ContentType { get; set; }
        public object Content { get; set; }
        public string RawContentType { get; set; }
        public IDictionary<string, string> FormData { get; set; }
        public IDictionary<string, byte[]> BinaryData { get; set; }
        public string GraphQLQuery { get; set; }
        public object GraphQLVariables { get; set; }

        public static RequestBody CreateJson(object content)
        {
            return new RequestBody { ContentType = RequestContentType.Json, Content = content };
        }
        
        public static RequestBody CreateFormData(IDictionary<string, string> formData)
        {
            return new RequestBody { ContentType = RequestContentType.FormData, FormData = formData };
        }
        
        public static RequestBody CreateUrlEncoded(IDictionary<string, string> formData)
        {
            return new RequestBody { ContentType = RequestContentType.UrlEncoded, FormData = formData };
        }

        public static RequestBody CreateRaw(string content, string contentType)
        {
            return new RequestBody { ContentType = RequestContentType.Raw, Content = content, RawContentType = contentType };
        }
        
        public static RequestBody CreateBinary(byte[] content)
        {
            return new RequestBody { ContentType = RequestContentType.Binary, Content = content };
        }
        
        public static RequestBody CreateGraphQL(string query, object variables = null)
        {
            return new RequestBody { ContentType = RequestContentType.GraphQL, GraphQLQuery = query, GraphQLVariables = variables };
        }
    }
}