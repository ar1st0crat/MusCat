using System;
using System.Diagnostics;

namespace MusCat.Core.Util
{
    [DebuggerStepThrough]
    public static class Guard
    {
        public static void AgainstNull(object argument)
        {
            if (argument == null)
            {
                throw new ArgumentNullException($"{nameof(argument)} is null");
            }
        }
    }
}
