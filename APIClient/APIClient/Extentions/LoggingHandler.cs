using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace APIClient.Extentions
{
    public class LoggingHandler : DelegatingHandler
    {
        public LoggingHandler(HttpMessageHandler innerHandler = null, bool ignoreSslErrors = false)
            : base(innerHandler ?? CreateHandler(ignoreSslErrors))
        {
            
        }

        private static HttpClientHandler CreateHandler(bool ignoreSslErrors)
        {
            var handler = new HttpClientHandler();
            if (ignoreSslErrors)
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }
            return handler;
        }
        
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine("Request:");
            Console.WriteLine($"{request.Method} {request.RequestUri}");
            Console.WriteLine("Headers:");
            foreach (var header in request.Headers)
            {
                Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            if (request.Content != null)
            {
                Console.WriteLine("Content Headers:");
                foreach (var header in request.Content.Headers)
                {
                    Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
                }

                Console.WriteLine("Content:");
                var content = await request.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
            
            var response = await base.SendAsync(request, cancellationToken);

            Console.WriteLine("\nResponse:");
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine("Headers:");
            foreach (var header in response.Headers)
            {
                Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            if (response.Content != null)
            {
                Console.WriteLine("Content:");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }

            return response;
        }
    }
}