using System;
using System.Threading;
using System.Threading.Tasks;

namespace APIClient.Client.Interfaces
{
    public interface IRetryPolicy
    {
        Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);
    }
}