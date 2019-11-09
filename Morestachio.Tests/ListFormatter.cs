using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using Morestachio.Attributes;
using Morestachio.Formatter.Framework;

namespace Morestachio.Tests
{
    public static class ListFormatter
    {
        [MorestachioFormatter("Select", "Selects a Property from each item in the list and creates a new list", ReturnHint = "List contains the property. Can be listed with #each")]
        public static IEnumerable Select<T>(IEnumerable<T> sourceCollection, string arguments, [RestParameterAttribute]object[] args)
        {
            return sourceCollection.AsQueryable().Select(arguments, args);
        }
    }
}