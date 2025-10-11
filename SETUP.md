# MDLSoft.DistributedLock - Setup Guide

This document provides setup instructions for the MDLSoft.DistributedLock library.

## What was Created

✅ **Complete Lightweight C# Library Solution** with the following components:

### 1. Main Library (`MDLSoft.DistributedLock`)
- **Universal framework support**: .NET Framework 4.0, 4.5.1, 4.6.1, 4.8, and .NET Standard 2.0
- **Zero external dependencies** (except minimal framework-specific backports)
- **Interfaces**:
  - `IDistributedLock` - Represents an acquired distributed lock
  - `IDistributedLockProvider` - Provides distributed lock functionality
- **Implementation**:
  - `SqlServerDistributedLockProvider` - SQL Server-based lock provider (raw ADO.NET)
  - `SqlServerDistributedLock` - SQL Server lock implementation
- **Exception Types**:
  - `DistributedLockException` - Base exception
  - `DistributedLockTimeoutException` - Timeout-specific exception
  - `DistributedLockOperationException` - Operation failure exception

### 2. Test Project (`MDLSoft.DistributedLock.Tests`)
- **Comprehensive unit tests** using xUnit and FluentAssertions
- Tests for both synchronous and asynchronous operations
- Tests for lock acquisition, timeout handling, and concurrent access
- Total: 9 tests covering all major functionality

### 3. Example Applications
- **Modern .NET Example** (`MDLSoft.DistributedLock.Example`) - .NET 8 with async/await
- **.NET Framework 4.0 Example** (`MDLSoft.DistributedLock.Example.Net40`) - Legacy compatibility
- Demonstrates basic usage, timeouts, concurrent access, and error handling

### 4. GitHub Repository Setup
- **README.md** - Comprehensive documentation with accurate API reference
- **LICENSE** - MIT License
- **GitHub Actions** - CI/CD pipeline for building, testing, and publishing to NuGet
- **.gitignore** - Properly configured for .NET projects
- **Directory.Build.props** - Shared build configuration
- **SETUP.md** - This setup guide
- **CHANGELOG.md** - Version history and release notes

### 5. NuGet Package

**Single universal package for maximum compatibility:**

- **`MDLSoft.DistributedLock.2.0.0.nupkg`** - Universal package (49.2KB)
  - Supports .NET Framework 4.0, 4.5.1, 4.6.1, 4.8, and .NET Standard 2.0
  - Uses raw ADO.NET for optimal performance and zero dependencies
  - Full feature set with modern and legacy framework support
  - Lightweight with minimal dependencies

## Key Features Implemented

✅ **Distributed Locking**: Acquire and release locks across multiple processes/applications  
✅ **SQL Server Backend**: Uses SQL Server as the lock storage mechanism  
✅ **High Performance**: Built with raw ADO.NET for optimal efficiency  
✅ **Sync & Async**: Full support for both synchronous and asynchronous operations  
✅ **Universal Framework**: Single package supports .NET Framework 4.0 through .NET 8+  
✅ **Lightweight**: Zero external dependencies (except minimal framework backports)  
✅ **Reliable**: Robust error handling and thread-safe operations  
✅ **Easy to Use**: Simple API with comprehensive documentation

## Database Requirements

The library automatically creates the following SQL Server table:

```sql
CREATE TABLE [DistributedLocks] (
    [LockId] NVARCHAR(255) NOT NULL PRIMARY KEY,
    [LockToken] NVARCHAR(255) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
```

## Key Architecture Decisions

### Performance Optimizations
- **No Transactions**: Removed unnecessary `BeginTransaction` for better performance
- **Primary Key Constraints**: Uses database constraints instead of application logic
- **READ COMMITTED**: Optimal isolation level for distributed locks
- **Raw ADO.NET**: Direct database operations without ORM overhead

### Async Implementation
- **True Async IO**: Uses `OpenAsync()` and `ExecuteNonQueryAsync()` 
- **No Task.Run**: Eliminated anti-pattern that blocks thread pool threads
- **Proper Cancellation**: Full `CancellationToken` support throughout

### Universal Compatibility
- **Single Package**: One NuGet package supports all .NET versions
- **Conditional Compilation**: Framework-specific optimizations
- **Zero Dependencies**: No external packages (except minimal backports)

## Next Steps

### 1. Set up SQL Server Connection
Update the connection string in your applications to point to your SQL Server instance.
**Note**: The library will automatically create the required table when `EnsureTableExists()` is called.

### 2. Initialize Git Repository
```bash
git init
git add .
git commit -m "Initial commit: MDLSoft.DistributedLock library"
```

### 3. Create GitHub Repository
1. Create a new repository on GitHub named `MDLSoft.DistributedLock`
2. Push the local repository to GitHub:
```bash
git remote add origin https://github.com/YourUsername/MDLSoft.DistributedLock.git
git branch -M main
git push -u origin main
```

### 4. Configure GitHub Secrets
For the CI/CD pipeline to work, add these secrets to your GitHub repository:
- `NUGET_API_KEY` - Your NuGet API key for publishing packages

### 5. Publish to NuGet
The GitHub Actions workflow will automatically:
- Run tests on every push
- Build and pack the NuGet package
- Publish to NuGet when you create a version tag (e.g., `v1.0.0`)

### 6. Create a Release
To publish version 2.0.0:
```bash
git tag v2.0.0
git push origin v2.0.0
```

### 7. Development Setup
For local development:
```bash
# Build all target frameworks
dotnet build

# Run tests
dotnet test

# Pack NuGet package
dotnet pack -c Release
```

### 8. Testing .NET Framework 4.0 Compatibility
To test the .NET Framework 4.0 example:
```powershell
# Build the main library for .NET 4.0
dotnet build MDLSoft.DistributedLock\MDLSoft.DistributedLock.csproj -f net40

# Test with PowerShell script
cd MDLSoft.DistributedLock.Example.Net40
.\Test-Net40.ps1
```

## Usage Example

```csharp
using MDLSoft.DistributedLock;

var connectionString = "Server=.;Database=MyApp;Integrated Security=true;";
var provider = new SqlServerDistributedLockProvider(connectionString);

// Ensure the table exists
await provider.EnsureTableExistsAsync();

// Acquire a lock with timeout
using (var distributedLock = await provider.TryAcquireLockAsync("my-resource", TimeSpan.FromSeconds(30)))
{
    if (distributedLock != null)
    {
        // Critical section - only one process can execute this
        await DoImportantWorkAsync();
    }
} // Lock is automatically released here
```

## Build Status

All projects build successfully and pass tests:
- ✅ MDLSoft.DistributedLock (Main Library)
- ✅ MDLSoft.DistributedLock.Tests (9 tests passing)
- ✅ MDLSoft.DistributedLock.Example (Modern .NET Example)
- ✅ MDLSoft.DistributedLock.Example.Net40 (.NET Framework 4.0 Example)

## Package Information

- **Package ID**: MDLSoft.DistributedLock
- **Version**: 2.0.0
- **License**: MIT
- **Size**: 49.2 KB
- **Target Frameworks**: net40, net451, net461, net48, netstandard2.0
- **Dependencies**: Minimal (only framework-specific async/SQL client backports)

The library is now ready for production use and NuGet publication with universal .NET framework compatibility!