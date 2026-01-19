using System;
using System.Threading;
using System.Threading.Tasks;

namespace MDLSoft.DistributedLock
{
    /// <summary>
    /// Represents a distributed lock that can be acquired and released
    /// </summary>
    public interface IDistributedLock : IDisposable
    {
        /// <summary>
        /// Gets the unique identifier for this lock
        /// </summary>
        string LockId { get; }

        /// <summary>
        /// Gets the user context associated with this lock, if any
        /// </summary>
        string? UserContext { get; }

        /// <summary>
        /// Gets a value indicating whether the lock is currently acquired
        /// </summary>
        bool IsAcquired { get; }

        /// <summary>
        /// Releases the lock synchronously
        /// </summary>
        void Release();

        /// <summary>
        /// Releases the lock asynchronously
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task that represents the asynchronous release operation</returns>
        Task ReleaseAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}