using System.Collections.Generic;
using Microsoft.Identity.Client;

namespace BookingVCSkypeBot.Authentication.AADv2
{
    public class AuthTokenCache
    {
        private const string CacheId = "TokenCache";

        private readonly Dictionary<string, object> cacheData = new Dictionary<string, object>();
        private readonly TokenCache cache = new TokenCache();

        public AuthTokenCache()
        {
            cache.SetBeforeAccess(BeforeAccessNotification);
            cache.SetAfterAccess(AfterAccessNotification);
            Load();
        }

        public AuthTokenCache(byte[] tokenCache)
        {
            cache.SetBeforeAccess(BeforeAccessNotification);
            cache.SetAfterAccess(AfterAccessNotification);
            cache.Deserialize(tokenCache);
        }

        public TokenCache GetCacheInstance()
        {
            cache.SetBeforeAccess(BeforeAccessNotification);
            cache.SetAfterAccess(AfterAccessNotification);
            Load();
            return cache;
        }

        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (cache.HasStateChanged)
            {
                Persist();
            }
        }

        private void Load()
        {
            if (cacheData.ContainsKey(CacheId))
            {
                cache.Deserialize((byte[])cacheData[CacheId]);
            }
        }

        private void Persist()
        {
            cache.HasStateChanged = false;

            cacheData[CacheId] = cache.Serialize();
        }
    }
}