using System.Collections.Generic;
using Microsoft.Identity.Client;

namespace BookingVCSkypeBot.Authentication.AADv2
{
    public class InMemoryTokenCacheMSAL
    {
        private readonly string cacheId;
        private readonly Dictionary<string, object> cacheData = new Dictionary<string, object>();
        private readonly TokenCache cache = new TokenCache();

        public InMemoryTokenCacheMSAL()
        {
            cacheId = "MSAL_TokenCache";
            cache.SetBeforeAccess(BeforeAccessNotification);
            cache.SetAfterAccess(AfterAccessNotification);
            Load();
        }

        public InMemoryTokenCacheMSAL(byte[] tokenCache)
        {
            cacheId = "MSAL_TokenCache";
            cache.SetBeforeAccess(BeforeAccessNotification);
            cache.SetAfterAccess(AfterAccessNotification);
            cache.Deserialize(tokenCache);
        }

        public TokenCache GetMsalCacheInstance()
        {
            cache.SetBeforeAccess(BeforeAccessNotification);
            cache.SetAfterAccess(AfterAccessNotification);
            Load();
            return cache;
        }

        public void Load()
        {
            if (cacheData.ContainsKey(cacheId))
            {
                cache.Deserialize((byte[])cacheData[cacheId]);
            }
        }

        public void Persist()
        {
            // Optimistically set HasStateChanged to false. We need to do it early to avoid losing changes made by a concurrent thread.
            cache.HasStateChanged = false;

            // Reflect changes in the persistent store
            cacheData[cacheId] = cache.Serialize();
        }

        // Triggered right before ADAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after ADAL accessed the cache.
        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (cache.HasStateChanged)
            {
                Persist();
            }
        }
    }
}