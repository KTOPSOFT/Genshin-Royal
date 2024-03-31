using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.ULogin
{
    public class bl_AuthButton : MonoBehaviour
    {
        public AuthenticationType authenticationType = AuthenticationType.ULogin;

        /// <summary>
        /// 
        /// </summary>
        public void Auth()
        {
            bl_LoginPro.onRequestAuth?.Invoke(authenticationType);
        }
    }
}