using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Xunit;
using Microsoft.Data.SqlClient;
using MDLSoft.DistributedLock;

namespace MDLSoft.DistributedLock.Tests
{
    public class SqlServerDistributedLockProviderTests : IDisposable
    {
        private readonly string _connectionString;
        private readonly SqlServerDistributedLockProvider _lockProvider;
        private const string TestTableName = "TestDistributedLocks";

        public SqlServerDistributedLockProviderTests()
        {
            // Use local SQL Server instance for testing
            // You can modify this connection string for your test environment
            _connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=TestDistributedLocks;Integrated Security=true;TrustServerCertificate=true;";
            _lockProvider = new SqlServerDistributedLockProvider(_connectionString, TestTableName, false);

            // Ensure database and table exist for testing
            CreateTestDatabase();
            _lockProvider.EnsureTableExists();
        }

        private void CreateTestDatabase()
        {
            var masterConnectionString = @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;";
            using (var connection = new SqlConnection(masterConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                
                var dbName = "TestDistributedLocks";
                var dbPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{dbName}.mdf");
                var logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{dbName}_log.ldf");

                command.CommandText = $@"
                    IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{dbName}')
                    CREATE DATABASE [{dbName}]
                    ON PRIMARY (NAME={dbName}, FILENAME='{dbPath}')
                    LOG ON (NAME={dbName}_log, FILENAME='{logPath}')";
                
                try 
                {
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex) when (ex.Number == 1801) // Database already exists
                {
                    // Ignore
                }
            }
        }

        [Fact]
        public void TryAcquireLock_ShouldReturnLock_WhenLockNotExists()
        {


            // Arrange
            var lockId = "test-lock-1";

            // Act
            var lockResult = _lockProvider.TryAcquireLock(lockId);

            // Assert
            Assert.NotNull(lockResult);
            Assert.Equal(lockId, lockResult.LockId);
            Assert.True(lockResult.IsAcquired);

            // Cleanup
            lockResult.Release();
        }

        [Fact]
        public void TryAcquireLock_ShouldReturnNull_WhenLockAlreadyExists()
        {


            // Arrange
            var lockId = "test-lock-2";

            // Act
            var firstLock = _lockProvider.TryAcquireLock(lockId);
            var secondLock = _lockProvider.TryAcquireLock(lockId);

            // Assert
            Assert.NotNull(firstLock);
            Assert.Null(secondLock);

            // Cleanup
            firstLock.Release();
        }

        [Fact]
        public async Task TryAcquireLockAsync_ShouldReturnLock_WhenLockNotExists()
        {


            // Arrange
            var lockId = "test-lock-3";

            // Act
            var lockResult = await _lockProvider.TryAcquireLockAsync(lockId);

            // Assert
            Assert.NotNull(lockResult);
            Assert.Equal(lockId, lockResult.LockId);
            Assert.True(lockResult.IsAcquired);

            // Cleanup
            await lockResult.ReleaseAsync();
        }

        [Fact]
        public void AcquireLock_ShouldReturnLock_WhenLockNotExists()
        {


            // Arrange
            var lockId = "test-lock-4";

            // Act
            var lockResult = _lockProvider.AcquireLock(lockId);

            // Assert
            Assert.NotNull(lockResult);
            Assert.Equal(lockId, lockResult.LockId);
            Assert.True(lockResult.IsAcquired);

            // Cleanup
            lockResult.Release();
        }

        [Fact]
        public void AcquireLock_ShouldThrowTimeout_WhenLockAlreadyExistsAndTimeoutSpecified()
        {


            // Arrange
            var lockId = "test-lock-5";
            var timeout = TimeSpan.FromMilliseconds(500);

            // Act & Assert
            var firstLock = _lockProvider.AcquireLock(lockId);

            var exception = Assert.Throws<DistributedLockTimeoutException>(() =>
                _lockProvider.AcquireLock(lockId, timeout));

            Assert.Equal(lockId, exception.LockId);

            // Cleanup
            firstLock.Release();
        }

        [Fact]
        public async Task AcquireLockAsync_ShouldReturnLock_WhenLockNotExists()
        {


            // Arrange
            var lockId = "test-lock-6";

            // Act
            var lockResult = await _lockProvider.AcquireLockAsync(lockId);

            // Assert
            Assert.NotNull(lockResult);
            Assert.Equal(lockId, lockResult.LockId);
            Assert.True(lockResult.IsAcquired);

            // Cleanup
            await lockResult.ReleaseAsync();
        }

        [Fact]
        public void Lock_ShouldBeReleasedAfterDispose()
        {


            // Arrange
            var lockId = "test-lock-7";

            // Act
            using (var firstLock = _lockProvider.AcquireLock(lockId))
            {
                Assert.True(firstLock.IsAcquired);
            }

            // The lock should be released after dispose, so we should be able to acquire it again
            var secondLock = _lockProvider.TryAcquireLock(lockId);

            // Assert
            Assert.NotNull(secondLock);
            Assert.True(secondLock.IsAcquired);

            // Cleanup
            secondLock.Release();
        }

        [Fact]
        public void Lock_ShouldBeReleasedAfterRelease()
        {


            // Arrange
            var lockId = "test-lock-8";

            // Act
            var firstLock = _lockProvider.AcquireLock(lockId);
            firstLock.Release();

            // The lock should be released, so we should be able to acquire it again
            var secondLock = _lockProvider.TryAcquireLock(lockId);

            // Assert
            Assert.NotNull(secondLock);
            Assert.True(secondLock.IsAcquired);

            // Cleanup
            secondLock.Release();
        }

        [Fact]
        public void TryAcquireLock_ShouldStoreUserContext()
        {
            // Arrange
            var lockId = "test-lock-user-context";
            var userContext = "TestUserContextInfo";

            // Act
            var lockResult = _lockProvider.TryAcquireLock(lockId, userContext: userContext);

            // Assert
            Assert.NotNull(lockResult);
            Assert.Equal(lockId, lockResult.LockId);
            Assert.Equal(userContext, lockResult.UserContext);
            Assert.True(lockResult.IsAcquired);

            // Verify persistence (optional, but good for integration test)
            // You might need to query the DB directly to be 100% sure it's in the column, 
            // but checking the returned object is a good first step.
            
            // Cleanup
            lockResult.Release();
        }

        [Fact]
        public void TryAcquireLock_ShouldThrow_WhenLockIdTooLong()
        {

            var longId = new string('a', 256);
            Assert.Throws<ArgumentException>(() => _lockProvider.TryAcquireLock(longId));
        }

        [Fact]
        public async Task ConcurrentLockAcquisition_ShouldOnlyAllowOneLock()
        {
            // Arrange
            var lockId = "concurrent-test-" + Guid.NewGuid();
            var tasks = new Task<bool>[10];
            var successCount = 0;

            // Act - Launch 10 concurrent attempts to acquire the same lock
            for (int i = 0; i < 10; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    try
                    {
                        var lockResult = _lockProvider.TryAcquireLock(lockId, TimeSpan.FromMilliseconds(0));
                        if (lockResult != null)
                        {
                            Interlocked.Increment(ref successCount);
                            Thread.Sleep(50); // Hold lock briefly
                            lockResult.Release();
                            return true;
                        }
                        return false;
                    }
                    catch
                    {
                        return false;
                    }
                });
            }

            await Task.WhenAll(tasks);

            // Assert - Only one task should have successfully acquired the lock
            successCount.Should().Be(1, "exactly one task should acquire the lock in concurrent scenarios");
        }

        public void Dispose()
        {


            // Cleanup test data
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = $"DROP TABLE IF EXISTS [{TestTableName}]";
                    command.ExecuteNonQuery();
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
