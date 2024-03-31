using MFPS.Internal.Structures;
using MFPSEditor;
using UnityEngine;
using MFPS.Addon.Avatars;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPS.Addon.Avatars
{
    [CreateAssetMenu(fileName = "New Avatar Data", menuName = "MFPS/Addons/Avatars/Avatar Data")]
    public class EmblemData : ScriptableObject
    {
        [SpritePreview(Height = 60, AutoScale = true), FormerlySerializedAs("Avatar")] public Texture2D Emblem;
        public Color BackgroundColor = Color.black;
        public Color OutlineColor = new Color(1, 1, 1, 0.25f);
        public Color VignetteColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);
        [Range(0.5f, 2)] public float Size = 1;
        public MFPSItemUnlockability Unlockability;

        private Sprite m_cachedSprite = null;

        /// <summary>
        /// Get the Avatar ID
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return bl_EmblemsDataBase.GetEmblemID(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Sprite GetEmblemAsSprite()
        {
            if (m_cachedSprite == null && Emblem != null) m_cachedSprite = Sprite.Create(Emblem, new Rect(0, 0, Emblem.width, Emblem.height), Vector2.zero);

            return m_cachedSprite;
        }

        /// <summary>
        /// If this emblem is the current equipped one?
        /// </summary>
        /// <returns></returns>
        public bool IsEquipped()
        {
            int equippedId = bl_EmblemsDataBase.GetUserEmblem().GetID();
            return GetID() == equippedId;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(EmblemData))]
public class EmblemDataEditor : Editor
{
    EmblemData script;
    Texture2D vignette;
    private bool isListed = false;

    private void OnEnable()
    {
        script = (EmblemData)target;
        vignette = bl_EmblemsDataBase.Instance.vignetteTexture;
        isListed = bl_EmblemsDataBase.Instance.emblems.Contains(script);
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical("box");
        base.OnInspectorGUI();
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal("box");
        GUILayout.FlexibleSpace();
        var r = GUILayoutUtility.GetRect(100, 100);

        if(script.OutlineColor.a > 0)
        {

            var rr = r;
            rr.width = rr.height = 102;
            rr.x -= 1;
            rr.y -= 1;

            EditorGUI.DrawRect(rr, script.OutlineColor);
        }

        EditorGUI.DrawRect(r, script.BackgroundColor);
        if(vignette != null)
        {
            GUI.color = script.VignetteColor;
            GUI.DrawTexture(r, vignette);
            GUI.color = Color.white;
        }
        if(script.Emblem != null)
        {
            float size = 100 * script.Size;
            float dif = (size - 100) * 0.5f;
            var rr = r;
            rr.width = rr.height = size;
            rr.x -= dif;
            rr.y -= dif;
            GUI.DrawTexture(rr, script.Emblem);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if(!isListed)
        {
            if(GUILayout.Button("List this Emblem"))
            {
                bl_EmblemsDataBase.Instance.emblems.Add(script);
                EditorUtility.SetDirty(bl_EmblemsDataBase.Instance);
                isListed = true;
            }
        }
    }
}
#endif