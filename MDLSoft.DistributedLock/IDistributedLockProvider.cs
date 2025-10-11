using System;
using System.Threading;
using System.Threading.Tasks;

namespace MDLSoft.DistributedLock
{
    /// <summary>
    /// Provides distributed lock functionality
    /// </summary>
    public interface IDistributedLockProvider
    {
        /// <summary>
        /// Attempts to acquire a distributed lock synchronously
        /// </summary>
        /// <param name="lockId">The unique identifier for the lock</param>
        /// <param name="timeout">The maximum time to wait for the lock</param>
        /// <returns>The acquired lock if successful, null if the lock could not be acquired</returns>
        IDistributedLock? TryAcquireLock(string lockId, TimeSpan? timeout = null);

        /// <summary>
        /// Attempts to acquire a distributed lock asynchronously
        /// </summary>
        /// <param name="lockId">The unique identifier for the lock</param>
        /// <param name="timeout">The maximum time to wait for the lock</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task that represents the asynchronous operation containing the acquired lock if successful, null if the lock could not be acquired</returns>
        Task<IDistributedLock?> TryAcquireLockAsync(string lockId, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Acquires a distributed lock synchronously, throwing an exception if unable to acquire
        /// </summary>
        /// <param name="lockId">The unique identifier for the lock</param>
        /// <param name="timeout">The maximum time to wait for the lock</param>
        /// <returns>The acquired lock</returns>
        /// <exception cref="DistributedLockTimeoutException">Thrown when the lock cannot be acquired within the specified timeout</exception>
        IDistributedLock AcquireLock(string lockId, TimeSpan? timeout = null);

        /// <summary>
        /// Acquires a distributed lock asynchronously, throwing an exception if unable to acquire
        /// </summary>
        /// <param name="lockId">The unique identifier for the lock</param>
        /// <param name="timeout">The maximum time to wait for the lock</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task that represents the asynchronous operation containing the acquired lock</returns>
        /// <exception cref="DistributedLockTimeoutException">Thrown when the lock cannot be acquired within the specified timeout</exception>
        Task<IDistributedLock> AcquireLockAsync(string lockId, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}