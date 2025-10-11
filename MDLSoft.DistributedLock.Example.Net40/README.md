# MDLSoft.DistributedLock Example for .NET Framework 4.0

This is a comprehensive example application demonstrating how to use MDLSoft.DistributedLock on .NET Framework 4.0.

## 🚨 **Important Note**

This project uses the **classic .csproj format** and is designed to be built with **Visual Studio** or **MSBuild** directly, not with `dotnet build`. The `dotnet` CLI has limited support for old-style project files.

## Building and Running

### Prerequisites
- .NET Framework 4.0 or later
- SQL Server or SQL Server LocalDB  
- **Visual Studio 2010 or later** (recommended)
- **OR MSBuild tools** (if available)

### Building Options

#### Option 1: Visual Studio (Recommended)
1. Open `MDLSoft.DistributedLock.sln` in Visual Studio
2. Right-click on `MDLSoft.DistributedLock.Example.Net40` project
3. Select "Build" or "Rebuild"
4. Run the resulting executable

#### Option 2: MSBuild (Command Line)
```cmd
# If MSBuild is in PATH
msbuild MDLSoft.DistributedLock.Example.Net40.csproj /p:Configuration=Debug

# Or use full path to MSBuild
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" MDLSoft.DistributedLock.Example.Net40.csproj
```

#### Option 3: Copy DLL Manually
If you can't build the project:
1. Build main library: `dotnet build ..\MDLSoft.DistributedLock\MDLSoft.DistributedLock.csproj -f net40`
2. Copy the program source and DLL to your own .NET 4.0 project
3. Add reference to the compiled DLL

### Testing Without Building
Use the included PowerShell test script:
```powershell
.\Test-Net40.ps1
```
This script loads the compiled .NET 4.0 DLL directly and tests all functionality.

## What This Example Demonstrates

### 1. Basic Lock Usage
- Acquiring and releasing locks
- Checking lock status
- Proper error handling

### 2. Lock Timeouts
- Setting lock acquisition timeouts
- Handling timeout scenarios
- Demonstrating lock conflicts

### 3. Concurrent Access Simulation
- Multiple threads competing for the same lock
- Only one thread can acquire the lock at a time
- Proper synchronization patterns

### 4. Using Statement Pattern
- Automatic lock release through IDisposable
- Exception-safe lock handling
- Resource cleanup

### 5. Error Handling
- Invalid parameters (null/empty lock IDs)
- Timeout exceptions
- Database connection issues
- Proper exception handling patterns

## Key Features Shown

- ✅ **Synchronous Operations**: All operations use sync methods (no async/await in .NET 4.0)
- ✅ **Thread Safety**: Demonstrates thread-safe lock operations
- ✅ **Resource Management**: Proper disposal and cleanup patterns
- ✅ **Error Resilience**: Comprehensive error handling
- ✅ **Configuration**: App.config connection string management

## .NET Framework 4.0 Specific Features

This example is specifically designed for .NET Framework 4.0 and demonstrates:

- Using `System.Data.SqlClient` (not Microsoft.Data.SqlClient)
- Classic .csproj format with Framework references
- Threading with `Thread` class (no Task.Run)
- Configuration with `app.config`
- Classic using statements and disposal patterns

## Code Structure

```
Program.cs
├── Main()                          # Entry point and initialization
├── InitializeLockProvider()        # Setup connection and provider
├── Example1_BasicLockUsage()       # Basic acquire/release cycle
├── Example2_LockTimeout()          # Timeout handling
├── Example3_ConcurrentAccess()     # Multi-threading demonstration
├── Example4_UsingStatement()       # IDisposable pattern
└── Example5_ErrorHandling()        # Exception scenarios
```

## Expected Output

When run successfully, you should see output similar to:

```
=== MDLSoft.DistributedLock Example (.NET Framework 4.0) ===

Using connection string: Server=***
✓ Lock table created/verified

--- Example 1: Basic Lock Usage ---
✓ Lock acquired: basic-example-123456789
  Performing work...
✓ Lock released

--- Example 2: Lock with Timeout ---
✓ First lock acquired: timeout-example-123456789
✓ Second lock correctly timed out
✓ First lock released
✓ Third lock acquired after first was released
✓ Third lock released

... (additional examples)

=== All examples completed successfully! ===
```

## Troubleshooting

### Common Issues

1. **"SQL Server not available"**
   - Install SQL Server LocalDB: `SqlLocalDB.exe create MSSQLLocalDB`
   - Or update connection string to point to your SQL Server instance

2. **"Cannot find MDLSoft.DistributedLock.dll"**
   - Build the main library first: `dotnet build ..\MDLSoft.DistributedLock\MDLSoft.DistributedLock.csproj -f net40`
   - Ensure the .NET 4.0 DLL exists at: `..\MDLSoft.DistributedLock\bin\Debug\net40\MDLSoft.DistributedLock.dll`

3. **"System.Data.SqlClient not found"**
   - This should be included in .NET Framework 4.0 by default
   - Check that you're targeting .NET Framework 4.0 (not .NET Core)

### Manual Testing

If you can't build the project, you can test the library functionality with the included PowerShell script:

```powershell
# Run the test script
.\Test-Net40.ps1
```

This script:
- Loads the compiled .NET 4.0 DLL directly
- Tests all core functionality without requiring project compilation
- Validates API compatibility
- Demonstrates proper usage patterns

## Framework Compatibility

This example specifically targets **.NET Framework 4.0** to demonstrate:
- Backwards compatibility with older .NET Framework versions
- Classic project format support (.csproj vs PackageReference)
- Synchronous-only operations (async/await introduced in .NET 4.5)
- Traditional configuration and deployment patterns
- Threading with `Thread` class (pre-Task Parallel Library)

For modern .NET applications (.NET 5+, .NET Core, .NET Standard), see the main example project (`MDLSoft.DistributedLock.Example`) which targets .NET 8 and includes async operations, modern C# features, and PackageReference-style project format.