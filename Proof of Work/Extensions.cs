using System;
using System.Collections;
using System.Text;

namespace Proof_of_Work
{
    public static class Extensions
    {
        public static string ToBitString(this BitArray arr)
        {
            var builder = new StringBuilder();

            foreach (var bit in arr)
            {
                builder.Append((bool)bit ? 1 : 0);
            }

            return builder.ToString();
        }
    }
}
