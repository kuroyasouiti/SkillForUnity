#!/bin/bash
# SkillForUnity Test Runner Script (Bash)
# Runs Unity Editor tests in batch mode

# Default values
UNITY_PATH="/Applications/Unity/Hub/Editor/2022.3.0f1/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="$(cd "$(dirname "$0")" && pwd)"
TEST_PLATFORM="EditMode"
RESULTS_PATH="TestResults.xml"

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --unity-path)
            UNITY_PATH="$2"
            shift 2
            ;;
        --project-path)
            PROJECT_PATH="$2"
            shift 2
            ;;
        --test-platform)
            TEST_PLATFORM="$2"
            shift 2
            ;;
        --results-path)
            RESULTS_PATH="$2"
            shift 2
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo "=== SkillForUnity Test Runner ==="
echo "Unity Path: $UNITY_PATH"
echo "Project Path: $PROJECT_PATH"
echo "Test Platform: $TEST_PLATFORM"
echo "Results Path: $RESULTS_PATH"
echo ""

# Check if Unity exists
if [ ! -f "$UNITY_PATH" ]; then
    echo "ERROR: Unity not found at: $UNITY_PATH"
    echo "Please specify the correct Unity path using --unity-path parameter"
    exit 1
fi

# Run tests
echo "Running tests..."

"$UNITY_PATH" \
    -runTests \
    -batchmode \
    -projectPath "$PROJECT_PATH" \
    -testResults "$RESULTS_PATH" \
    -testPlatform "$TEST_PLATFORM" \
    -logFile "TestLog.txt"

EXIT_CODE=$?

# Check results
echo ""
if [ $EXIT_CODE -eq 0 ]; then
    echo "✓ All tests passed!"
else
    echo "✗ Some tests failed. Exit code: $EXIT_CODE"
fi

# Display results if file exists
if [ -f "$RESULTS_PATH" ]; then
    echo ""
    echo "Test results saved to: $RESULTS_PATH"
    
    # Parse XML and display summary (requires xmllint)
    if command -v xmllint &> /dev/null; then
        echo ""
        echo "=== Test Summary ==="
        TOTAL=$(xmllint --xpath 'string(/test-run/@total)' "$RESULTS_PATH" 2>/dev/null)
        PASSED=$(xmllint --xpath 'string(/test-run/@passed)' "$RESULTS_PATH" 2>/dev/null)
        FAILED=$(xmllint --xpath 'string(/test-run/@failed)' "$RESULTS_PATH" 2>/dev/null)
        SKIPPED=$(xmllint --xpath 'string(/test-run/@skipped)' "$RESULTS_PATH" 2>/dev/null)
        
        echo "Total: $TOTAL"
        echo "Passed: $PASSED"
        echo "Failed: $FAILED"
        echo "Skipped: $SKIPPED"
    fi
fi

# Display log if exists
if [ -f "TestLog.txt" ]; then
    echo ""
    echo "Test log saved to: TestLog.txt"
fi

exit $EXIT_CODE

