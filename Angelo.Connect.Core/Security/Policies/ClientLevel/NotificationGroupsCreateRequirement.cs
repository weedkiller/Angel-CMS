﻿using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Angelo.Connect.Configuration;
using Angelo.Connect.Security.KnownClaims;
using Angelo.Connect.Abstractions;


namespace Angelo.Connect.Security.Policies.ClientLevel
{
    public class NotificationGroupsCreateRequirement : IAuthorizationRequirement
    {
    }

    public class NotificationGroupsCreateHandler : AbstractClientLevelOrAboveClaimHandler<NotificationGroupsCreateRequirement>
    {
        public override IEnumerable<string> ValidClaimTypes { get; set; }

        public NotificationGroupsCreateHandler(IContextAccessor<AdminContext> adminContextAccessor) : base(adminContextAccessor)
        {
            ValidClaimTypes = new string[]
            {
                ClientClaimTypes.AppNotifyGroupsCreate,
                ClientClaimTypes.PrimaryAdmin
            };
        }

    }
}
