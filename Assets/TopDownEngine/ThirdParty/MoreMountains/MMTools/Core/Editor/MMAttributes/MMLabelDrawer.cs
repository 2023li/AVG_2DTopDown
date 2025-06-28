using UnityEditor;
using UnityEngine;

namespace MoreMountains.Tools
{


    // 自定义 PropertyDrawer
    [CustomPropertyDrawer(typeof(MMLabelAttribute))]
    public class MMLabelDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var labelAttr = (MMLabelAttribute)attribute;
            label.text = labelAttr.Label;
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
