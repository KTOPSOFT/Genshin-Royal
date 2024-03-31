using UnityEngine;
using UnityEditor;
using Lovatto.Localization;
using UnityEditorInternal;
using MFPSEditor;
using Photon.Pun;
using TMPro;

[CustomEditor(typeof(bl_LanguageTexts))]
public class bl_LanguageTextsEditor : Editor
{
    private bl_LanguageTexts script;
    private SerializedProperty listProp;
    private ReorderableList list;
    private bool openEdit = false;
    private void OnEnable()
    {
        script = (bl_LanguageTexts)target;
        listProp = serializedObject.FindProperty("Data");
        list = new ReorderableList(serializedObject, listProp, false, true, false, false);
        list.drawElementCallback += DrawElement();
        list.drawHeaderCallback += DrawCustomHeader();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        script.LanguageName = EditorGUILayout.TextField("Language Name", script.LanguageName);
        script.PlurarLetter = EditorGUILayout.TextField("Plural Letter", script.PlurarLetter);
        script.LanguageIcon = EditorGUILayout.ObjectField("Icon", script.LanguageIcon, typeof(Sprite), false) as Sprite;
        script.CustomFont = EditorGUILayout.ObjectField("Custom Font", script.CustomFont, typeof(TMP_FontAsset), false) as TMP_FontAsset;

        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("Edit in Localization Window", EditorStyles.toolbarButton))
        {
            EditorWindow.GetWindow<bl_LocalizationWindow>();
        }
        GUILayout.Space(5);
        if (openEdit)
        {
            if (GUILayout.Button("Close Edit", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                openEdit = false;
            }
        }
        else
        {
            if (GUILayout.Button("Edit Here", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                openEdit = true;
            }
        }
        GUILayout.EndHorizontal();
        GUI.enabled = openEdit;
        var property = list.serializedProperty;
        list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
        GUI.enabled = true;
        if (GUILayout.Button("Apply Changes", EditorStyles.toolbarButton))
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(script);
            AssetDatabase.SaveAssets();
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(script);
        }
    }

   private ReorderableList.ElementCallbackDelegate DrawElement()
    {
        return (rect, index, isActive, isFocused) =>
        {
            var property = list.serializedProperty.GetArrayElementAtIndex(index);
            var textPro = property.FindPropertyRelative("Text");
            rect.height = EditorGUIUtility.singleLineHeight;
            Rect r = rect;
            r.width = 35;
            GUI.Label(r, index.ToString());
            r = rect;
            r.x += 40;
            r.width = 85;
            var sid = property.FindPropertyRelative("StringID");
            EditorGUI.LabelField(r, sid.stringValue);
            r = rect;
            r.x = rect.x + 120;
            r.width -= 120;
            EditorGUI.PropertyField(r, textPro, GUIContent.none);
        };
    }

    private ReorderableList.HeaderCallbackDelegate DrawCustomHeader()
    {
        return (rect) =>
        {
            Rect r = rect;
            r.width = 20;
            GUI.Label(r, "ID");
            r.x += 30;
            r.width = 85;
            GUI.Label(r, "String ID");
            r = rect;
            r.x += 115;
            r.width -= 105;
            GUI.Label(r, "Text");
        };
    }
}