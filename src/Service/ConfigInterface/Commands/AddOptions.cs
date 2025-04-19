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
    /// Add command options
    /// </summary>
    public class AddOptions : EntityOptions
    {
        public AddOptions(
            string source,
            IEnumerable<string> permissions,
            string entity,
            string? sourceType,
            IEnumerable<string>? sourceParameters,
            IEnumerable<string>? sourceKeyFields,
            string? restRoute,
            IEnumerable<string>? restMethodsForStoredProcedure,
            string? graphQLType,
            string? graphQLOperationForStoredProcedure,
            IEnumerable<string>? fieldsToInclude,
            IEnumerable<string>? fieldsToExclude,
            string? policyRequest,
            string? policyDatabase,
            string? config)
            : base(entity,
                  sourceType,
                  sourceParameters,
                  sourceKeyFields,
                  restRoute,
                  restMethodsForStoredProcedure,
                  graphQLType,
                  graphQLOperationForStoredProcedure,
                  fieldsToInclude,
                  fieldsToExclude,
                  policyRequest,
                  policyDatabase,
                  config)
        {
            Source = source;
            Permissions = permissions;
        }

        public string Source { get; }
        public IEnumerable<string> Permissions { get; }
        public int Handler(ILogger logger, FileSystemRuntimeConfigLoader loader, IFileSystem fileSystem)
        {
            logger.LogInformation("{productName} {version}", PRODUCT_NAME, ProductInfo.GetProductVersion());
            if (!IsEntityProvided(Entity, logger, command: "add"))
            {
                return -1;
            }

            bool isSuccess = ConfigGenerator.TryAddEntityToConfigWithOptions(this, loader, fileSystem);
            if (isSuccess)
            {
                logger.LogInformation("Added new entity: {Entity} with source: {Source} and permissions: {permissions}.", Entity, Source, string.Join(SEPARATOR, Permissions));
                logger.LogInformation("SUGGESTION: Use 'dab update [entity-name] [options]' to update any entities in your config.");
            }
            else
            {
                logger.LogError("Could not add entity: {Entity} with source: {Source} and permissions: {permissions}.", Entity, Source, string.Join(SEPARATOR, Permissions));
            }

            return isSuccess ? CliReturnCode.SUCCESS : CliReturnCode.GENERAL_ERROR;
        }
    }
}
