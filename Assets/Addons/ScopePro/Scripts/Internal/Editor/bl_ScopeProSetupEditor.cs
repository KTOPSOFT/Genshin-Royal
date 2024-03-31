using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MFPS.Addon.ScopePro
{
    [CustomEditor(typeof(bl_ScopePro))]
    public class bl_ScopeProSetupEditor : Editor
    {

        bl_ScopePro script;
        GUIStyle textWrap;
        bool firstSetup = false;
        public int selectedTMaterial = 0;
        public string[] templateMaterials;
        private MaterialEditor materialEditor;
        private bl_Gun m_Gun;

        /// <summary>
        /// 
        /// </summary>
        void OnEnable()
        {
            script = (bl_ScopePro)target;

            var ps = script.transform.root.GetComponent<bl_PlayerReferences>();
            if (script.PlayerCamera == null && ps != null)
            {
                script.PlayerCamera = ps.playerCamera;
                EditorUtility.SetDirty(target);
            }
            templateMaterials = new string[bl_ScopeProSettings.Instance.templateScopeMaterials.Length];
            for (int i = 0; i < bl_ScopeProSettings.Instance.templateScopeMaterials.Length; i++)
            {
                if (bl_ScopeProSettings.Instance.templateScopeMaterials[i] == null) continue;
                templateMaterials[i] = bl_ScopeProSettings.Instance.templateScopeMaterials[i].name;
            }

            if (script.ScopeRTMaterial != null)
            {
                materialEditor = (MaterialEditor)CreateEditor(script.ScopeRTMaterial);
            }

            m_Gun = script.transform.GetComponentInParent<bl_Gun>();
            if (m_Gun != null)
            {
                if (m_Gun.gameObject.GetComponent<bl_ScopeProWeapon>() == null)
                {
                    m_Gun.gameObject.AddComponent<bl_ScopeProWeapon>();
                    EditorUtility.SetDirty(m_Gun.gameObject);
                }
            }
        }

        void OnDisable()
        {
            if (materialEditor != null)
            {
                DestroyImmediate(materialEditor);
            }
        }

        public override void OnInspectorGUI()
        {
            if (!firstSetup)
            {
                textWrap = new GUIStyle(EditorStyles.helpBox);
                textWrap.wordWrap = true;
                textWrap.richText = true;
                textWrap.alignment = TextAnchor.MiddleLeft;
                if (EditorGUIUtility.isProSkin)
                {
                    textWrap.normal.textColor = Color.white;
                }
                firstSetup = true;
            }

            EditorGUI.BeginChangeCheck();

            if (script.NormalScopeMesh == null)
            {
                DrawGuide(1);
            }
            else if (script.RTScopeMesh == null)
            {
                DrawGuide(2);
            }
            else
            {
                DrawInspector();
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (materialEditor != null)
                {
                    materialEditor.PropertiesChanged();
                    EditorUtility.SetDirty(materialEditor.target);
                }
                if (script.NormalScopeMesh != null && script.NormalScopeMesh.gameObject.layer != LayerMask.NameToLayer("Weapons"))
                {
                    script.NormalScopeMesh.gameObject.layer = LayerMask.NameToLayer("Weapons");
                }
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                script.ApplyProps();
            }
        }

        void DrawGuide(int step)
        {
            EditorGUILayout.BeginVertical("box");
            if (step == 1)
            {
                if (script.meshDisplayMode == bl_ScopePro.MeshDisplayMode.None)
                {
                    GUILayout.Label("<b>Initial Setup:</b>\nSelect how you want to display the scope pro:\n\n<b>SwapOnAim</b> = Show the scope pro only when the player Aiming and use a simple mesh when is not <i>(Optimized)</i>\n\n<b>ShowAlways</b> = Always show the scope pro mesh, looks better even when is not necessary.\n", textWrap);
                    script.meshDisplayMode = (bl_ScopePro.MeshDisplayMode)EditorGUILayout.EnumPopup("Scope Display Mode", script.meshDisplayMode, EditorStyles.toolbarPopup);
                }
                else if (script.meshDisplayMode == bl_ScopePro.MeshDisplayMode.SwapOnAim)
                {
                    GUILayout.Label("Lets setup the scope mesh.\n\nInside of your scope/sight model <i>(where this script is attached)</i> you should have a separate mesh for the scope/sight <b>glass</b>, the default one that should come with your model, so assign that mesh bellow:\n", textWrap);
                    script.NormalScopeMesh = EditorGUILayout.ObjectField("Normal Scope Mesh", script.NormalScopeMesh, typeof(GameObject), true) as GameObject;
                }
            }
            else if (step == 2)
            {
                if (script.meshDisplayMode == bl_ScopePro.MeshDisplayMode.SwapOnAim)
                {
                    if (script.ScopeRTMaterial == null)
                    {
                        GUILayout.Label("Select the scope material that you want to use, you can create a new one and  assign it manually or you can select one of the templates from the dropdown below:\n", textWrap);
                        script.ScopeRTMaterial = EditorGUILayout.ObjectField("RT Material", script.ScopeRTMaterial, typeof(Material), true) as Material;
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            selectedTMaterial = EditorGUILayout.Popup(selectedTMaterial, templateMaterials, EditorStyles.toolbarPopup);
                            GUILayout.Space(4);
                            if (GUILayout.Button("Select", EditorStyles.toolbarButton, GUILayout.Width(100)))
                            {
                                Material tempMat = bl_ScopeProSettings.Instance.templateScopeMaterials[selectedTMaterial];
                                if (tempMat == null) return;

                                Material newMat = new Material(tempMat);
                                string path = AssetDatabase.GetAssetPath(tempMat);
                                string matName = m_Gun != null ? $"{tempMat.name} [{m_Gun.gameObject.name}]" : $"{tempMat.name} [{Random.Range(0, 9999)}]";
                                path = path.Replace(tempMat.name, matName);
                                path = AssetDatabase.GenerateUniqueAssetPath(path);
                                AssetDatabase.CreateAsset(newMat, path);
                                newMat = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
                                script.ScopeRTMaterial = newMat;
                            }
                        }
                    }
                    else
                    {
                        if (script.PlayerCamera == null)
                        {
                            GUILayout.Label("Assign the Player Camera below", textWrap);
                            script.PlayerCamera = EditorGUILayout.ObjectField("Player Camera", script.PlayerCamera, typeof(Camera), true) as Camera;
                        }
                        else
                        {
                            GUILayout.Label("Click in the button below to setup the necessary stuff for this scope\n", textWrap);
                            if (GUILayout.Button("Setup", EditorStyles.toolbarButton, GUILayout.Width(100)))
                            {
                                DoSetup();
                            }
                        }
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        void DoSetup()
        {
            if (script.meshDisplayMode == bl_ScopePro.MeshDisplayMode.SwapOnAim)
            {
                if (script.RTScopeMesh == null)
                {
                    GameObject g = Instantiate(script.NormalScopeMesh) as GameObject;
                    g.name = g.name.Replace("(Clone)", " [PRO]");
                    g.transform.parent = script.NormalScopeMesh.transform.parent;
                    g.transform.localScale = script.NormalScopeMesh.transform.localScale;
                    g.transform.localPosition = script.NormalScopeMesh.transform.localPosition;
                    g.transform.localRotation = script.NormalScopeMesh.transform.localRotation;
                    g.GetComponent<Renderer>().sharedMaterial = script.ScopeRTMaterial;
                    script.RTScopeMesh = g;
                    script.NormalScopeMesh.SetActive(false);
                }

                if (script.RenderCamera == null && script.RTScopeMesh != null)
                {
                    Camera scopeCamera = script.RTScopeMesh.GetComponentInChildren<Camera>();
                    if (scopeCamera == null)
                    {
                        GameObject go = new GameObject("Scope Camera");
                        go.transform.parent = script.RTScopeMesh.transform;
                        scopeCamera = go.AddComponent<Camera>();
                        scopeCamera.targetTexture = bl_ScopeProSettings.Instance.RenderTextureTemplate;
                        scopeCamera.cullingMask = bl_ScopeProSettings.Instance.ScopeCameraLayers;
                        if (script.PlayerCamera != null)
                        {
                            scopeCamera.transform.position = script.PlayerCamera.transform.position;
                            scopeCamera.transform.rotation = script.PlayerCamera.transform.rotation;
                        }

                        EditorUtility.SetDirty(scopeCamera);
                    }
                    script.RenderCamera = scopeCamera;
                }


                if (script.ScopeRTMaterial != null)
                {
                    materialEditor = (MaterialEditor)CreateEditor(script.ScopeRTMaterial);
                }
            }
        }

        void DrawInspector()
        {
            GUILayout.Space(5);
            GUILayout.Label("Setup", "ChannelStripEffectBar");
            GUILayout.BeginVertical("box");
            script.NormalScopeMesh = EditorGUILayout.ObjectField("Normal Scope Mesh", script.NormalScopeMesh, typeof(GameObject), true) as GameObject;
            if (script.RTScopeMesh != null)
                script.RTScopeMesh = EditorGUILayout.ObjectField("RT Scope Mesh", script.RTScopeMesh, typeof(GameObject), true) as GameObject;
            script.PlayerCamera = EditorGUILayout.ObjectField("Player Camera", script.PlayerCamera, typeof(Camera), true) as Camera;

            if (script.RenderCamera == null)
                script.RenderCamera = EditorGUILayout.ObjectField("Render Camera", script.RenderCamera, typeof(Camera), true) as Camera;

            GUI.enabled = true;
            if (script.ScopeRTMaterial == null)
            {
                script.ScopeRTMaterial = EditorGUILayout.ObjectField("RT Material", script.ScopeRTMaterial, typeof(Material), true) as Material;
            }
            GUILayout.EndVertical();

            if (script.PlayerCamera != null && script.RenderCamera != null)
            {
                if (GUILayout.Button("Align Cameras"))
                {
                    script.ApplyProps();
                }
            }

            if (materialEditor != null)
            {
                GUILayout.Space(5);
                GUILayout.Label("Material", "ChannelStripEffectBar");
                materialEditor.DrawHeader();
                materialEditor.OnInspectorGUI();
            }
        }
    }
}