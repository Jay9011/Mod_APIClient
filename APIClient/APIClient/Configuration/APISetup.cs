using System;
using System.Collections.Generic;
using System.IO;
using APIClient.Configuration.Interfaces;
using APIClient.Configuration.Models;
using FileIOHelper;
using Newtonsoft.Json;

namespace APIClient.Configuration
{
    public class APISetup : IAPISetup
    {
        private const string BaseUrlKey = "BaseUrl";
        private const string EndpointsKey = "Endpoints";

        public IAPIConnectionInfo ConnectionInfo { get; }
        
        private readonly object _lock = new object();

        #region 의존 주입

        private readonly IIOHelper _ioHelper;
        private readonly string _sectionName;

        #endregion
        
        public APISetup(IIOHelper ioHelper, string sectionName)
        {
            #region 의존 주입

            _ioHelper = ioHelper ?? throw new ArgumentException(nameof(ioHelper));
            _sectionName = sectionName ?? throw new ArgumentException(nameof(sectionName));

            #endregion

            LoadConnectionInfo();
        }

        public IAPIConnectionInfo GetConnectionInfo()
        {
            lock (_lock)
            {
                return LoadConnectionInfo();
            }
        }

        public void UpdateConnectionInfo(IAPIConnectionInfo connectionInfo)
        {
            lock (_lock)
            {
                var settings = new Dictionary<string, string>
                {
                    [BaseUrlKey] = connectionInfo.BaseUrl,
                    [EndpointsKey] = JsonConvert.SerializeObject(connectionInfo.Endpoints)
                };
                
                _ioHelper.WriteSection(_sectionName, settings);
            }
        }
        
        private APIConnectionInfo LoadConnectionInfo()
        {
            Dictionary<string, string> settings;

            try
            {
                settings = _ioHelper.ReadSection(_sectionName);
            }
            // 섹션이 없을 경우 새로 추가
            catch (KeyNotFoundException)
            {
                _ioHelper.WriteValue(_sectionName, BaseUrlKey, "");
                settings = _ioHelper.ReadSection(_sectionName);
            }
            // 파일이 없을 경우 새로 생성
            catch (FileNotFoundException)
            {
                _ioHelper.WriteValue(_sectionName, BaseUrlKey, "");
                settings = _ioHelper.ReadSection(_sectionName);
            }
            catch (Exception e)
            {
                throw new ArgumentException("설정 파일을 읽는 도중 오류가 발생했습니다.", e);
            }

            if (!settings.TryGetValue(BaseUrlKey, out var baseUrl))
            {
                throw new KeyNotFoundException($"'{BaseUrlKey}' not found.");
            }

            var info = new APIConnectionInfo() { BaseUrl = baseUrl };
            
            if (settings.TryGetValue(EndpointsKey, out var endpointsJson))
            {
                try
                {
                    var endpoints = JsonConvert.DeserializeObject<Dictionary<string, string>>(endpointsJson);
                    if (endpoints != null)
                    {
                        info.Endpoints = endpoints;
                    }
                }
                catch (JsonException e)
                {
                    throw new FormatException($"Failed to parse endpoints JSON in section '{_sectionName}'.", e);
                }
            }
            
            return info;
        }
    }
}