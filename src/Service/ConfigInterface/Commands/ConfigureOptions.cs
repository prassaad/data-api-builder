// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Abstractions;
using Azure.DataApiBuilder.Config;
using Azure.DataApiBuilder.Product;
using ConfigInterface.Constants;
using static ConfigInterface.Utils;

namespace ConfigInterface.Commands
{
    /// <summary>
    /// Configure command options
    /// This command will be used to configure non-entity config properties.
    /// </summary>
    public class ConfigureOptions : Options
    {
        public ConfigureOptions(
            int? depthLimit = null,
            string? config = null)
            : base(config)
        {
            DepthLimit = depthLimit;
        }

        public int? DepthLimit { get; }

        public int Handler(ILogger logger, FileSystemRuntimeConfigLoader loader, IFileSystem fileSystem)
        {
            logger.LogInformation("{productName} {version}", PRODUCT_NAME, ProductInfo.GetProductVersion());
            bool isSuccess = ConfigGenerator.TryConfigureSettings(this, loader, fileSystem);
            if (isSuccess)
            {
                logger.LogInformation("Successfully updated runtime settings in the config file.");
                return CliReturnCode.SUCCESS;
            }
            else
            {
                logger.LogError("Failed to update runtime settings in the config file.");
                return CliReturnCode.GENERAL_ERROR;
            }
        }
    }
}
