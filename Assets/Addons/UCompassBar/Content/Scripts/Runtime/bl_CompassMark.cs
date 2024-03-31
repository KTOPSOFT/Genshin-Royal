using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Addon.Compass
{
    public class bl_CompassMark : MonoBehaviour
    {
        public ActivationType m_ActivationType = ActivationType.OnEnable;
        public Sprite Icon;
        public Color IconColor = Color.white;

        private bool MarkSet = false;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            if (m_ActivationType == ActivationType.OnEnable)
            {
                if (!MarkSet)
                {
                    CompassMarkEvent.SetCompassMark(Mark);
                    MarkSet = true;
                }
                else
                {
                    CompassMarkEvent.ShowMark(transform, true);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_ActivationType != ActivationType.OnTrigger)
                return;

            if (other.transform.CompareTag("Player"))
            {
                CompassMarkEvent.SetCompassMark(Mark);
                MarkSet = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDestroy()
        {
            if (MarkSet)
            {
                CompassMarkEvent.DestroyMark(transform);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            if (MarkSet)
            {
                CompassMarkEvent.ShowMark(transform, false);
            }
        }

        private CompassMark Mark
        {
            get
            {
                CompassMark mark = new CompassMark();
                mark.Target = transform;
                mark.Icon = Icon;
                mark.IconColor = IconColor;
                return mark;
            }
        }

        [System.Serializable]
        public enum ActivationType
        {
            OnEnable,
            OnTrigger,
        }
    }
}