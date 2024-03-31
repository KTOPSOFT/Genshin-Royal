using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class ScopeProShaderEditor : ShaderGUI
{

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material targetMat = materialEditor.target as Material;
        EditorGUIUtility.labelWidth = 0f;
        // see if redify is set, and show a checkbox
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginVertical("box");
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var matProp = ShaderGUI.FindProperty("_ReticleTexure", properties);
                materialEditor.TexturePropertySingleLine(new GUIContent("Reticle"), matProp);
                var rColor = ShaderGUI.FindProperty("_ReticleTint", properties);
                rColor.colorValue = EditorGUILayout.ColorField(rColor.colorValue);
            }
            var rDepth = ShaderGUI.FindProperty("_ReticleDepth", properties);
            rDepth.floatValue = EditorGUILayout.Slider("Reticle Size", rDepth.floatValue, 0.2f, 10);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        {
            var matProp = ShaderGUI.FindProperty("_RenderTexture", properties);
            materialEditor.TextureProperty(matProp, "Render Texture");
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        {
            var vrProp = ShaderGUI.FindProperty("_VignetteRadius", properties);
            vrProp.floatValue = EditorGUILayout.Slider("Vignette Smooth", vrProp.floatValue, 0, 2);
            var vsProp = ShaderGUI.FindProperty("_VignetteSmoothness", properties);
            vsProp.floatValue = EditorGUILayout.Slider("Vignette Radius", vsProp.floatValue, 0, 2);
        }
        EditorGUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
        {
        }
    }
}