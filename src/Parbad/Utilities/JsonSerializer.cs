using System;
using System.Web.Script.Serialization;

namespace Parbad.Utilities
{
    internal static class JsonSerializer
    {
        public static string Serialize(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return new JavaScriptSerializer().Serialize(obj);
        }

        public static T Deserialize<T>(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));

            return new JavaScriptSerializer().Deserialize<T>(json);
        }
    }
}