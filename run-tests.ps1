# SkillForUnity Test Runner Script (PowerShell)
# Runs Unity Editor tests in batch mode

param(
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\2022.3.0f1\Editor\Unity.exe",
    [string]$ProjectPath = $PSScriptRoot,
    [string]$TestPlatform = "EditMode",
    [string]$ResultsPath = "TestResults.xml"
)

Write-Host "=== SkillForUnity Test Runner ===" -ForegroundColor Cyan
Write-Host "Unity Path: $UnityPath"
Write-Host "Project Path: $ProjectPath"
Write-Host "Test Platform: $TestPlatform"
Write-Host "Results Path: $ResultsPath"
Write-Host ""

# Check if Unity exists
if (-not (Test-Path $UnityPath)) {
    Write-Host "ERROR: Unity not found at: $UnityPath" -ForegroundColor Red
    Write-Host "Please specify the correct Unity path using -UnityPath parameter" -ForegroundColor Yellow
    exit 1
}

# Run tests
Write-Host "Running tests..." -ForegroundColor Green

$arguments = @(
    "-runTests",
    "-batchmode",
    "-projectPath", "`"$ProjectPath`"",
    "-testResults", "`"$ResultsPath`"",
    "-testPlatform", $TestPlatform,
    "-logFile", "TestLog.txt"
)

$process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow

# Check results
Write-Host ""
if ($process.ExitCode -eq 0) {
    Write-Host "✓ All tests passed!" -ForegroundColor Green
} else {
    Write-Host "✗ Some tests failed. Exit code: $($process.ExitCode)" -ForegroundColor Red
}

# Display results if file exists
if (Test-Path $ResultsPath) {
    Write-Host ""
    Write-Host "Test results saved to: $ResultsPath" -ForegroundColor Cyan
    
    # Parse XML and display summary
    [xml]$results = Get-Content $ResultsPath
    $testRun = $results.'test-run'
    
    Write-Host ""
    Write-Host "=== Test Summary ===" -ForegroundColor Cyan
    Write-Host "Total: $($testRun.total)"
    Write-Host "Passed: $($testRun.passed)" -ForegroundColor Green
    Write-Host "Failed: $($testRun.failed)" -ForegroundColor $(if ($testRun.failed -eq "0") { "Green" } else { "Red" })
    Write-Host "Skipped: $($testRun.skipped)" -ForegroundColor Yellow
}

# Display log if exists
if (Test-Path "TestLog.txt") {
    Write-Host ""
    Write-Host "Test log saved to: TestLog.txt" -ForegroundColor Cyan
}

exit $process.ExitCode

