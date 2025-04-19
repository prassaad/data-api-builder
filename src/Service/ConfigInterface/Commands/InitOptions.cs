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
    /// Init command options
    /// </summary>
    public class InitOptions : Options
    {
        public InitOptions(
            DatabaseType databaseType,
            string? connectionString,
            string? cosmosNoSqlDatabase,
            string? cosmosNoSqlContainer,
            string? graphQLSchemaPath,
            bool setSessionContext,
            HostMode hostMode,
            IEnumerable<string>? corsOrigin,
            string authenticationProvider,
            string? audience = null,
            string? issuer = null,
            string restPath = RestRuntimeOptions.DEFAULT_PATH,
            string? runtimeBaseRoute = null,
            bool restDisabled = false,
            string graphQLPath = GraphQLRuntimeOptions.DEFAULT_PATH,
            bool graphqlDisabled = false,
            CliBool restEnabled = CliBool.None,
            CliBool graphqlEnabled = CliBool.None,
            CliBool restRequestBodyStrict = CliBool.None,
            CliBool multipleCreateOperationEnabled = CliBool.None,
            string? config = null)
            : base(config)
        {
            DatabaseType = databaseType;
            ConnectionString = connectionString;
            CosmosNoSqlDatabase = cosmosNoSqlDatabase;
            CosmosNoSqlContainer = cosmosNoSqlContainer;
            GraphQLSchemaPath = graphQLSchemaPath;
            SetSessionContext = setSessionContext;
            HostMode = hostMode;
            CorsOrigin = corsOrigin;
            AuthenticationProvider = authenticationProvider;
            Audience = audience;
            Issuer = issuer;
            RestPath = restPath;
            RuntimeBaseRoute = runtimeBaseRoute;
            RestDisabled = restDisabled;
            GraphQLPath = graphQLPath;
            GraphQLDisabled = graphqlDisabled;
            RestEnabled = restEnabled;
            GraphQLEnabled = graphqlEnabled;
            RestRequestBodyStrict = restRequestBodyStrict;
            MultipleCreateOperationEnabled = multipleCreateOperationEnabled;
        }

        public DatabaseType DatabaseType { get; }
        public string? ConnectionString { get; }
        public string? CosmosNoSqlDatabase { get; }
        public string? CosmosNoSqlContainer { get; }
        public string? GraphQLSchemaPath { get; }
        public bool SetSessionContext { get; }
        public HostMode HostMode { get; }
        public IEnumerable<string>? CorsOrigin { get; }
        public string AuthenticationProvider { get; }
        public string? Audience { get; }
        public string? Issuer { get; }
        public string RestPath { get; }
        public string? RuntimeBaseRoute { get; }
        public bool RestDisabled { get; }
        public string GraphQLPath { get; }
        public bool GraphQLDisabled { get; }
        public CliBool RestEnabled { get; }
        public CliBool GraphQLEnabled { get; }

        // Since the rest.request-body-strict option does not have a default value, it is required to specify a value for this option if it is
        // included in the init command.
        public CliBool RestRequestBodyStrict { get; }

        public CliBool MultipleCreateOperationEnabled { get; }

        public int Handler(ILogger logger, FileSystemRuntimeConfigLoader loader, IFileSystem fileSystem)
        {
            logger.LogInformation("{productName} {version}", PRODUCT_NAME, ProductInfo.GetProductVersion());
            bool isSuccess = ConfigGenerator.TryGenerateConfig(this, loader, fileSystem);
            if (isSuccess)
            {
                logger.LogInformation("Config file generated.");
                logger.LogInformation("SUGGESTION: Use 'dab add [entity-name] [options]' to add new entities in your config.");
                return CliReturnCode.SUCCESS;
            }
            else
            {
                logger.LogError("Could not generate config file.");
                return CliReturnCode.GENERAL_ERROR;
            }
        }
    }
}
