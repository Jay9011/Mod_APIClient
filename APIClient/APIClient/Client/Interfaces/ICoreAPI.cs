using System.Threading.Tasks;
using APIClient.Models;
using APIClient.Models.Interfaces;

namespace APIClient.Client.Interfaces
{
    public interface ICoreAPI
    {
        /// <summary>
        /// HTTP GET 요청
        /// </summary>
        Task<APIResponse<T>> GetAsync<T>(string endpointName, IAPIRequest parameters = null);

        /// <summary>
        /// HTTP POST 요청
        /// </summary>
        Task<APIResponse<T>> PostAsync<T>(string endpointName, IAPIRequest parameters = null);

        /// <summary>
        /// HTTP PUT 요청
        /// </summary>
        Task<APIResponse<T>> PutAsync<T>(string endpointName, IAPIRequest parameters = null);

        /// <summary>
        /// HTTP DELETE 요청
        /// </summary>
        Task<APIResponse<T>> DeleteAsync<T>(string endpointName, IAPIRequest parameters = null);

        /// <summary>
        /// 일반 HTTP 요청
        /// </summary>
        Task<APIResponse<T>> SendAsync<T>(APIRequest request);
    }
}