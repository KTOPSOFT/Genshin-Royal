using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MFPS.ULogin
{
    public class bl_UINavigationBlocker : MonoBehaviour
    {
        [LovattoToogle] public bool unblockOnDisable = true;
        public Selectable singleSelectable;

        private GameObject lastSelected = null;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            bl_AxisUINavigation.blockedObject = singleSelectable.gameObject;
            lastSelected = EventSystem.current.currentSelectedGameObject;
            singleSelectable.Select();
        }

        private void OnDisable()
        {
            if (unblockOnDisable) bl_AxisUINavigation.blockedObject = null;

            if(lastSelected != null && lastSelected.GetComponent<Selectable>() != null)
            {
                lastSelected.GetComponent<Selectable>().Select();
            }
        }
    }
}