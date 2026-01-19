# MDLSoft.DistributedLock

A high-performance, lightweight distributed lock library for SQL Server with zero external dependencies.

## Features

- 🔒 **Distributed Locking**: Acquire and release locks across multiple processes/applications
- 🗄️ **SQL Server Backend**: Uses SQL Server as the lock storage mechanism
- ⚡ **Zero Dependencies**: Built with raw ADO.NET for maximum performance and minimal footprint
- 🔄 **Sync & Async**: Full support for both synchronous and asynchronous operations
- 🎯 **Universal Compatibility**: Single package supports .NET Framework 4.0 through .NET 8+
- 🚀 **High Performance**: Optimized lock acquisition with READ COMMITTED isolation
- 🔧 **Simple API**: Clean, intuitive interface with manual lock release
- 📦 **Lightweight**: Only 49KB package size

## Installation

Install the package via NuGet:

```bash
# Single package for ALL .NET versions (4.0 through 8+)
dotnet add package MDLSoft.DistributedLock
```

Or via Package Manager Console:

```powershell
# Works with any .NET version
Install-Package MDLSoft.DistributedLock
```

## Quick Start

### 1. Setup the Database Table

```csharp
var connectionString = "Server=.;Database=MyApp;Integrated Security=true;";
var provider = new SqlServerDistributedLockProvider(connectionString);

// Ensure the locks table exists
await provider.EnsureTableExistsAsync();
```

### 2. Basic Usage

```csharp
// Synchronous usage
using (var lockProvider = new SqlServerDistributedLockProvider(connectionString))
{
    using (var distributedLock = lockProvider.TryAcquireLock("my-resource"))
    {
        if (distributedLock != null)
        {
            // Critical section - only one process can execute this
            Console.WriteLine("Lock acquired successfully!");
            // Do your work here...
        }
        else
        {
            Console.WriteLine("Could not acquire lock");
        }
    } // Lock is automatically released here
}

// Asynchronous usage
using (var lockProvider = new SqlServerDistributedLockProvider(connectionString))
{
    using (var distributedLock = await lockProvider.TryAcquireLockAsync("my-resource"))
    {
        if (distributedLock != null)
        {
            // Critical section
            await DoImportantWorkAsync();
        }
    }
}
```

### 4. Storing User Context

You can optionally store a user context string (e.g., username, machine name, reason) with the lock. This can be useful for debugging or administrative tools to see who holds a lock.

```csharp
// Acquire lock with user context
using (var distributedLock = await lockProvider.TryAcquireLockAsync(
    lockId: "my-resource",
    userContext: "User: john.doe | Machine: WEB-01"))
{
    if (distributedLock != null)
    {
        Console.WriteLine($"Lock acquired by: {distributedLock.UserContext}");
        // Do work...
    }
}
```

## Examples

### Complete Example Applications

This repository includes comprehensive example applications demonstrating various usage patterns:

#### 1. Modern .NET Example (`MDLSoft.DistributedLock.Example`)
- **Target**: .NET 8 (modern applications)
- **Features**: Async/await, cancellation tokens, modern C# patterns
- **Use Case**: Web applications, services, modern desktop apps

#### 2. .NET Framework 4.0 Example (`MDLSoft.DistributedLock.Example.Net40`)
- **Target**: .NET Framework 4.0 (legacy compatibility)
- **Features**: Synchronous operations, classic .NET patterns, Thread-based concurrency
- **Use Case**: Legacy applications, Windows services, older codebases

**What the examples demonstrate:**
- ✅ Basic lock acquisition and release patterns
- ✅ Lock timeout handling and conflict resolution
- ✅ Concurrent access simulation (multiple threads/processes)
- ✅ Proper error handling and exception management
- ✅ IDisposable pattern for automatic cleanup
- ✅ Configuration management (app.config/appsettings.json)
- ✅ Database table initialization
- ✅ Thread safety and synchronization patterns

**Running the examples:**

```bash
# Modern .NET example
cd MDLSoft.DistributedLock.Example
dotnet run

# .NET Framework 4.0 example (requires Visual Studio or MSBuild)
cd MDLSoft.DistributedLock.Example.Net40
# Build with Visual Studio or MSBuild, then run the .exe
```

## API Reference

### IDistributedLockProvider

The main interface for acquiring distributed locks.

#### Methods

- `TryAcquireLock(string lockId, TimeSpan? timeout = null, string? userContext = null)` - Attempts to acquire a lock synchronously
- `TryAcquireLockAsync(string lockId, TimeSpan? timeout = null, string? userContext = null, CancellationToken cancellationToken = default)` - Attempts to acquire a lock asynchronously
- `AcquireLock(string lockId, TimeSpan? timeout = null, string? userContext = null)` - Acquires a lock synchronously (throws on failure)
- `AcquireLockAsync(string lockId, TimeSpan? timeout = null, string? userContext = null, CancellationToken cancellationToken = default)` - Acquires a lock asynchronously (throws on failure)

### IDistributedLock

Represents an acquired distributed lock.

#### Properties

- `string LockId` - The unique identifier for the lock
- `string? UserContext` - The user context associated with the lock
- `bool IsAcquired` - Whether the lock is currently acquired

#### Methods

- `Release()` - Releases the lock synchronously
- `ReleaseAsync(CancellationToken cancellationToken = default)` - Releases the lock asynchronously

### SqlServerDistributedLockProvider

SQL Server implementation of the distributed lock provider.

#### Constructor

```csharp
public SqlServerDistributedLockProvider(string connectionString, string tableName = "DistributedLocks")
```

#### Additional Methods

- `EnsureTableExists()` - Creates the locks table if it doesn't exist (sync)
- `EnsureTableExistsAsync(CancellationToken cancellationToken = default)` - Creates the locks table if it doesn't exist (async)

## Database Schema

The library automatically creates the following table structure:

```sql
CREATE TABLE [DistributedLocks] (
    [LockId] NVARCHAR(255) NOT NULL PRIMARY KEY,
    [LockToken] NVARCHAR(255) NOT NULL,
    [UserContext] NVARCHAR(MAX) NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE()
);
```

or


```sql
CREATE TABLE [DistributedLocks] (
    [LockId] NVARCHAR(255) NOT NULL PRIMARY KEY,
    [LockToken] NVARCHAR(255) NOT NULL,
    [UserContext] NVARCHAR(MAX) NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE()
);
```

## Configuration

### Connection Strings

The library supports:
- **System.Data.SqlClient** (for .NET Framework 4.0-4.8)  
- **Microsoft.Data.SqlClient** (for .NET Standard 2.0/.NET Core/.NET 5+)

### Custom Table Name

You can specify a custom table name for the locks:

```csharp
var provider = new SqlServerDistributedLockProvider(connectionString, "MyCustomLocksTable");
```

## Error Handling

The library includes specific exception types:

- `DistributedLockException` - Base exception for all lock operations
- `DistributedLockTimeoutException` - Thrown when a lock cannot be acquired within the timeout period
- `DistributedLockOperationException` - Thrown when a lock operation fails

## Best Practices

1. **Always use `using` statements** to ensure locks are properly released
2. **Use timeouts** when acquiring locks to avoid indefinite waiting
3. **Handle timeout exceptions** gracefully in your application
4. **Monitor lock duration** - Keep critical sections as short as possible
5. **Use meaningful lock IDs** that clearly identify the resource being protected

## Performance Considerations

- **Lightweight**: Zero external dependencies (except minimal framework backports)
- **Fast**: Raw ADO.NET operations with optimized SQL
- **Efficient**: Lock acquisition uses READ COMMITTED isolation with primary key constraints
- **Scalable**: Consider the database load when using many concurrent locks

## Thread Safety

The library is thread-safe and can be used from multiple threads simultaneously. Each lock acquisition creates a unique lock token to prevent conflicts.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

If you encounter any issues or have questions, please [open an issue](https://github.com/mdallago/MDLSoft.DistributedLock/issues) on GitHub.
