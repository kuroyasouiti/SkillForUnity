using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace MCP.Editor
{
    internal readonly struct CommandResult
    {
        public CommandResult(bool success, int exitCode, string output, string error)
        {
            Success = success;
            ExitCode = exitCode;
            Output = output;
            Error = error;
        }

        public bool Success { get; }
        public int ExitCode { get; }
        public string Output { get; }
        public string Error { get; }
    }

    internal static class ProcessHelper
    {
        public static void RunShellCommandAsync(string command, string workingDirectory, Action<CommandResult> callback)
        {
            Task.Run(() =>
            {
                var result = RunShellCommand(command, workingDirectory);
                EditorApplication.delayCall += () => callback?.Invoke(result);
            });
        }

        private static CommandResult RunShellCommand(string command, string workingDirectory)
        {
            try
            {
                var effectiveWorkingDirectory = string.IsNullOrWhiteSpace(workingDirectory)
                    ? Directory.GetCurrentDirectory()
                    : workingDirectory;
                var startInfo = CreateProcessStartInfo(command, effectiveWorkingDirectory);
                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    return new CommandResult(false, -1, string.Empty, "Failed to start process.");
                }

                var output = new StringBuilder();
                var error = new StringBuilder();

                process.OutputDataReceived += (_, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        output.AppendLine(args.Data);
                    }
                };

                process.ErrorDataReceived += (_, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        error.AppendLine(args.Data);
                    }
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                return new CommandResult(process.ExitCode == 0, process.ExitCode, output.ToString(), error.ToString());
            }
            catch (Exception ex)
            {
                return new CommandResult(false, -1, string.Empty, ex.Message);
            }
        }

        private static ProcessStartInfo CreateProcessStartInfo(string command, string workingDirectory)
        {
#if UNITY_EDITOR_WIN
            return new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
#else
            var escapedCommand = command.Replace("\"", "\\\"");
            return new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-lc \"{escapedCommand}\"",
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
#endif
        }
    }
}
