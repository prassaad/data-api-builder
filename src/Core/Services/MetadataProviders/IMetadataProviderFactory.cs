// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.DataApiBuilder.Config.DatabasePrimitives;
using Azure.DataApiBuilder.Config.ObjectModel;
using Azure.DataApiBuilder.Core.Configurations;

namespace Azure.DataApiBuilder.Core.Services.MetadataProviders
{
    /// <summary>
    /// IMetadataProviderFactory class.
    /// Used to get the appropriate metadata provider based on the data source name.
    /// </summary>
    public interface IMetadataProviderFactory
    {
        // <summary>
        /// Updates the RuntimeConfigProvider and reinitializes metadata providers based on the new configuration.
        /// </summary>
        /// <param name="runtimeConfigProvider">The new RuntimeConfigProvider to set</param>
        /// <exception cref="ArgumentNullException">Thrown when runtimeConfigProvider is null</exception>
        /// <exception cref="DataApiBuilderException">Thrown when initialization with new config fails</exception>
        public Task SetDynamicRuntimeConfigProvider(RuntimeConfigProvider runtimeConfigProvider, RuntimeConfig runtimeConfig);

        /// <summary>
        /// Gets the appropriate metadata provider based on the data source name.
        /// </summary>
        public ISqlMetadataProvider GetMetadataProvider(string dataSourceName);

        /// <summary>
        /// Lists the metadata providers.
        /// </summary>
        public IEnumerable<ISqlMetadataProvider> ListMetadataProviders();

        public List<Exception> GetAllMetadataExceptions();

        /// <summary>
        /// Initializes the metadata providers.
        /// </summary>
        public Task InitializeAsync();

        /// <summary>
        /// Initializes the metadata providers with parameters
        /// Note : this is used in GraphQL workload to call the parameterized initialize async method in providers
        /// </summary>
        public void InitializeAsync(Dictionary<string, Dictionary<string, DatabaseObject>> EntityToDatabaseObjectMap,
            Dictionary<string, Dictionary<string, string>> graphQLStoredProcedureExposedNameToEntityNameMap);
    }
}
