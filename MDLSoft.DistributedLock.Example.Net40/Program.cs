using System;
using System.Configuration;
using System.Threading;

namespace MDLSoft.DistributedLock.Example.Net40
{
    /// <summary>
    /// Example application demonstrating MDLSoft.DistributedLock on .NET Framework 4.0
    /// 
    /// This example shows:
    /// 1. Basic lock acquisition and release
    /// 2. Lock timeouts
    /// 3. Concurrent access simulation
    /// 4. Error handling
    /// 5. Proper disposal patterns
    /// </summary>
    static class Program
    {
        private static SqlServerDistributedLockProvider _lockProvider;

        static void Main()
        {
            Console.WriteLine("=== MDLSoft.DistributedLock Example (.NET Framework 4.0) ===\n");

            try
            {
                // Initialize connection string and lock provider
                InitializeLockProvider();

                // Run various examples
                Console.WriteLine("Running examples...\n");

                Example1_BasicLockUsage();
                Example2_LockTimeout();
                Example3_ConcurrentAccess();
                Example4_UsingStatement();
                Example5_ErrorHandling();

                Console.WriteLine("\n=== All examples completed successfully! ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                Console.WriteLine("\nThis might be because SQL Server is not available.");
                Console.WriteLine("Please ensure you have SQL Server or SQL Server LocalDB installed.");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static void InitializeLockProvider()
        {
            // Get connection string from app.config
            var _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found in app.config.");
            }

            Console.WriteLine("Using connection string: " + _connectionString.Replace(_connectionString.Split(';')[0], "Server=***"));

            // Create lock provider
            _lockProvider = new SqlServerDistributedLockProvider(_connectionString, "ExampleLocks", false);

            // Ensure the database table exists
            try
            {
                _lockProvider.EnsureTableExists();
                Console.WriteLine("✓ Lock table created/verified");
            }
            catch (Exception ex)
            {
                Console.WriteLine("✗ Failed to create lock table: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Example 1: Basic lock acquisition and release
        /// </summary>
        static void Example1_BasicLockUsage()
        {
            Console.WriteLine("--- Example 1: Basic Lock Usage ---");

            var lockId = "basic-example-" + DateTime.Now.Ticks;

            try
            {
                // Try to acquire a lock
                var distributedLock = _lockProvider.TryAcquireLock(lockId);

                if (distributedLock != null)
                {
                    Console.WriteLine("✓ Lock acquired: " + lockId);

                    // Simulate some work
                    Console.WriteLine("  Performing work...");
                    Thread.Sleep(1000);

                    // Release the lock
                    distributedLock.Release();
                    Console.WriteLine("✓ Lock released");
                }
                else
                {
                    Console.WriteLine("✗ Failed to acquire lock");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("✗ Error: " + ex.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example 2: Lock with timeout
        /// </summary>
        static void Example2_LockTimeout()
        {
            Console.WriteLine("--- Example 2: Lock with Timeout ---");

            var lockId = "timeout-example-" + DateTime.Now.Ticks;

            try
            {
                // First, acquire a lock to block the second attempt
                var firstLock = _lockProvider.AcquireLock(lockId);
                Console.WriteLine("✓ First lock acquired: " + lockId);

                // Try to acquire the same lock with a timeout (should fail)
                var secondLock = _lockProvider.TryAcquireLock(lockId, TimeSpan.FromMilliseconds(500));

                if (secondLock == null)
                {
                    Console.WriteLine("✓ Second lock correctly timed out");
                }
                else
                {
                    Console.WriteLine("✗ Second lock should have timed out");
                    secondLock.Release();
                }

                // Release the first lock
                firstLock.Release();
                Console.WriteLine("✓ First lock released");

                // Now the second attempt should succeed
                var thirdLock = _lockProvider.TryAcquireLock(lockId);
                if (thirdLock != null)
                {
                    Console.WriteLine("✓ Third lock acquired after first was released");
                    thirdLock.Release();
                    Console.WriteLine("✓ Third lock released");
                }
            }
            catch (DistributedLockTimeoutException ex)
            {
                Console.WriteLine("✓ Lock timeout handled correctly: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("✗ Error: " + ex.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example 3: Simulating concurrent access from multiple "processes"
        /// </summary>
        static void Example3_ConcurrentAccess()
        {
            Console.WriteLine("--- Example 3: Concurrent Access Simulation ---");

            var lockId = "concurrent-example-" + DateTime.Now.Ticks;
            var completedCount = 0;

            // Simulate 5 concurrent "processes" trying to acquire the same lock
            for (int i = 1; i <= 5; i++)
            {
                var processId = i;
                var thread = new Thread(() =>
                {
                    try
                    {
                        Console.WriteLine("Process {0}: Trying to acquire lock...", processId);

                        var distributedLock = _lockProvider.TryAcquireLock(lockId, TimeSpan.FromSeconds(2));

                        if (distributedLock != null)
                        {
                            Console.WriteLine("Process {0}: ✓ Lock acquired! Working...", processId);

                            // Simulate exclusive work
                            Thread.Sleep(500);

                            distributedLock.Release();
                            Console.WriteLine("Process {0}: ✓ Work completed, lock released", processId);
                        }
                        else
                        {
                            Console.WriteLine("Process {0}: ✗ Could not acquire lock within timeout", processId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Process {0}: ✗ Error: {1}", processId, ex.Message);
                    }
                    finally
                    {
                        Interlocked.Increment(ref completedCount);
                    }
                });

                thread.Start();
            }

            // Wait for all processes to complete
            while (completedCount < 5)
            {
                Thread.Sleep(100);
            }

            Console.WriteLine("✓ All concurrent processes completed");
            Console.WriteLine();
        }

        /// <summary>
        /// Example 4: Using statement for automatic disposal
        /// </summary>
        static void Example4_UsingStatement()
        {
            Console.WriteLine("--- Example 4: Using Statement (Automatic Disposal) ---");

            var lockId = "using-example-" + DateTime.Now.Ticks;

            try
            {
                // The using statement automatically calls Dispose() which releases the lock
                using (var distributedLock = _lockProvider.AcquireLock(lockId))
                {
                    Console.WriteLine("✓ Lock acquired: " + lockId);
                    Console.WriteLine("  Performing work...");
                    Thread.Sleep(500);
                    // Lock will be automatically released when exiting the using block
                }

                Console.WriteLine("✓ Lock automatically released via Dispose()");

                // Verify the lock was released by acquiring it again
                using (var secondLock = _lockProvider.AcquireLock(lockId))
                {
                    Console.WriteLine("✓ Successfully acquired the same lock again");
                }

                Console.WriteLine("✓ Second lock also automatically released");
            }
            catch (Exception ex)
            {
                Console.WriteLine("✗ Error: " + ex.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example 5: Error handling scenarios
        /// </summary>
        static void Example5_ErrorHandling()
        {
            Console.WriteLine("--- Example 5: Error Handling ---");

            try
            {
                // Test invalid lock ID
                try
                {
                    var _ = _lockProvider.TryAcquireLock(null);
                    Console.WriteLine("✗ Should have thrown exception for null lock ID");
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("✓ Correctly handled null lock ID");
                }

                // Test empty lock ID
                try
                {
                    var _ = _lockProvider.TryAcquireLock("");
                    Console.WriteLine("✗ Should have thrown exception for empty lock ID");
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("✓ Correctly handled empty lock ID");
                }

                // Test timeout exception
                try
                {
                    var lockId = "timeout-test-" + DateTime.Now.Ticks;

                    // First acquire the lock
                    var firstLock = _lockProvider.AcquireLock(lockId);

                    // Try to acquire again with AcquireLock (should throw timeout exception)
                    var secondLock = _lockProvider.AcquireLock(lockId, TimeSpan.FromMilliseconds(100));

                    Console.WriteLine("✗ Should have thrown timeout exception");
                    firstLock.Release();
                    secondLock.Release();
                }
                catch (DistributedLockTimeoutException ex)
                {
                    Console.WriteLine("✓ Correctly handled lock timeout: " + ex.Message);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("✗ Unexpected error: " + ex.Message);
            }

            Console.WriteLine();
        }
    }
}
