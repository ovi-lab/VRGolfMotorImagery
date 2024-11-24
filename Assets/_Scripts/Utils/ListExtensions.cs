using System;
using System.Collections.Generic;

public static class ListExtensions {
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts, Random random) {
        int count = ts.Count;
        int last = count - 1;
        for (int i = 0; i < last; ++i) {
            int r = random.Next(i, count);
            (ts[i], ts[r]) = (ts[r], ts[i]);
        }
    }
}