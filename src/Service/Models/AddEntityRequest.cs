// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel.DataAnnotations;

namespace Azure.DataApiBuilder.Service.Models
{
    public class AddEntityRequest
    {
        [Required(ErrorMessage = "EntityName is required")]
        public string? SubDomain { get; set; }
        public string? TenantWorkSpaceId { get; set; }
        public string? EntityName { get; set; }
        public bool EnableGraphQL { get; set; } = true;
        public bool EnableRest { get; set; } = true;
        public bool AllowAnonymous { get; set; } = true;
    }
}
