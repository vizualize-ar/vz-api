using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace VZ.Shared
{
    public static class IEnumerableExtensions
    {
        public static IList<T> EmptyIfNull<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return new Collection<T>();
            }
            return enumerable.AsList();
        }
    }
}
