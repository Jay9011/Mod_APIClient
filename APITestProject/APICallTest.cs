using APIClient.Authentication.Models;
using APIClient.Client;
using APIClient.Client.Interfaces;
using APIClient.Configuration;
using APIClient.Configuration.Interfaces;
using APIClient.Configuration.Models;
using APIClient.Extentions;
using APIClient.Models;
using APIClient.Models.Interfaces;
using FileIOHelper;
using FileIOHelper.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace APITestProject
{
    public class APICallTest
    {
        private IIOHelper _ioHelper;
        private string SectionName = "SECUiDEA_API";
        private string WebSectionName = "Web_API";
        
        public APICallTest()
        {
            _ioHelper = new RegistryHelper("SoftWare\\SECUiDEA\\APITest");
            SetupTestAPI();
        }

        public void SetupTestAPI()
        {
            var connectionInfo = new APIConnectionInfo
            {
                BaseUrl = "http://192.168.0.63:2763",    // SECUiDEA API
                // BaseUrl = "http://localhost:5070/",     // Local Test API
                Endpoints = new Dictionary<string, string>
                {
                    ["GetTokenSECU"] = "/CommonAPIv1/GetTokenSECU",
                    ["GetVisitInfoList"] = "/CommonAPIv1/GetVisitInfoList",
                    ["SetVisitReserveInfo"] = "/CommonAPIv1/SetVisitReserveInfo"
                }
            };
            
            var apiSetup = new APISetup(_ioHelper, SectionName);
            apiSetup.UpdateConnectionInfo(connectionInfo);

            var connectionInfo2 = new APIConnectionInfo
            {
                // BaseUrl = "http://192.168.0.63:2751", // SECUiDEA Web API
                BaseUrl = "http://localhost:5287/", // Local Test Web API
                Endpoints = new Dictionary<string, string>
                {
                    ["GetVisitInfoList"] = "/Visit/GetVisitInfoList"
                }
            };
            
            var webApiSetup = new APISetup(_ioHelper, WebSectionName);
            webApiSetup.UpdateConnectionInfo(connectionInfo2);
        }
        
        [Fact]
        public async Task GetTokenTest()
        {
            // Act
            var response = await GetAPIKey();
            
            // Assert
            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Data);
            Assert.True(response.Data.ErrorCode == "0000");
        }
        
        [Fact]
        public async Task GetVisitInfoListTest()
        {
            // Arrange
            var response = await GetAPIKey();
            var token = response.Data.Token;
            
            IAPISetup apiSetup = new APISetup(_ioHelper, SectionName);
            ICoreAPI api = new CoreAPIClient(apiSetup);

            VisitInfoModel model = new VisitInfoModel
            {
                VisitStatusID = "1",
                VisitSDate = "2025-01-01",
                VisitEDate = "2025-01-31"
            };
            
            Dictionary<string, string> formModel = new Dictionary<string, string>
            {
                ["Type"] = "Search",
                ["VisitStatusID"] = model.VisitStatusID,
                ["VisitSDate"] = model.VisitSDate,
                ["VisitEDate"] = model.VisitEDate
            };
            
            IAPIRequest requestParam = new APIRequest
            {
                Authentication = new BearerTokenAuthentication(token),
                Body = RequestBody.CreateFormData(formModel)
            };
            
            // Act
            var visitInfoList = await api.PostAsync<object>("GetVisitInfoList", requestParam);
            
            // Assert
            Assert.True(visitInfoList.IsSuccess);
            Assert.NotNull(visitInfoList.Data);
        }

        [Fact]
        public async Task GetVisitInfoListWebTest()
        {
            // Arrange
            var response = await GetAPIKey();
            var token = response.Data.Token;
            
            IAPISetup apiSetup = new APISetup(_ioHelper, WebSectionName);
            ICoreAPI api = new CoreAPIClient(apiSetup);

            VisitInfo model = new VisitInfo
            {
                Type = "VisitSearch",
                Status = ["0", "1", "2", "3"],
                VisitSDate = "2025-01-01",
                VisitEDate = "2025-01-31",
            };
            
            IAPIRequest request = new APIRequest
            {
                Authentication = new BearerTokenAuthentication(token),
                Body = RequestBody.CreateJson(model)
            };
            
            // Act
            var visitInfoList = await api.PostAsync<object>("GetVisitInfoList", request);

            JObject data = visitInfoList.Data as JObject;
            
            // Assert
            Assert.True(visitInfoList.IsSuccess);
            Assert.NotNull(visitInfoList.Data);
            Assert.True(data?.ContainsKey("success"));
            Assert.Equal(data.GetValue("success"), true);
        }
        
        private async Task<IAPIResponse<TokenResponseModel>> GetAPIKey()
        {
            // Arrange
            IAPISetup apiSetup = new APISetup(_ioHelper, SectionName);
            ICoreAPI api = new CoreAPIClient(apiSetup);
            
            Dictionary<string, string> formModel = new Dictionary<string, string>
            {
                ["ID"] = "apiuser",
                ["Email"] = "api@api.com"
            };

            IAPIRequest requestParam = new APIRequest
            {
                Body = RequestBody.CreateFormData(formModel)
            };

            // Act
            return await api.PostAsync<TokenResponseModel>("GetTokenSECU", requestParam);
        }
    }
    
    // Test Models
    public class TokenResponseModel
    {
        public string ErrorCode { get; set; }
        public string Token { get; set; }
    }

    public class VisitInfoModel
    {
        public string VisitStatusID { get; set; }
        public string? VisitSDate { get; set; }
        public string? VisitEDate { get; set; }
    }

    public class VisitInfo
    {
        public string? VisitSDate { get; set; }
        public string? VisitEDate { get; set; }
        public List<string>? Status { get; set; }
        public string? Type { get; set; }
    }
}