using System.Net.Http;
using System.Threading.Tasks;
using APIClient.Models;

namespace APIClient.Serialization.Interfaces
{
    public interface IAPIResponseDeserializer
    {
        /// <summary>
        /// HTTP 응답 데이터 역직렬화
        /// </summary>
        /// <param name="response"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<APIResponse<T>> DeserializeResponse<T>(HttpResponseMessage response);
    }
}