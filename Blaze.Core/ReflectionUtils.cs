using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Core
{
    public static class ReflectionUtils
    {
        public static bool IsStruct(this Type self)
        {
            return self.IsValueType && !self.IsPrimitive;
        }
    }
}
