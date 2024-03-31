using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace MFPS.Addon.Compass
{
    public class bl_CompassMagnet : MonoBehaviour
    {
        private Transform Compass;
        private PhotonView View;

        private void OnEnable()
        {
            View = transform.root.GetComponent<PhotonView>();
            if (View != null && !View.IsMine) return;

            if (Compass == null)
            {
                GameObject g = new GameObject("Compass");
                Compass = g.transform;
                Compass.parent = transform;
                Compass.localPosition = Vector3.zero;
                Compass.localRotation = Quaternion.identity;
            }
            CompassMarkEvent.SetCompassCamera(Compass);
        }

        private void Update()
        {
            if (Compass == null || View == null) return;
            if (!View.IsMine) return;

            Vector3 v = Compass.eulerAngles;
            v.x = 0;
            Compass.eulerAngles = v;
            //Debug.DrawRay(Compass.position, Compass.forward, Color.red);
        }
    }
}