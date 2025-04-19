// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Abstractions;
using Azure.DataApiBuilder.Config;
using Azure.DataApiBuilder.Config.ObjectModel;
using Azure.DataApiBuilder.Product;
using ConfigInterface.Constants;
using static ConfigInterface.Utils;

namespace ConfigInterface.Commands
{
    /// <summary>
    /// Telemetry command options
    /// </summary>
    public class AddTelemetryOptions : Options
    {
        public AddTelemetryOptions(string appInsightsConnString, CliBool appInsightsEnabled, string? config) : base(config)
        {
            AppInsightsConnString = appInsightsConnString;
            AppInsightsEnabled = appInsightsEnabled;
        }

        // Connection string for the Application Insights resource to which telemetry data should be sent.
        // This option  is required and must be provided with a valid connection string.
        public string AppInsightsConnString { get; }

        // To specify whether Application Insights telemetry should be enabled. This flag is optional and default value is true.
        public CliBool AppInsightsEnabled { get; }

        public int Handler(ILogger logger, FileSystemRuntimeConfigLoader loader, IFileSystem fileSystem)
        {
            logger.LogInformation("{productName} {version}", PRODUCT_NAME, ProductInfo.GetProductVersion());

            bool isSuccess = ConfigGenerator.TryAddTelemetry(this, loader, fileSystem);

            if (isSuccess)
            {
                logger.LogInformation("Successfully added telemetry to the configuration file.");
            }
            else
            {
                logger.LogError("Failed to add telemetry to the configuration file.");
            }

            return isSuccess ? CliReturnCode.SUCCESS : CliReturnCode.GENERAL_ERROR;
        }
    }
}
