using System;
using System.Web.Script.Serialization;

namespace Parbad.Utilities
{
    internal static class JsonSerializer
    {
        public static string Serialize(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var serializer = new JavaScriptSerializer();

            return serializer.Serialize(obj);
        }
    }
}