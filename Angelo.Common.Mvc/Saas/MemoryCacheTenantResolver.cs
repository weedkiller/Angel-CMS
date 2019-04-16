﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;


namespace Angelo.Common.Mvc.Saas
{
    public abstract class MemoryCacheTenantResolver<TTenant> : ITenantResolver<TTenant>
    {
        protected readonly IMemoryCache cache;
        protected readonly ILogger log;

        public MemoryCacheTenantResolver(IMemoryCache cache, ILoggerFactory loggerFactory)
        {
            Ensure.Argument.NotNull(cache, nameof(cache));
            Ensure.Argument.NotNull(loggerFactory, nameof(loggerFactory));

            this.cache = cache;
            this.log = loggerFactory.CreateLogger<MemoryCacheTenantResolver<TTenant>>();
        }

        protected virtual MemoryCacheEntryOptions CreateCacheEntryOptions()
        {
            return new MemoryCacheEntryOptions()
                .SetSlidingExpiration(new TimeSpan(1, 0, 0))
                .RegisterPostEvictionCallback((key, value, reason, state)
                    => DisposeTenantContext(key, value as TenantContext<TTenant>));
        }

        protected virtual void DisposeTenantContext(object cacheKey, TenantContext<TTenant> tenantContext)
        {
            if (tenantContext != null)
            {
                log.LogDebug("Disposing TenantContext:{id} instance with key \"{cacheKey}\".", tenantContext.Id, cacheKey);
                tenantContext.Dispose();
            }
        }

        protected abstract string GetContextIdentifier(HttpContext context);
        protected abstract IEnumerable<string> GetTenantIdentifiers(TenantContext<TTenant> tenantContext);
        protected abstract Task<TenantContext<TTenant>> ResolveAsync(HttpContext httpContext);
        

        async Task<TenantContext<TTenant>> ITenantResolver<TTenant>.ResolveAsync(HttpContext context)
        {
            Ensure.Argument.NotNull(context, nameof(context));

            // Obtain the key used to identify cached tenants from the current request
            var cacheKey = GetContextIdentifier(context);

            if (cacheKey == null)
            {
                return null;
            }

            var tenantContext = cache.Get(cacheKey) as TenantContext<TTenant>;

            if (tenantContext == null)
            {
                log.LogDebug("TenantContext not present in cache with key \"{cacheKey}\". Attempting to resolve.", cacheKey);
                tenantContext = await ResolveAsync(context);

                if (tenantContext != null)
                {
                    var tenantIdentifiers = GetTenantIdentifiers(tenantContext);

                    if (tenantIdentifiers != null)
                    {
                        var cacheEntryOptions = CreateCacheEntryOptions();

                        log.LogDebug("TenantContext:{id} resolved. Caching with keys \"{tenantIdentifiers}\".", tenantContext.Id, tenantIdentifiers);

                        foreach (var identifier in tenantIdentifiers)
                        {
                            cache.Set(identifier, tenantContext, cacheEntryOptions);
                        }
                    }
                }
            }
            else
            {
                log.LogDebug("TenantContext:{id} retrieved from cache with key \"{cacheKey}\".", tenantContext.Id, cacheKey);
            }

            return tenantContext;
        }
    }

}