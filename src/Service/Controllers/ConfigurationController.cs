// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Azure.DataApiBuilder.Config;
using Azure.DataApiBuilder.Config.ObjectModel;
using Azure.DataApiBuilder.Core.Configurations;
using Azure.DataApiBuilder.Core.Models;
using Azure.DataApiBuilder.Core.Services;
using Azure.DataApiBuilder.Core.Services.MetadataProviders;
using Azure.DataApiBuilder.Service.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.DataApiBuilder.Service.Controllers
{
    [ApiController]
    [Route("dab/[controller]")]
    public class ConfigurationController : Controller
    {
        RuntimeConfigProvider _configurationProvider;
        private readonly ILogger<ConfigurationController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        public ConfigurationController(
            RuntimeConfigProvider configurationProvider,
            ILogger<ConfigurationController> logger,
            IConfiguration configuration,
            IServiceProvider serviceProvider
        )
        {
            _configurationProvider = configurationProvider;
            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Takes in the runtime configuration, configuration overrides, schema and access token configures the runtime.
        /// If the runtime is already configured, it will return a conflict result.
        /// </summary>
        /// <param name="configuration">Runtime configuration, config overrides, schema and access token.</param>
        /// <returns>Ok in case of success, Bad request on bad config
        /// or Conflict if the runtime is already configured </returns>
        [HttpPost("v2")]
        public async Task<ActionResult> Index([FromBody] ConfigurationPostParametersV2 configuration)
        {
            if (_configurationProvider.TryGetConfig(out _))
            {
                return new ConflictResult();
            }

            try
            {
                string mergedConfiguration = MergeJsonProvider.Merge(configuration.Configuration, configuration.ConfigurationOverrides);

                bool initResult = await _configurationProvider.Initialize(
                    mergedConfiguration,
                    configuration.Schema,
                    configuration.AccessToken);

                if (initResult && _configurationProvider.TryGetConfig(out _))
                {
                    return Ok();
                }
                else
                {
                    _logger.LogError(
                        message: "{correlationId} Failed to initialize configuration.",
                        HttpContextExtensions.GetLoggerCorrelationId(HttpContext));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(
                    exception: e,
                    message: "{correlationId} Exception during configuration initialization.",
                    HttpContextExtensions.GetLoggerCorrelationId(HttpContext));
            }

            return BadRequest();
        }

        /// <summary>
        /// Takes in the runtime configuration, schema, connection string and access token and configures the runtime.
        /// If the runtime is already configured, it will return a conflict result.
        /// </summary>
        /// <param name="configuration">Runtime configuration, schema, connection string and access token.</param>
        /// <returns>Ok in case of success, Bad request on bad config
        /// or Conflict if the runtime is already configured </returns>
        public async Task<ActionResult> Index([FromBody] ConfigurationPostParameters configuration)
        {
            if (_configurationProvider.TryGetConfig(out _))
            {
                return new ConflictResult();
            }

            try
            {
                bool initResult = await _configurationProvider.Initialize(
                    configuration.Configuration,
                    configuration.Schema,
                    configuration.ConnectionString,
                    configuration.AccessToken,
                    replaceEnvVar: false,
                    replacementFailureMode: Config.Converters.EnvironmentVariableReplacementFailureMode.Ignore);

                if (initResult && _configurationProvider.TryGetConfig(out _))
                {
                    return Ok();
                }
                else
                {
                    _logger.LogError(
                        message: "{correlationId} Failed to initialize configuration.",
                        HttpContextExtensions.GetLoggerCorrelationId(HttpContext));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(
                    exception: e,
                    message: "{correlationId} Exception during configuration initialization.",
                    HttpContextExtensions.GetLoggerCorrelationId(HttpContext));
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("addentity")]
        public async Task<ActionResult> AddEntity()
        {
            string configFileName = _configuration.GetValue<string>("ConfigFileName")
               ?? FileSystemRuntimeConfigLoader.DEFAULT_CONFIG_FILE_NAME;

            string? connectionString = _configuration.GetValue<string?>(
                FileSystemRuntimeConfigLoader.RUNTIME_ENV_CONNECTION_STRING
                    .Replace(FileSystemRuntimeConfigLoader.ENVIRONMENT_PREFIX, ""),
                null);

            // Create service instances
            FileSystem fileSystem = new();
            FileSystemRuntimeConfigLoader configLoader = new(fileSystem);
            configLoader.UpdateConfigFilePath(configFileName);

            RuntimeConfigProvider configProvider = new(configLoader);

            if (configProvider.TryGetConfig(out RuntimeConfig? runtimeConfig) && runtimeConfig.DataSource.DatabaseType is DatabaseType.PostgreSQL)
            {
                configProvider.IsLateConfigured = true;
            }
            var scope = _serviceProvider.CreateScope();
            var scopeServiceProvider = scope.ServiceProvider;
            var isRuntimeReady = PerformOnConfigChangeAsync(scopeServiceProvider, configProvider);


            return new JsonResult(runtimeConfig);

        }

        private async Task<bool> PerformOnConfigChangeAsync(IServiceProvider scopeServiceProvider, RuntimeConfigProvider runtimeConfigProvider)
        {
            try
            {
                //RuntimeConfigProvider runtimeConfigProvider = scopeServiceProvider.GetService<RuntimeConfigProvider>()!;
                RuntimeConfig runtimeConfig = runtimeConfigProvider.GetConfig();

                RuntimeConfigValidator runtimeConfigValidator = scopeServiceProvider.GetService<RuntimeConfigValidator>()!;
                // Now that the configuration has been set, perform validation of the runtime config
                // itself.

                runtimeConfigValidator.ValidateConfigProperties();

                if (runtimeConfig.IsDevelopmentMode())
                {
                    // Running only in developer mode to ensure fast and smooth startup in production.
                    runtimeConfigValidator.ValidatePermissionsInConfig(runtimeConfig);
                    //runtimeConfigValidator.ValidateEntitiesMetadata(runtimeConfig);
                }

                IMetadataProviderFactory sqlMetadataProviderFactory =
                    scopeServiceProvider.GetRequiredService<IMetadataProviderFactory>();


                if (sqlMetadataProviderFactory is not null)
                {
                    await sqlMetadataProviderFactory.SetDynamicRuntimeConfigProvider(runtimeConfigProvider, runtimeConfig);
                    await sqlMetadataProviderFactory.InitializeAsync();
                }

                // Manually trigger DI service instantiation of GraphQLSchemaCreator and RestService
                // to attempt to reduce chances that the first received client request
                // triggers instantiation and encounters undesired instantiation latency.
                // In their constructors, those services consequentially inject
                // other required services, triggering instantiation. Such recursive nature of DI and
                // service instantiation results in the activation of all required services.
                GraphQLSchemaCreator graphQLSchemaCreator =
                    scopeServiceProvider.GetRequiredService<GraphQLSchemaCreator>();

                RestService restService =
                    scopeServiceProvider.GetRequiredService<RestService>();

                if (graphQLSchemaCreator is null || restService is null)
                {
                    //_logger.LogError("Endpoint service initialization failed.");
                }

                if (runtimeConfig.IsDevelopmentMode())
                {
                    // Running only in developer mode to ensure fast and smooth startup in production.
                    runtimeConfigValidator.ValidateRelationshipConfigCorrectness(runtimeConfig);
                    runtimeConfigValidator.ValidateRelationships(runtimeConfig, sqlMetadataProviderFactory!);
                }

                // OpenAPI document creation is only attempted for REST supporting database types.
                // CosmosDB is not supported for OpenAPI document creation.
                if (!runtimeConfig.CosmosDataSourceUsed)
                {
                    // Attempt to create OpenAPI document.
                    // Errors must not crash nor halt the intialization of the engine
                    // because OpenAPI document creation is not required for the engine to operate.
                    // Errors will be logged.
                    try
                    {
                        //var loaded = runtimeConfigProvider.SetConfig(runtimeConfig);
                        IOpenApiDocumentor openApiDocumentor = scopeServiceProvider.GetRequiredService<IOpenApiDocumentor>();
                        openApiDocumentor.CreateDocumentNewConfig(runtimeConfigProvider, runtimeConfig);
                    }
                    catch (DataApiBuilderException)
                    {
                        //_logger.LogWarning(exception: dabException, message: "OpenAPI Documentor initialization failed. This will not affect dab startup.");
                    }
                }

                //_logger.LogInformation("Successfully completed runtime initialization.");
                return true;
            }
            catch (Exception)
            {
                //_logger.LogError(exception: ex, message: "Unable to complete runtime initialization. Refer to exception for error details.");
                return false;
            }
        }


    }
    /// <summary>
    /// The required parameters required to configure the runtime.
    /// </summary>
    /// <param name="Configuration">The runtime configuration.</param>
    /// <param name="Schema">The GraphQL schema. Can be left empty for SQL databases.</param>
    /// <param name="ConnectionString">The database connection string.</param>
    /// <param name="AccessToken">The managed identity access token (if any) used to connect to the database.</param>
    /// <param name="Database"> The name of the database to be used for Cosmos</param>
    public record class ConfigurationPostParameters(
        string Configuration,
        string? Schema,
        string ConnectionString,
        string? AccessToken)
    { }

    /// <summary>
    /// The required parameters required to configure the runtime.
    /// </summary>
    /// <param name="Configuration">The runtime configuration.</param>
    /// <param name="ConfigurationOverrides">Configuration parameters that override the options from the Configuration file.</param>
    /// <param name="Schema">The GraphQL schema. Can be left empty for SQL databases.</param>
    /// <param name="AccessToken">The managed identity access token (if any) used to connect to the database.</param>
    public record class ConfigurationPostParametersV2(
        string Configuration,
        string ConfigurationOverrides,
        string? Schema,
        string? AccessToken)
    { }
}
