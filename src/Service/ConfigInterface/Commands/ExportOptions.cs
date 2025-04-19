// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace ConfigInterface.Commands
{
    public class ExportOptions : Options
    {
        public ExportOptions(bool graphql, string outputDirectory, string? config, string? graphqlSchemaFile) : base(config)
        {
            GraphQL = graphql;
            OutputDirectory = outputDirectory;
            GraphQLSchemaFile = graphqlSchemaFile ?? "schema.graphql";
        }

        public bool GraphQL { get; }
        public string OutputDirectory { get; }
        public string GraphQLSchemaFile { get; }
    }
}
