using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Assets.Scripts.Data;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.EditorExt
{
    [CustomPropertyDrawer(typeof(ClimateState))]
    class ClimateStateDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            //int buttonsIntValue = 0;
            //bool[] buttonPressed = new bool[enumLength];
            var totalWidth = position.width;

            //var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            //EditorGUI.LabelField(labelRect, label);
            //EditorGUI.TextField(labelRect, label.text);

            var climate = (ClimateState)GetTargetObjectOfProperty(property);
            var rectTemp = new Rect(position.x, position.y, 0.75f * 0.7f * totalWidth, position.height);
            var rectTempInfo = new Rect(rectTemp.xMax, position.y, 0.75f * 0.3f * totalWidth, position.height);
            var rectHumidity = new Rect(rectTempInfo.xMax + 5, position.y, 0.25f * 0.75f * totalWidth - 5, position.height);
            var rectHumidityInfo = new Rect(rectHumidity.xMax, position.y, 0.25f * 0.25f * totalWidth, position.height);

            //EditorGUI.TextField(rectTemp, climate.Temperature.ToString(CultureInfo.InvariantCulture));
            EditorGUI.PropertyField(rectTemp, property.FindPropertyRelative("Temperature"), GUIContent.none);

            if(climate != null)
                EditorGUI.LabelField(rectTempInfo, string.Format("°F {1:##0.#}°C", climate.Temperature, climate.TemperatureAsCelsius()));
            else
                EditorGUI.LabelField(rectTempInfo, "°F");
            
            //EditorGUI.TextField(rectHumidity, climate.Humidity.ToString(CultureInfo.InvariantCulture));
            EditorGUI.PropertyField(rectHumidity, property.FindPropertyRelative("Humidity"), GUIContent.none);
            EditorGUI.LabelField(rectHumidityInfo, "%");
            

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Gets the object the property represents.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }
    }
}
