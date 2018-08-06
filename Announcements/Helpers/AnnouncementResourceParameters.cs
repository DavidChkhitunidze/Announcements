using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Announcements.Helpers
{
    public class AnnouncementResourceParameters
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 4;

        public string SearchString { get; set; }

        public string Fields { get; set; }

        public string GetQueryString(string url)
        {
            var propertiesDictionary = GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                .Where(p => p.GetValue(this) != null)
                .ToDictionary(p => p.Name.ToLowerInvariant(), p => p.GetValue(this).ToString());

            var queryString = QueryHelpers.AddQueryString(url, propertiesDictionary);
            return queryString;
        }
    }
}
