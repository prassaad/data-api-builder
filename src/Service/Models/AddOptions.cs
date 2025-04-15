// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.DataApiBuilder.Service.Models
{
    /// <summary>
    /// Add command options
    /// </summary>
    public class AddOptions
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
        {

        }
    } 
}
