// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Abstractions;
using System.Net;
using Azure.DataApiBuilder.Config.DatabasePrimitives;
using Azure.DataApiBuilder.Config.ObjectModel;
using Azure.DataApiBuilder.Core.Configurations;
using Azure.DataApiBuilder.Core.Resolvers.Factories;
using Azure.DataApiBuilder.Service.Exceptions;
using Microsoft.Extensions.Logging;

namespace Azure.DataApiBuilder.Core.Services.MetadataProviders
{
    /// <inheritdoc />
    public class MetadataProviderFactory : IMetadataProviderFactory
    {
        private readonly IDictionary<string, ISqlMetadataProvider> _metadataProviders;
        private RuntimeConfigProvider _runtimeConfigProvider;
        private readonly IAbstractQueryManagerFactory _queryManagerFactory;
        private readonly ILogger<ISqlMetadataProvider> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly bool _isValidateOnly;

        public MetadataProviderFactory(
           RuntimeConfigProvider runtimeConfigProvider,
            IAbstractQueryManagerFactory queryManagerFactory,
            ILogger<ISqlMetadataProvider> logger,
            IFileSystem fileSystem,
            bool isValidateOnly = false)
        {

            _runtimeConfigProvider = runtimeConfigProvider;
            _queryManagerFactory = queryManagerFactory;
            _logger = logger;
            _fileSystem = fileSystem;
            _isValidateOnly = isValidateOnly;

            _metadataProviders = new Dictionary<string, ISqlMetadataProvider>();
            foreach ((string dataSourceName, DataSource dataSource) in runtimeConfigProvider.GetConfig().GetDataSourceNamesToDataSourcesIterator())
            {
                ISqlMetadataProvider metadataProvider = dataSource.DatabaseType switch
                {
                    DatabaseType.CosmosDB_NoSQL => new CosmosSqlMetadataProvider(_runtimeConfigProvider, fileSystem),
                    DatabaseType.MSSQL => new MsSqlMetadataProvider(_runtimeConfigProvider, _queryManagerFactory, logger, dataSourceName, isValidateOnly),
                    DatabaseType.DWSQL => new MsSqlMetadataProvider(_runtimeConfigProvider, _queryManagerFactory, logger, dataSourceName, isValidateOnly),
                    DatabaseType.PostgreSQL => new PostgreSqlMetadataProvider(_runtimeConfigProvider, _queryManagerFactory, logger, dataSourceName, isValidateOnly),
                    DatabaseType.MySQL => new MySqlMetadataProvider(_runtimeConfigProvider, _queryManagerFactory, logger, dataSourceName, isValidateOnly),
                    _ => throw new NotSupportedException(dataSource.DatabaseTypeNotSupportedMessage),
                };

                _metadataProviders.Add(dataSourceName, metadataProvider);
            }
        }

        // <summary>
        /// Updates the RuntimeConfigProvider and reinitializes metadata providers based on the new configuration.
        /// </summary>
        /// <param name="runtimeConfigProvider">The new RuntimeConfigProvider to set</param>
        /// <exception cref="ArgumentNullException">Thrown when runtimeConfigProvider is null</exception>
        /// <exception cref="DataApiBuilderException">Thrown when initialization with new config fails</exception>
        public async Task SetDynamicRuntimeConfigProvider(RuntimeConfigProvider runtimeConfigProvider, RuntimeConfig runtimeConfig)
        {
            if (runtimeConfigProvider == null)
            {
                throw new ArgumentNullException(nameof(runtimeConfigProvider), "RuntimeConfigProvider cannot be null");
            }

            try
            {
                // Clear existing metadata providers
                _metadataProviders.Clear();
                _runtimeConfigProvider = runtimeConfigProvider;

                // Rebuild metadata providers with new runtime config
                foreach ((string dataSourceName, DataSource dataSource) in runtimeConfig.GetDataSourceNamesToDataSourcesIterator())
                {
                    ISqlMetadataProvider metadataProvider = dataSource.DatabaseType switch
                    {
                        DatabaseType.CosmosDB_NoSQL => new CosmosSqlMetadataProvider(runtimeConfigProvider, _fileSystem),
                        DatabaseType.MSSQL => new MsSqlMetadataProvider(runtimeConfigProvider, _queryManagerFactory, _logger, dataSourceName, _isValidateOnly),
                        DatabaseType.DWSQL => new MsSqlMetadataProvider(runtimeConfigProvider, _queryManagerFactory, _logger, dataSourceName, _isValidateOnly),
                        DatabaseType.PostgreSQL => new PostgreSqlMetadataProvider(runtimeConfigProvider, _queryManagerFactory, _logger, dataSourceName, _isValidateOnly),
                        DatabaseType.MySQL => new MySqlMetadataProvider(runtimeConfigProvider, _queryManagerFactory, _logger, dataSourceName, _isValidateOnly),
                        _ => throw new NotSupportedException(dataSource.DatabaseTypeNotSupportedMessage),
                    };

                    _metadataProviders.Add(dataSourceName, metadataProvider);
                }
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                throw new DataApiBuilderException(
                    message: "Failed to set new RuntimeConfigProvider",
                    statusCode: HttpStatusCode.InternalServerError,
                    subStatusCode: DataApiBuilderException.SubStatusCodes.ConfigValidationError,
                    innerException: ex);
            }
        }

        /// <inheritdoc />
        public ISqlMetadataProvider GetMetadataProvider(string dataSourceName)
        {
            if (!(_metadataProviders.TryGetValue(dataSourceName, out ISqlMetadataProvider? metadataProvider)))
            {
                throw new DataApiBuilderException(
                    $"{nameof(dataSourceName)}:{dataSourceName} could not be found within the config",
                    HttpStatusCode.BadRequest,
                    DataApiBuilderException.SubStatusCodes.DataSourceNotFound);
            }

            return metadataProvider;
        }

        /// <inheritdoc />
        public async Task InitializeAsync()
        {
            foreach ((_, ISqlMetadataProvider provider) in _metadataProviders)
            {
                if (provider is not null)
                {
                    await provider.InitializeAsync();
                }
            }
        }

        /// <summary>
        /// Captures all the metadata exceptions from all the metadata providers at a single place.
        /// </summary>
        /// <returns>List of Exceptions</returns>
        public List<Exception> GetAllMetadataExceptions()
        {
            List<Exception> allMetadataExceptions = new();
            foreach ((_, ISqlMetadataProvider provider) in _metadataProviders)
            {
                if (provider is not null)
                {
                    allMetadataExceptions.AddRange(provider.SqlMetadataExceptions);
                }
            }

            return allMetadataExceptions;
        }

        public IEnumerable<ISqlMetadataProvider> ListMetadataProviders()
        {
            return _metadataProviders.Values;
        }

        /// <inheritdoc />
        public void InitializeAsync(
            Dictionary<string, Dictionary<string, DatabaseObject>> EntityToDatabaseObjectMap,
            Dictionary<string, Dictionary<string, string>> graphQLStoredProcedureExposedNameToEntityNameMap)
        {
            foreach ((string dataSourceName, ISqlMetadataProvider provider) in _metadataProviders)
            {
                if (provider is not null)
                {
                    provider.InitializeAsync(EntityToDatabaseObjectMap[dataSourceName], graphQLStoredProcedureExposedNameToEntityNameMap[dataSourceName]);
                }
            }
        }
    }
}
