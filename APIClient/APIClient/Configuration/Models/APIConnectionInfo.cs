using System;
using System.Collections.Generic;
using APIClient.Configuration.Interfaces;

namespace APIClient.Configuration.Models
{
    public class APIConnectionInfo : IAPIConnectionInfo
    {
        public string BaseUrl { get; set; }
        public Dictionary<string, string> Endpoints { get; set; }

        public string GetEndpoint(string endpointName)
        {
            if (!Endpoints.TryGetValue(endpointName, out var endpoint))
            {
                throw new KeyNotFoundException($"Endpoint '{endpointName}' not found.");
            }

            return endpoint;
        }
    }
}