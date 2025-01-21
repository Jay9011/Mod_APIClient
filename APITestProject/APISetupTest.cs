using APIClient.Authentication.Models;
using APIClient.Client;
using APIClient.Configuration;
using APIClient.Configuration.Models;
using FileIOHelper;
using FileIOHelper.Helpers;

namespace APITestProject
{
    public class APISetupTest
    {
        private IIOHelper _ioHelper;
        private string SectionName = "TEST_API";

        public APISetupTest()
        {
            _ioHelper = new RegistryHelper("SoftWare\\SECUiDEA\\APITest");
            SetupTestAPI();
        }

        public void SetupTestAPI()
        {
            var connectionInfo = new APIConnectionInfo
            {
                BaseUrl = "https://test.URICode.com",
                Endpoints = new Dictionary<string, string>
                {
                    ["ConnectionTest"] = "Connection", // GET, no body
                    ["GetAuth"] = "GetAuth", // POST with id, email
                    ["GetPerson"] = "GetPerson/{id}", // GET with path parameter
                    ["GetPeople"] = "GetPerson", // POST, no body
                }
            };

            var apiSetup = new APISetup(_ioHelper, SectionName);
            apiSetup.UpdateConnectionInfo(connectionInfo);
        }

        [Fact]
        public async Task EndPointMatchingTest()
        {
            // Arrange
            var apiSetup = new APISetup(_ioHelper, SectionName);
            var api = new CoreAPIClient(apiSetup);

            // Act
            var connectionInfo = apiSetup.GetConnectionInfo();

            // Assert
            Assert.Equal("https://test.URICode.com", connectionInfo.BaseUrl);
            Assert.Equal("Connection", connectionInfo.GetEndpoint("ConnectionTest"));
            Assert.Equal("GetAuth", connectionInfo.GetEndpoint("GetAuth"));
            Assert.Equal("GetPerson/{id}", connectionInfo.GetEndpoint("GetPerson"));
            Assert.Equal("GetPerson", connectionInfo.GetEndpoint("GetPeople"));
        }
    }
}