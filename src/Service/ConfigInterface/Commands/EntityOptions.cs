// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace ConfigInterface.Commands
{
    /// <summary>
    /// Command options for entity manipulation.
    /// </summary>
    public class EntityOptions : Options
    {
        public EntityOptions(
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
            : base(config)
        {
            Entity = entity;
            SourceType = sourceType;
            SourceParameters = sourceParameters;
            SourceKeyFields = sourceKeyFields;
            RestRoute = restRoute;
            RestMethodsForStoredProcedure = restMethodsForStoredProcedure;
            GraphQLType = graphQLType;
            GraphQLOperationForStoredProcedure = graphQLOperationForStoredProcedure;
            FieldsToInclude = fieldsToInclude;
            FieldsToExclude = fieldsToExclude;
            PolicyRequest = policyRequest;
            PolicyDatabase = policyDatabase;
        }

        // Entity is required but we have made required as false to have custom error message (more user friendly), if not provided.
        public string Entity { get; }
        public string? SourceType { get; }
        public IEnumerable<string>? SourceParameters { get; }
        public IEnumerable<string>? SourceKeyFields { get; }
        public string? RestRoute { get; }
        public IEnumerable<string>? RestMethodsForStoredProcedure { get; }
        public string? GraphQLType { get; }
        public string? GraphQLOperationForStoredProcedure { get; }
        public IEnumerable<string>? FieldsToInclude { get; }
        public IEnumerable<string>? FieldsToExclude { get; }

        public string? PolicyRequest { get; }
        public string? PolicyDatabase { get; }
    }
}
