using APIClient.Authentication.Models;
using APIClient.Client;
using APIClient.Client.Interfaces;
using APIClient.Configuration;
using APIClient.Configuration.Interfaces;
using APIClient.Configuration.Models;
using APIClient.Models;
using APIClient.Models.Interfaces;
using FileIOHelper;
using FileIOHelper.Helpers;

namespace APITestProject
{
    public class APICallTest
    {
        private IIOHelper _ioHelper;
        private string SectionName = "SECUiDEA_API";
        
        public APICallTest()
        {
            _ioHelper = new RegistryHelper("SoftWare\\SECUiDEA\\APITest");
            SetupTestAPI();
        }

        public void SetupTestAPI()
        {
            var connectionInfo = new APIConnectionInfo
            {
                BaseUrl = "http://192.168.0.63:2763",
                Endpoints = new Dictionary<string, string>
                {
                    ["GetTokenSECU"] = "/CommonAPIv1/GetTokenSECU",
                    ["GetTodos"] = "/todos",
                    ["GetTodoById"] = "/todos/{id}",
                    ["CreatePost"] = "/posts",
                    ["UpdatePost"] = "/posts/{id}",
                    ["SearchComments"] = "/comments",
                }
            };
            
            var apiSetup = new APISetup(_ioHelper, SectionName);
            apiSetup.UpdateConnectionInfo(connectionInfo);
        }
        
        [Fact]
        public async Task GetTokenTest()
        {
            // Arrange
            IAPISetup apiSetup = new APISetup(_ioHelper, SectionName);
            
            string test1 = apiSetup.GetConnectionInfo().BaseUrl;
            string test2 = apiSetup.GetConnectionInfo().Endpoints["GetTokenSECU"];
            string test3 = apiSetup.GetConnectionInfo().Endpoints["CreatePost"];
            
            ICoreAPI api = new CoreAPIClient(apiSetup);
            
            
            TokenModel model = new TokenModel
            {
                ID = "apiuser",
                Email = "api@api.com"
            };
            IAPIRequest requestParam = new APIRequestParameters
            {
                Body = model
            };

            // Act
            var response = await api.PostAsync<TokenModel>("GetTokenSECU", requestParam);
            
            // Assert
            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Data);
            Assert.True(response.Data.ErrorCode == "0000");
        }
    }

    // Test Models
    public class TokenModel
    {
        public string ID { get; set; }
        public string Email { get; set; }
        public string ErrorCode { get; set; }
        public string Token { get; set; }
    }
}