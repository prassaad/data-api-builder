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
    /// Start command options
    /// </summary>
    public class StartOptions : Options
    {
        private const string LOGLEVEL_HELPTEXT = "Specifies logging level as provided value. For possible values, see: https://go.microsoft.com/fwlink/?linkid=2263106";

        public StartOptions(bool verbose, LogLevel? logLevel, bool isHttpsRedirectionDisabled, string config)
            : base(config)
        {
            // When verbose is true we set LogLevel to information.
            LogLevel = verbose is true ? Microsoft.Extensions.Logging.LogLevel.Information : logLevel;
            IsHttpsRedirectionDisabled = isHttpsRedirectionDisabled;
        }

        // SetName defines mutually exclusive sets, ie: can not have
        // both verbose and LogLevel.
        public bool Verbose { get; }
        public LogLevel? LogLevel { get; }
        public bool IsHttpsRedirectionDisabled { get; }

        public int Handler(ILogger logger, FileSystemRuntimeConfigLoader loader, IFileSystem fileSystem)
        {
            logger.LogInformation("{productName} {version}", PRODUCT_NAME, ProductInfo.GetProductVersion());
            bool isSuccess = ConfigGenerator.TryStartEngineWithOptions(this, loader, fileSystem);

            if (!isSuccess)
            {
                logger.LogError("Failed to start the engine.");
            }

            return isSuccess ? CliReturnCode.SUCCESS : CliReturnCode.GENERAL_ERROR;
        }
    }
}
