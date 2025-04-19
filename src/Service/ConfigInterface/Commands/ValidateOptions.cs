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
    /// Validate command options
    /// </summary>
    public class ValidateOptions : Options
    {
        public ValidateOptions(string config)
            : base(config)
        { }

        /// <summary>
        /// This Handler method is responsible for validating the config file and is called when `validate`
        /// command is invoked.
        /// </summary>
        public int Handler(ILogger logger, FileSystemRuntimeConfigLoader loader, IFileSystem fileSystem)
        {
            logger.LogInformation("{productName} {version}", PRODUCT_NAME, ProductInfo.GetProductVersion());
            bool isValidConfig = ConfigGenerator.IsConfigValid(this, loader, fileSystem);

            if (isValidConfig)
            {
                logger.LogInformation("Config is valid.");
            }
            else
            {
                logger.LogError("Config is invalid. Check above logs for details.");
            }

            return isValidConfig ? CliReturnCode.SUCCESS : CliReturnCode.GENERAL_ERROR;
        }
    }
}
