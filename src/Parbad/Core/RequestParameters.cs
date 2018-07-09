using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Parbad.Core
{
    internal class RequestParameters : IRequestParameters
    {
        private readonly IDictionary<string, object> _parameters;

        public RequestParameters(HttpRequest httpRequest)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            _parameters = new Dictionary<string, object>();

            for (int paramIndex = 0; paramIndex < httpRequest.Params.Count; paramIndex++)
            {
                _parameters.Add(httpRequest.Params.GetKey(paramIndex), httpRequest.Params[paramIndex]);
            }
        }

        public object Get(string key, bool caseSensitive = false)
        {
            Func<KeyValuePair<string, object>, bool> predicate;

            if (caseSensitive)
            {
                predicate = keyValuePair => keyValuePair.Key == key;
            }
            else
            {
                predicate = keyValuePair => keyValuePair.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase);
            }

            return _parameters.SingleOrDefault(predicate).Value;
        }

        public void Set(string key, object value)
        {
            _parameters.Add(key, value);
        }
    }
}