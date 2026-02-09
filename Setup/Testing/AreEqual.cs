using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Setup.Testing;

internal static class AreEqual
{
    public static bool Lists<T>(List<T> left, List<T> right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left == null || right == null || left.Count != right.Count)
        {
            return false;
        }

        for (int i = 0; i < left.Count; i++)
        {
            if (!Equals(left[i], right[i]))
            {
                return false;
            }
        }

        return true;
    }
}
