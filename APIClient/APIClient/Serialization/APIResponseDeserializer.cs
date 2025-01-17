using System;
using System.Net.Http;
using System.Threading.Tasks;
using APIClient.Models;
using APIClient.Serialization.Interfaces;
using Newtonsoft.Json;

namespace APIClient.Serialization
{
    public class APIResponseDeserializer: IAPIResponseDeserializer
    {
        private readonly JsonSerializerSettings _jsonSettings;

        public APIResponseDeserializer(JsonSerializerSettings jsonSettings = null)
        {
            _jsonSettings = jsonSettings?? new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };
        }

        public async Task<APIResponse<T>> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var result = new APIResponse<T>
            {
                StatusCode = response.StatusCode,
                IsSuccess = response.IsSuccessStatusCode
            };

            foreach (var header in response.Headers)
            {
                result.Headers[header.Key] = string.Join(",", header.Value);
            }

            result.RawContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(result.RawContent))
            {
                try
                {
                    result.Data = JsonConvert.DeserializeObject<T>(result.RawContent, _jsonSettings);
                }
                catch (JsonException e)
                { 
                    result.IsSuccess = false;
                    result.ErrorMessage = $"Response deserialization failed: {e.Message}";
                }
            }
            else if (!response.IsSuccessStatusCode)
            {
                result.ErrorMessage = result.RawContent;
            }
            
            return result;
        }
    }
}