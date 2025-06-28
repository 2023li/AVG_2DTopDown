using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MoreMountains.Tools
{
    // 修改类名和属性标记
    public class MMLabelAttribute : PropertyAttribute
    {
        public readonly string Label;
        public MMLabelAttribute(string label) => Label = label;
    }
}
