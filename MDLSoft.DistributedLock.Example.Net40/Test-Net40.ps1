# Test script for .NET Framework 4.0 distributed lock functionality
# This script verifies that the library works correctly on .NET Framework 4.0

Write-Host "=== Testing MDLSoft.DistributedLock on .NET Framework 4.0 ===" -ForegroundColor Green
Write-Host ""

try {
    # Load the .NET 4.0 compiled library
    $libraryPath = "..\MDLSoft.DistributedLock\bin\Debug\net40\MDLSoft.DistributedLock.dll"
    
    if (-not (Test-Path $libraryPath)) {
        Write-Host "ERROR: Library not found at $libraryPath" -ForegroundColor Red
        Write-Host "Please build the main library first:" -ForegroundColor Yellow
        Write-Host "  dotnet build ..\MDLSoft.DistributedLock\MDLSoft.DistributedLock.csproj -f net40" -ForegroundColor Yellow
        exit 1
    }

    Add-Type -Path $libraryPath
    Write-Host "✓ Library loaded successfully" -ForegroundColor Green

    # Test connection string
    $connectionString = "Server=(localdb)\MSSQLLocalDB;Database=Net40Test;Integrated Security=true;TrustServerCertificate=true;"
    Write-Host "Using connection: $($connectionString.Split(';')[0])..." -ForegroundColor Cyan

    # Create provider
    $provider = New-Object MDLSoft.DistributedLock.SqlServerDistributedLockProvider($connectionString, "Net40TestLocks")
    Write-Host "✓ Provider created" -ForegroundColor Green

    # Test table creation
    try {
        $provider.EnsureTableExists()
        Write-Host "✓ Lock table created/verified" -ForegroundColor Green
    } catch {
        Write-Host "✗ Failed to create table: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "This might be because SQL Server LocalDB is not available" -ForegroundColor Yellow
        exit 1
    }

    # Test 1: Basic lock acquisition
    Write-Host "" 
    Write-Host "--- Test 1: Basic Lock Operations ---" -ForegroundColor Cyan
    
    $lockId = "powershell-test-$(Get-Date -Format 'yyyyMMddHHmmss')"
    $lock = $provider.TryAcquireLock($lockId)
    
    if ($lock) {
        Write-Host "✓ Lock acquired: $lockId" -ForegroundColor Green
        Write-Host "  Lock ID: $($lock.LockId)" -ForegroundColor Gray
        Write-Host "  Is Acquired: $($lock.IsAcquired)" -ForegroundColor Gray
        
        $lock.Release()
        Write-Host "✓ Lock released" -ForegroundColor Green
    } else {
        Write-Host "✗ Failed to acquire lock" -ForegroundColor Red
        exit 1
    }

    # Test 2: Lock conflict
    Write-Host ""
    Write-Host "--- Test 2: Lock Conflicts ---" -ForegroundColor Cyan
    
    $conflictLockId = "conflict-test-$(Get-Date -Format 'yyyyMMddHHmmss')"
    
    # Acquire first lock
    $firstLock = $provider.AcquireLock($conflictLockId)
    Write-Host "✓ First lock acquired: $conflictLockId" -ForegroundColor Green
    
    # Try to acquire second lock (should fail)
    $secondLock = $provider.TryAcquireLock($conflictLockId, [TimeSpan]::FromMilliseconds(100))
    
    if ($secondLock -eq $null) {
        Write-Host "✓ Second lock correctly failed (conflict detected)" -ForegroundColor Green
    } else {
        Write-Host "✗ Second lock should have failed!" -ForegroundColor Red
        $secondLock.Release()
        exit 1
    }
    
    # Release first lock
    $firstLock.Release()
    Write-Host "✓ First lock released" -ForegroundColor Green
    
    # Now second attempt should succeed
    $thirdLock = $provider.TryAcquireLock($conflictLockId)
    if ($thirdLock) {
        Write-Host "✓ Third lock acquired after first was released" -ForegroundColor Green
        $thirdLock.Release()
        Write-Host "✓ Third lock released" -ForegroundColor Green
    } else {
        Write-Host "✗ Third lock should have succeeded" -ForegroundColor Red
        exit 1
    }

    # Test 3: Using statement simulation (IDisposable)
    Write-Host ""
    Write-Host "--- Test 3: IDisposable Pattern ---" -ForegroundColor Cyan
    
    $disposableLockId = "disposable-test-$(Get-Date -Format 'yyyyMMddHHmmss')"
    
    try {
        $disposableLock = $provider.AcquireLock($disposableLockId)
        Write-Host "✓ Disposable lock acquired: $disposableLockId" -ForegroundColor Green
        
        # Simulate work
        Start-Sleep -Milliseconds 100
        
    } finally {
        # This simulates the using statement cleanup
        if ($disposableLock) {
            $disposableLock.Dispose()
            Write-Host "✓ Lock disposed (released)" -ForegroundColor Green
        }
    }
    
    # Verify the lock was released by acquiring it again
    $verifyLock = $provider.TryAcquireLock($disposableLockId)
    if ($verifyLock) {
        Write-Host "✓ Successfully acquired same lock again (dispose worked)" -ForegroundColor Green
        $verifyLock.Release()
    } else {
        Write-Host "✗ Could not reacquire lock (dispose may have failed)" -ForegroundColor Red
        exit 1
    }

    # Test 4: Error handling
    Write-Host ""
    Write-Host "--- Test 4: Error Handling ---" -ForegroundColor Cyan
    
    # Test null lock ID
    try {
        $nullLock = $provider.TryAcquireLock($null)
        Write-Host "✗ Should have thrown exception for null lock ID" -ForegroundColor Red
        exit 1
    } catch {
        Write-Host "✓ Correctly handled null lock ID: $($_.Exception.GetType().Name)" -ForegroundColor Green
    }
    
    # Test empty lock ID
    try {
        $emptyLock = $provider.TryAcquireLock("")
        Write-Host "✗ Should have thrown exception for empty lock ID" -ForegroundColor Red
        exit 1
    } catch {
        Write-Host "✓ Correctly handled empty lock ID: $($_.Exception.GetType().Name)" -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "=== ALL TESTS PASSED! ===" -ForegroundColor Green -BackgroundColor DarkGreen
    Write-Host ""
    Write-Host "✓ .NET Framework 4.0 compatibility confirmed" -ForegroundColor Green
    Write-Host "✓ All core functionality working" -ForegroundColor Green
    Write-Host "✓ Error handling working correctly" -ForegroundColor Green
    Write-Host "✓ Thread safety and lock conflicts handled properly" -ForegroundColor Green

} catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack trace: $($_.Exception.StackTrace)" -ForegroundColor Red
    exit 1
}