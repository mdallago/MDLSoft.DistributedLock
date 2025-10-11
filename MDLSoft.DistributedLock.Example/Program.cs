using MDLSoft.DistributedLock;

// Example usage of MDLSoft.DistributedLock
Console.WriteLine("MDLSoft.DistributedLock Example");
Console.WriteLine("================================");

// Configure your connection string
var connectionString = @"Data Source=.\sqlexpress;Initial Catalog=OlimpoTest;Trusted_Connection=True;TrustServerCertificate=true;";

// Create the lock provider
var lockProvider = new SqlServerDistributedLockProvider(connectionString);

try
{
    // Ensure the table exists
    await lockProvider.EnsureTableExistsAsync();
    Console.WriteLine("✓ Database table ensured");

    // Example 1: Basic lock usage
    Console.WriteLine("\nExample 1: Basic Lock Usage");
    await BasicLockExample(lockProvider);

    // Example 2: Lock with timeout
    Console.WriteLine("\nExample 2: Lock with Timeout");
    await LockWithTimeoutExample(lockProvider);

    // Example 3: Concurrent lock attempts
    Console.WriteLine("\nExample 3: Concurrent Lock Attempts");
    await ConcurrentLockExample(lockProvider);
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Console.WriteLine("Make sure you have SQL Server LocalDB installed and running.");
}

static async Task BasicLockExample(SqlServerDistributedLockProvider provider)
{
    var lockId = "example-basic-lock";
    
    using (var distributedLock = await provider.TryAcquireLockAsync(lockId))
    {
        if (distributedLock != null)
        {
            Console.WriteLine($"✓ Acquired lock '{lockId}'");
            
            // Simulate some work
            await Task.Delay(1000);
            Console.WriteLine("  Work completed");
        }
        else
        {
            Console.WriteLine($"❌ Could not acquire lock '{lockId}'");
        }
    }
    Console.WriteLine($"✓ Lock '{lockId}' released");
}

static async Task LockWithTimeoutExample(SqlServerDistributedLockProvider provider)
{
    var lockId = "example-timeout-lock";
    
    try
    {
        // First, acquire a lock that will block the second attempt
        using (var firstLock = await provider.TryAcquireLockAsync(lockId))
        {
            if (firstLock != null)
            {
                Console.WriteLine($"✓ First lock acquired '{lockId}'");
                
                // Try to acquire the same lock with a timeout (should fail)
                try
                {
                    using (var secondLock = await provider.AcquireLockAsync(lockId, TimeSpan.FromSeconds(2)))
                    {
                        Console.WriteLine("This should not be reached");
                    }
                }
                catch (DistributedLockTimeoutException ex)
                {
                    Console.WriteLine($"✓ Expected timeout exception: {ex.Message}");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Unexpected error: {ex.Message}");
    }
}

static async Task ConcurrentLockExample(SqlServerDistributedLockProvider provider)
{
    var lockId = "example-concurrent-lock";
    var tasks = new List<Task>();
    
    // Start 3 concurrent tasks trying to acquire the same lock
    for (int i = 1; i <= 3; i++)
    {
        int taskId = i;
        tasks.Add(Task.Run(async () =>
        {
            using (var distributedLock = await provider.TryAcquireLockAsync(lockId, TimeSpan.FromSeconds(5)))
            {
                if (distributedLock != null)
                {
                    Console.WriteLine($"✓ Task {taskId} acquired lock '{lockId}'");
                    await Task.Delay(2000); // Simulate work
                    Console.WriteLine($"✓ Task {taskId} completed work");
                }
                else
                {
                    Console.WriteLine($"❌ Task {taskId} could not acquire lock '{lockId}'");
                }
            }
        }));
    }
    
    await Task.WhenAll(tasks);
    Console.WriteLine("✓ All concurrent tasks completed");
}

Console.WriteLine("\n✓ Example completed successfully!");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
