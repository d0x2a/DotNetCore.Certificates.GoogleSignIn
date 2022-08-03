using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.Caching;

namespace DotNetCore.Certificates.GoogleSignIn;

public class CertificateManager
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MemoryCache _cache;
    private const string CacheKey = "DotNetCore.Certificates.GoogleSignIn__Values";

    public CertificateManager(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _cache = MemoryCache.Default;
    }

    public async Task<ImmutableDictionary<string, X509Certificate2>> GetCertificates()
    {
        var tryGetValue = _cache.Get(CacheKey);
        if (tryGetValue != null)
        {
            return (ImmutableDictionary<string, X509Certificate2>)tryGetValue;
        }

        var httpClient = _httpClientFactory.CreateClient();
        var message = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v1/certs");
        var expiration = message.Headers.CacheControl?.MaxAge?.TotalMilliseconds ?? 0d;
        var keys = await message.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        var keysTyped = (keys ?? throw new InvalidOperationException()).Select(e =>
                new KeyValuePair<string, X509Certificate2>(e.Key, X509Certificate2.CreateFromPem(e.Value)))
            .ToImmutableDictionary();
        _cache.Set(CacheKey, keysTyped, DateTimeOffset.UtcNow.AddMilliseconds(expiration));
        return keysTyped;
    }
}