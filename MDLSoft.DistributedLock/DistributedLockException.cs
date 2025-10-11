using System;

namespace MDLSoft.DistributedLock
{
    /// <summary>
    /// Base exception for distributed lock operations
    /// </summary>
    public class DistributedLockException : Exception
    {
        public DistributedLockException() { }
        public DistributedLockException(string message) : base(message) { }
        public DistributedLockException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a distributed lock operation times out
    /// </summary>
    public class DistributedLockTimeoutException : DistributedLockException
    {
        public string LockId { get; }

        public DistributedLockTimeoutException(string lockId) 
            : base($"Timeout occurred while trying to acquire lock '{lockId}'")
        {
            LockId = lockId;
        }

        public DistributedLockTimeoutException(string lockId, TimeSpan timeout) 
            : base($"Timeout occurred while trying to acquire lock '{lockId}' within {timeout}")
        {
            LockId = lockId;
        }
    }

    /// <summary>
    /// Exception thrown when a distributed lock operation fails
    /// </summary>
    public class DistributedLockOperationException : DistributedLockException
    {
        public string LockId { get; }

        public DistributedLockOperationException(string lockId, string operation) 
            : base($"Failed to {operation} lock '{lockId}'")
        {
            LockId = lockId;
        }

        public DistributedLockOperationException(string lockId, string operation, Exception innerException) 
            : base($"Failed to {operation} lock '{lockId}'", innerException)
        {
            LockId = lockId;
        }
    }
}