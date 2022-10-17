using System;
using System.Collections.Generic;
using System.Linq;

namespace ChargeAmpReminder;

public static class IEnumerableExtensions
{
    public static bool None<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
    {
        return !enumerable.Any(predicate);
    }
}