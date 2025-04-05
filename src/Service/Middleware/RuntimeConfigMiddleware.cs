// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Abstractions;
using Azure.DataApiBuilder.Config;
using Azure.DataApiBuilder.Config.ObjectModel;
using Azure.DataApiBuilder.Core.Configurations;
using Azure.DataApiBuilder.Core.Services;
using Azure.DataApiBuilder.Core.Services.MetadataProviders;
using Azure.DataApiBuilder.Service.Exceptions;

namespace Azure.DataApiBuilder.Service.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class RuntimeConfigMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public RuntimeConfigMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }
        public Task Invoke(HttpContext context, IServiceProvider serviceProvider)
        {
            // Resolve services dynamically within the request scope

            string configFileName = _configuration.GetValue<string>("ConfigFileName")
                ?? FileSystemRuntimeConfigLoader.DEFAULT_CONFIG_FILE_NAME;

            string? connectionString = _configuration.GetValue<string?>(
                FileSystemRuntimeConfigLoader.RUNTIME_ENV_CONNECTION_STRING
                    .Replace(FileSystemRuntimeConfigLoader.ENVIRONMENT_PREFIX, ""),
                null);

            // Create service instances
            IFileSystem fileSystem = new FileSystem();
            FileSystemRuntimeConfigLoader configLoader = new(fileSystem, configFileName, connectionString);
            RuntimeConfigProvider configProvider = new(configLoader);

            // Configure Application Insights conditionally
            //configProvider.TryGetConfig(out RuntimeConfig? runtimeConfig);
            //configLoader.TryLoadKnownConfig(out RuntimeConfig? config, replaceEnvVar: true, runtimeConfig!.DefaultDataSourceName);

            //configProvider.HotReloadConfig();

            //bool isRuntimeReady = false;

            //// Add to request services if needed
            var scope = serviceProvider.CreateScope();
            var scopeServiceProvider = scope.ServiceProvider;
            var isRuntimeReady = PerformOnConfigChangeAsync(scopeServiceProvider, configProvider);

            //DataSource dataSource = new(DatabaseType.PostgreSQL,
            //    connectionString!,
            //    Options: null);



            //RestService restService =
            //    scopeServiceProvider.GetRequiredService<RestService>();

            //IOpenApiDocumentor openApiDocumentor = scopeServiceProvider.GetRequiredService<IOpenApiDocumentor>();
            //openApiDocumentor.CreateDocument();

            return _next(context);
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
                }

                IMetadataProviderFactory sqlMetadataProviderFactory =
                    scopeServiceProvider.GetRequiredService<IMetadataProviderFactory>();

                if (sqlMetadataProviderFactory is not null)
                {
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
                        openApiDocumentor.CreateDocumentNewConfig(runtimeConfig);
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
    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class RuntimeConfigMiddlewareExtensions
    {
        public static IApplicationBuilder UseRuntimeConfigMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RuntimeConfigMiddleware>();
        }
    }
}
