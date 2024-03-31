#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

[System.AttributeUsage(System.AttributeTargets.Field)]
public class LocalizationTextAttribute : PropertyAttribute
{
    public LocalizationTextAttribute()
    {

    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(LocalizationTextAttribute))]
public class LocalizationTextAttributeDrawer : PropertyDrawer
{

    private int id = 0;

    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        string[] array = bl_Localization.Instance.GetIdsList().ToArray();
        if (array.Length > 0)
        {
            id = EditorGUI.Popup(position, prop.name, id, array);
            prop.stringValue = array[id];
        }
    }
}
#endif