using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Addon.ScopePro
{
    [CreateAssetMenu(fileName = "ScopeProSettings", menuName = "MFPS/Addons/ScopePro/Settings")]
    public class bl_ScopeProSettings : ScriptableObject
    {
        public LayerMask ScopeCameraLayers;
        public RenderTexture RenderTextureTemplate;
        public Material[] templateScopeMaterials;
        public Texture2D vignetteTexture;

        private static bl_ScopeProSettings m_Data;
        public static bl_ScopeProSettings Instance
        {
            get
            {
                if (m_Data == null)
                {
                    m_Data = Resources.Load("ScopeProSettings", typeof(bl_ScopeProSettings)) as bl_ScopeProSettings;
                }
                return m_Data;
            }
        }
    }
}