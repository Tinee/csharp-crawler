using System;
using System.Collections.Concurrent;


namespace Crawler
{
    static class Filters
    {
        // Seems to be the way C# is handling closures.
        // I like it.
        public static Func<Uri, bool> Unique()
        {
            var been = new ConcurrentDictionary<Uri, bool>();
            return (Uri u) => been.TryAdd(u, true);
        }

        public static Func<Uri, bool> SchemaContains(string compare) => (Uri u) => u.Scheme.Contains(compare);

        public static Func<Uri, bool> isSameDomain(Uri uri) => (Uri u) => uri.Host == u.Host;
    }
}
