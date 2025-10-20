using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;


#if NETSTANDARD2_0
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif

namespace MDLSoft.DistributedLock
{
    /// <summary>
    /// SQL Server implementation of a distributed lock (lightweight, no external dependencies)
    /// </summary>
    internal sealed class SqlServerDistributedLock : IDistributedLock
    {
        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly string _lockId;
        private readonly string _lockToken;
        private bool _isDisposed;
        private bool _isAcquired;

        public string LockId { get { return _lockId; } }
        public bool IsAcquired { get { return _isAcquired && !_isDisposed; } }

        internal SqlServerDistributedLock(string connectionString, string tableName, string lockId, string lockToken)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            _lockId = lockId ?? throw new ArgumentNullException(nameof(lockId));
            _lockToken = lockToken ?? throw new ArgumentNullException(nameof(lockToken));
            _isAcquired = true;
        }

        public void Release()
        {
            if (_isDisposed || !_isAcquired) return;

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        #pragma warning disable CA2100
                        command.CommandText = string.Format(CultureInfo.CurrentCulture,
                            "DELETE FROM [{0}] WHERE [LockId] = @LockId AND [LockToken] = @LockToken",
                            _tableName);
                        #pragma warning restore CA2100
                        command.Parameters.Add(new SqlParameter("@LockId", SqlDbType.NVarChar, 255) { Value = _lockId });
                        command.Parameters.Add(new SqlParameter("@LockToken", SqlDbType.NVarChar, 255) { Value = _lockToken });
                        command.ExecuteNonQuery();
                    }
                    _isAcquired = false;
                }
            }
            catch (Exception ex)
            {
                throw new DistributedLockOperationException(_lockId, "release", ex);
            }
        }

        public async Task ReleaseAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_isDisposed || !_isAcquired) return;

            try
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
                        #pragma warning disable CA2100
                        command.CommandText = string.Format(CultureInfo.CurrentCulture,
                            "DELETE FROM [{0}] WHERE [LockId] = @LockId AND [LockToken] = @LockToken",
                            _tableName);
                        #pragma warning restore CA2100
                        command.Parameters.Add(new SqlParameter("@LockId", SqlDbType.NVarChar, 255) { Value = _lockId });
                        command.Parameters.Add(new SqlParameter("@LockToken", SqlDbType.NVarChar, 255) { Value = _lockToken });
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
                    _isAcquired = false;
                }
            }
            catch (Exception ex)
            {
                throw new DistributedLockOperationException(_lockId, "release", ex);
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            #pragma warning disable CA1031
            try
            {
                if (_isAcquired)
                {
                    Release();
                }
            }
            catch
            {
                // Ignore exceptions during disposal
            }
            finally
            {
                _isDisposed = true;
            }
            #pragma warning restore CA1031
        }
    }
}
