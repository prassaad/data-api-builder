// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Azure.DataApiBuilder.Config;
using Azure.DataApiBuilder.Config.ObjectModel;
using Azure.DataApiBuilder.Core.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            // Add to request services if needed
            var scope = serviceProvider.CreateScope();
            var scopeServiceProvider = scope.ServiceProvider;

            var config = configProvider.TryGetConfig(out RuntimeConfig? runtimeConfig);

            return _next(context);
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
