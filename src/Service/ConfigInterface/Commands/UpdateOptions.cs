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
    /// Update command options
    /// </summary>
    public class UpdateOptions : EntityOptions
    {
        public UpdateOptions(
            string? source,
            IEnumerable<string>? permissions,
            string? relationship,
            string? cardinality,
            string? targetEntity,
            string? linkingObject,
            IEnumerable<string>? linkingSourceFields,
            IEnumerable<string>? linkingTargetFields,
            IEnumerable<string>? relationshipFields,
            IEnumerable<string>? map,
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
            string config)
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
            Relationship = relationship;
            Cardinality = cardinality;
            TargetEntity = targetEntity;
            LinkingObject = linkingObject;
            LinkingSourceFields = linkingSourceFields;
            LinkingTargetFields = linkingTargetFields;
            RelationshipFields = relationshipFields;
            Map = map;
        }

        public string? Source { get; }
        public IEnumerable<string>? Permissions { get; }

        public string? Relationship { get; }

        public string? Cardinality { get; }

        public string? TargetEntity { get; }

        public string? LinkingObject { get; }

        public IEnumerable<string>? LinkingSourceFields { get; }

        public IEnumerable<string>? LinkingTargetFields { get; }

        public IEnumerable<string>? RelationshipFields { get; }

        public IEnumerable<string>? Map { get; }

        public int Handler(ILogger logger, FileSystemRuntimeConfigLoader loader, IFileSystem fileSystem)
        {
            logger.LogInformation("{productName} {version}", PRODUCT_NAME, ProductInfo.GetProductVersion());
            if (!IsEntityProvided(Entity, logger, command: "update"))
            {
                return CliReturnCode.GENERAL_ERROR;
            }

            bool isSuccess = ConfigGenerator.TryUpdateEntityWithOptions(this, loader, fileSystem);

            if (isSuccess)
            {
                logger.LogInformation("Updated the entity: {Entity}.", Entity);
            }
            else
            {
                logger.LogError("Could not update the entity: {Entity}.", Entity);
            }

            return isSuccess ? CliReturnCode.SUCCESS : CliReturnCode.GENERAL_ERROR;
        }
    }
}
