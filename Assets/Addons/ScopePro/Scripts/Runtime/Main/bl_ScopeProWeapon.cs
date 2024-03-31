using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Addon.ScopePro
{
    [RequireComponent(typeof(bl_Gun))]
    public class bl_ScopeProWeapon : MonoBehaviour
    {
        private bl_Gun m_Gun;
        private bl_ScopePro ScopeSetup;
        private bool isAiming = false;
        private Color UIColor = new Color(0, 0, 0, 0);
        private float Alpha = 0;

        private Texture2D vignetteTexture;

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            m_Gun = GetComponent<bl_Gun>();
            vignetteTexture = bl_ScopeProSettings.Instance.vignetteTexture;
            GetScopeSetup();
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetScopeSetup()
        {
            ScopeSetup = GetComponentInChildren<bl_ScopePro>();

            if (m_Gun != null && ScopeSetup != null)
            {
                float percentage = m_Gun.aimZoom / 60;
                percentage = 1 - percentage;
                percentage = Mathf.Clamp(percentage, 0, 20);
                int compensation = Mathf.FloorToInt(20 * percentage);
                float zoom = m_Gun.aimZoom - compensation;

                ScopeSetup.RenderCamera.fieldOfView = Mathf.Clamp(zoom, 5, 105);
                m_Gun.BlockAimFoV = true;
                m_Gun.onWeaponRendersActive += OnWeaponRendersChange;
            }
            else
            {
                Debug.Log("Scope set up not found on this weapon.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void Update()
        {
            if (m_Gun == null || ScopeSetup == null)
                return;

            if ((m_Gun.isAiming && !m_Gun.isReloading) && !isAiming)
            {
                ScopeSetup.OnAim(true);
                isAiming = true;
            }
            else if ((!m_Gun.isAiming || m_Gun.isReloading) && isAiming)
            {
                ScopeSetup.OnAim(false);
                isAiming = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void OnWeaponRendersChange(bool active)
        {
            if (ScopeSetup == null) return;

            ScopeSetup.OnAim(m_Gun.isAiming);
        }

        /// <summary>
        /// 
        /// </summary>
        void OnGUI()
        {
            if (m_Gun == null || ScopeSetup == null || vignetteTexture == null)
                return;

            if (m_Gun.isAiming && !m_Gun.isReloading)
            {
                Alpha = Mathf.Lerp(Alpha, 1, Time.deltaTime * 8);
            }
            else
            {
                Alpha = Mathf.Lerp(Alpha, 0, Time.deltaTime * 8);
            }

            if (Alpha > 0)
            {
                UIColor.a = Alpha;
                GUI.color = UIColor;
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), vignetteTexture);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), vignetteTexture);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), vignetteTexture);
                GUI.color = Color.white;
            }
        }
    }
}