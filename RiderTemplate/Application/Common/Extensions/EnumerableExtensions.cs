using System.Collections;

namespace Application.Common.Extensions;

public static class EnumerableExtensions
{
    public static TOut? GetFirstTypeMatchOrDefault<TOut>(this IEnumerable enumerable)
    {
        foreach (var item in enumerable)
        {
            if (item is TOut @out)
            {
                return @out;
            }
        }

        return default;
    }
}
