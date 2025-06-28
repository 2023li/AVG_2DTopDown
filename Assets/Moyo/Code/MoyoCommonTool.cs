using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moyo.Unity

{
    public static class MoyoCommonTool
    {
        public static T GetRandomEnumValue<T>() where T : struct, Enum
        {
            Array values = Enum.GetValues(typeof(T));
            Random random = new Random();
            return (T)values.GetValue(random.Next(values.Length));
        }

    }
}
