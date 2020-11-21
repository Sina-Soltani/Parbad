using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Parbad.Tests.Helpers
{
    public class TestableQueryCollection : Dictionary<string, StringValues>, IQueryCollection
    {
        public TestableQueryCollection(IDictionary<string, StringValues> queries) : base(queries)
        {
        }

        public new ICollection<string> Keys => base.Keys;
    }
}
