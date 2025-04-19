// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace ConfigInterface
{
    /// <summary>
    /// Common options for all the commands
    /// </summary>
    public class Options
    {
        public Options(string? config)
        {
            Config = config;
        }

        public string? Config { get; }
    }
}
