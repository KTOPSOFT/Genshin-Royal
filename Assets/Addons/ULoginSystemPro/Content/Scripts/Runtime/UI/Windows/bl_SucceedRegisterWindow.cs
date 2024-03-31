using TMPro;
using UnityEngine;

namespace MFPS.ULogin
{
    public class bl_SucceedRegisterWindow : bl_LoginProBase
    {
        [TextArea(2, 5)] public string emailVerificationMessage, allSetMessage;

        public TextMeshProUGUI indicationText;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            if (ULoginSettings.RequiredEmailVerification)
            {
                indicationText.text = emailVerificationMessage;
            }
            else
            {
                indicationText.text = allSetMessage;
            }
        }
    }
}