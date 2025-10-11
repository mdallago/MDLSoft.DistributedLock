using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

#if NETSTANDARD2_0
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif

namespace MDLSoft.DistributedLock
{
    /// <summary>
    /// SQL Server implementation of distributed lock provider (lightweight, no external dependencies)
    /// </summary>
    public class SqlServerDistributedLockProvider : IDistributedLockProvider
    {
        private readonly string _connectionString;
        private readonly string _tableName;

        /// <summary>
        /// Initializes a new instance of the SqlServerDistributedLockProvider
        /// </summary>
        /// <param name="connectionString">The SQL Server connection string</param>
        /// <param name="tableName">The name of the table to store locks (default: DistributedLocks)</param>
        public SqlServerDistributedLockProvider(string connectionString, string tableName = "DistributedLocks")
        {
            if (connectionString == null)
                throw new ArgumentNullException("connectionString");
            if (tableName == null)
                throw new ArgumentNullException("tableName");

            _connectionString = connectionString;
            _tableName = tableName;
        }

        /// <summary>
        /// Ensures the locks table exists in the database
        /// </summary>
        public void EnsureTableExists()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = string.Format(@"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{0}' AND xtype='U')
                        CREATE TABLE [{0}] (
                            [LockId] NVARCHAR(255) NOT NULL PRIMARY KEY,
                            [LockToken] NVARCHAR(255) NOT NULL,
                            [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                        );
                    ", _tableName);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Ensures the locks table exists in the database asynchronously
        /// </summary>
        public async Task EnsureTableExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(_connectionString))
            {
#if NETSTANDARD2_0
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
#else
#if NET40 || NET451
                await TaskEx.Run(() => connection.Open(), cancellationToken).ConfigureAwait(false);
#else
                await Task.Run(() => connection.Open(), cancellationToken).ConfigureAwait(false);
#endif
#endif
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = string.Format(@"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{0}' AND xtype='U')
                        CREATE TABLE [{0}] (
                            [LockId] NVARCHAR(255) NOT NULL PRIMARY KEY,
                            [LockToken] NVARCHAR(255) NOT NULL,
                            [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                        );
                    ", _tableName);
                    
#if NETSTANDARD2_0
                    await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
#else
#if NET40 || NET451
                    await TaskEx.Run(() => command.ExecuteNonQuery(), cancellationToken).ConfigureAwait(false);
#else
                    await Task.Run(() => command.ExecuteNonQuery(), cancellationToken).ConfigureAwait(false);
#endif
#endif
                }
            }
        }



        public IDistributedLock? TryAcquireLock(string lockId, TimeSpan? timeout = null)
        {
            if (string.IsNullOrEmpty(lockId))
                throw new ArgumentException("Lock ID cannot be null or empty", "lockId");
            if (lockId.Length > 255)
                throw new ArgumentException("Lock ID length must be <= 255 characters", "lockId");

            var lockToken = Guid.NewGuid().ToString();
            var timeoutAt = timeout.HasValue ? DateTime.UtcNow.Add(timeout.Value) : (DateTime?)null;

            do
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    // Try to insert the new lock - let PK violation handle conflicts
                    using (var insertCommand = connection.CreateCommand())
                    {
                        insertCommand.CommandText = string.Format(@"
                            INSERT INTO [{0}] ([LockId], [LockToken])
                            VALUES (@LockId, @LockToken)", _tableName);
                        insertCommand.Parameters.AddWithValue("@LockId", lockId);
                        insertCommand.Parameters.AddWithValue("@LockToken", lockToken);

                        try
                        {
                            insertCommand.ExecuteNonQuery();
                            // Success - we got the lock
                            return new SqlServerDistributedLock(_connectionString, _tableName, lockId, lockToken);
                        }
                        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // PK violation or unique constraint
                        {
                            // Lock already exists - this is expected, not an error
                            // No rollback needed since INSERT failed and nothing was committed
                        }
                    }
                }

                // If we have a timeout, check if we've exceeded it
                if (timeoutAt.HasValue && DateTime.UtcNow >= timeoutAt.Value)
                    return null;

                // Wait a bit before retrying
                Thread.Sleep(100);
            } while (timeoutAt.HasValue);

            return null;
        }

        public async Task<IDistributedLock?> TryAcquireLockAsync(string lockId, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(lockId))
                throw new ArgumentException("Lock ID cannot be null or empty", "lockId");
            if (lockId.Length > 255)
                throw new ArgumentException("Lock ID length must be <= 255 characters", "lockId");

            var lockToken = Guid.NewGuid().ToString();
            var timeoutAt = timeout.HasValue ? DateTime.UtcNow.Add(timeout.Value) : (DateTime?)null;

            do
            {
                using (var connection = new SqlConnection(_connectionString))
                {
#if NETSTANDARD2_0
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
#else
#if NET40 || NET451
                    await TaskEx.Run(() => connection.Open(), cancellationToken).ConfigureAwait(false);
#else
                    await Task.Run(() => connection.Open(), cancellationToken).ConfigureAwait(false);
#endif
#endif
                    // Try to insert the new lock - let PK violation handle conflicts
                    using (var insertCommand = connection.CreateCommand())
                    {
                        insertCommand.CommandText = string.Format(@"
                            INSERT INTO [{0}] ([LockId], [LockToken])
                            VALUES (@LockId, @LockToken)", _tableName);
                        insertCommand.Parameters.AddWithValue("@LockId", lockId);
                        insertCommand.Parameters.AddWithValue("@LockToken", lockToken);

                        try
                        {
#if NETSTANDARD2_0
                            await insertCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
#else
#if NET40 || NET451
                            await TaskEx.Run(() => insertCommand.ExecuteNonQuery(), cancellationToken).ConfigureAwait(false);
#else
                            await Task.Run(() => insertCommand.ExecuteNonQuery(), cancellationToken).ConfigureAwait(false);
#endif
#endif
                            // Success - we got the lock
                            return new SqlServerDistributedLock(_connectionString, _tableName, lockId, lockToken);
                        }
                        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // PK violation or unique constraint
                        {
                            // Lock already exists - this is expected, not an error
                            // No rollback needed since INSERT failed and nothing was committed
                        }
                    }
                }

                // If we have a timeout, check if we've exceeded it
                if (timeoutAt.HasValue && DateTime.UtcNow >= timeoutAt.Value)
                    return null;

                // Wait a bit before retrying
#if NETSTANDARD2_0
                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
#else
#if NET40 || NET451
                await TaskEx.Delay(100, cancellationToken).ConfigureAwait(false);
#else
                await Task.Run(() => Thread.Sleep(100), cancellationToken).ConfigureAwait(false);
#endif
#endif
            } while (timeoutAt.HasValue);

            return null;
        }

        public IDistributedLock AcquireLock(string lockId, TimeSpan? timeout = null)
        {
            var lockResult = TryAcquireLock(lockId, timeout);
            if (lockResult == null)
            {
                throw new DistributedLockTimeoutException(lockId, timeout ?? TimeSpan.Zero);
            }
            return lockResult;
        }

        public async Task<IDistributedLock> AcquireLockAsync(string lockId, TimeSpan? timeout = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var lockResult = await TryAcquireLockAsync(lockId, timeout, cancellationToken).ConfigureAwait(false);
            if (lockResult == null)
            {
                throw new DistributedLockTimeoutException(lockId, timeout ?? TimeSpan.Zero);
            }
            return lockResult;
        }
    }
}