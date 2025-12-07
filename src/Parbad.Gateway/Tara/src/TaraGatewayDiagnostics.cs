// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Parbad.Gateway.Tara
{
    /// <summary>
    /// Diagnostic helpers for troubleshooting Tara gateway integration.
    /// </summary>
    public static class TaraGatewayDiagnostics
    {
        /// <summary>
        /// Enables detailed logging for Tara gateway requests.
        /// Logs will be written to Debug output.
        /// </summary>
        public static bool EnableDetailedLogging { get; set; }

        internal static void Log(string message)
        {
            if (EnableDetailedLogging)
            {
                Debug.WriteLine($"[Tara Gateway] {DateTime.Now:HH:mm:ss.fff} - {message}");
                Console.WriteLine($"[Tara Gateway] {DateTime.Now:HH:mm:ss.fff} - {message}");
            }
        }

        internal static void LogRequest(string endpoint, string url, string jsonPayload)
        {
            if (EnableDetailedLogging)
            {
                Log($"=== {endpoint} REQUEST ===");
                Log($"URL: {url}");
                Log($"Payload: {jsonPayload}");
                Log("=========================");
            }
        }

        internal static void LogResponse(string endpoint, int statusCode, string response)
        {
            if (EnableDetailedLogging)
            {
                Log($"=== {endpoint} RESPONSE ===");
                Log($"Status Code: {statusCode}");
                Log($"Response: {response}");
                Log("==========================");
            }
        }
    }
}

