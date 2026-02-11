using System;
using System.Collections.Generic;
using System.Web;

namespace NzbDrone.Plugin.SquidWTF
{
    internal static class Helpers
    {
        public static string BuildUrl(string baseUrl, string path, Dictionary<string, string> queryParams)
        {
            var uri = new Uri(new Uri(baseUrl.TrimEnd('/') + "/"), path.TrimStart('/'));

            var builder = new UriBuilder(uri);

            var query = HttpUtility.ParseQueryString(string.Empty);

            foreach (var kvp in queryParams)
            {
                query[kvp.Key] = queryParams[kvp.Key];
            }

            builder.Query = query.ToString();

            return builder.ToString();
        }
    }
}
