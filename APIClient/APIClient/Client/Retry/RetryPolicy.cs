using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using APIClient.Client.Interfaces;

namespace APIClient.Client.Retry
{
    public class RetryPolicy : IRetryPolicy
    {
        private readonly int _maxRetryCount;
        private readonly Func<int, TimeSpan> _delayStrategy;
        private readonly Func<Exception, bool> _shouldRetry;
        private readonly Action<Exception, int> _onRetry;
        
        public static RetryPolicy Default => new RetryPolicy();
        
        public RetryPolicy(
            int maxRetryCount = 3, 
            Func<int, TimeSpan> delayStrategy = null, 
            Func<Exception, bool> shouldRetry = null, 
            Action<Exception, int> onRetry = null)
        {
            _maxRetryCount = maxRetryCount;
            _delayStrategy = delayStrategy ?? (retryCount => TimeSpan.FromSeconds(Math.Pow(2, retryCount)));
            _shouldRetry = shouldRetry ?? (ex => ex is HttpRequestException || ex is TaskCanceledException);
            _onRetry = onRetry ?? ((ex, count) => System.Diagnostics.Debug.WriteLine($"Retry {count}: {ex.Message}"));
        }
        
        public static RetryPolicy Create(
            int maxRetryCount = 3, 
            Func<int, TimeSpan> delayStrategy = null, 
            Func<Exception, bool> shouldRetry = null, 
            Action<Exception, int> onRetry = null)
        {
            return new RetryPolicy(maxRetryCount, delayStrategy, shouldRetry, onRetry);
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
        {
            var retryCount = 0;
            while (true)
            {
                try
                {
                    return await action();
                }
                catch (Exception e)
                {
                    if (retryCount >= _maxRetryCount || !_shouldRetry(e) || cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    
                    _onRetry(e, retryCount + 1);

                    try
                    {
                        await Task.Delay(_delayStrategy(retryCount), cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }

                    retryCount++;
                }
            }
        }
    }
}