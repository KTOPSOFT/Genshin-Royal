using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace MFPS.Shop.Paypal
{
    [CreateAssetMenu(fileName = "PaypalSettings", menuName = "MFPS/Shop/Paypal")]
    public class bl_PaypalSettings : ScriptableObject
    {
        [Header("Live Credentials")]
        [SerializeField] private string clientID;
        [SerializeField] private string clientSecret;

        [Header("Sandbox Credentials")]
        [SerializeField] private string sandboxClientID;
        [SerializeField] private string sandboxClientSecret;

        [Header("Settings")]
        [LovattoToogle] public bool sandBoxMode = true;
        public PaypalCurrencyCodes currencyCode = PaypalCurrencyCodes.USD;
        public string redirectURL;
        public string cancelURL;
        public string bussinesName;

        const string paypalRestAPI = "https://api.paypal.com";
        const string paypalSandboxRestAPI = "https://api.sandbox.paypal.com";

        public string GetAPIEndPoint(string endPoint)
        {
            string baseUrl = sandBoxMode ? paypalSandboxRestAPI : paypalRestAPI;
            return $"{baseUrl}/{endPoint}";
        }

        public string GetAccessTokenHeader(AccessTokenResponse tokenResponse)
        {
            return $"{tokenResponse.token_type} {tokenResponse.access_token}";
        }

        public string GetAuthorizationHeader()
        {
            string cid = sandBoxMode ? sandboxClientID : clientID;
            string cs = sandBoxMode ? sandboxClientSecret : clientSecret;
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{cid}:{cs}"));
            return $"Basic {credentials}";
        }

        public static string GetClientID()
        {
            return Instance.sandBoxMode ? Instance.sandboxClientID : Instance.clientID;
        }

        public static string GetClientSecret()
        {
            return Instance.sandBoxMode ? Instance.sandboxClientSecret : Instance.clientSecret;
        }

        private static bl_PaypalSettings m_Data;
        public static bl_PaypalSettings Instance
        {
            get
            {
                if (m_Data == null)
                {
                    m_Data = Resources.Load("PaypalSettings", typeof(bl_PaypalSettings)) as bl_PaypalSettings;
                }
                return m_Data;
            }
        }
    }
}