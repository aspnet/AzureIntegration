// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.AzureAppServices.FunctionalTests
{
    internal static class ProcessExtensions
    {
        private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);

        public static void KillTree(this Process process)
        {
            process.KillTree(_defaultTimeout);
        }

        public static void KillTree(this Process process, TimeSpan timeout)
        {
            string stdout;
            RunProcessAndWaitForExit(
                "taskkill",
                $"/T /F /PID {process.Id}",
                timeout,
                out stdout);
        }

        private static int RunProcessAndWaitForExit(string fileName, string arguments, TimeSpan timeout, out string stdout)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = Process.Start(startInfo);

            stdout = null;
            if (process.WaitForExit((int)timeout.TotalMilliseconds))
            {
                stdout = process.StandardOutput.ReadToEnd();
            }
            else
            {
                process.Kill();
            }

            return process.ExitCode;
        }

        public static Task StartAndWaitForExitAsync(this Process subject)
        {
            var taskCompletionSource = new TaskCompletionSource<object>();

            subject.EnableRaisingEvents = true;

            subject.Exited += (s, a) =>
            {
                taskCompletionSource.SetResult(null);
            };

            subject.Start();

            return taskCompletionSource.Task;
        }
    }
}
