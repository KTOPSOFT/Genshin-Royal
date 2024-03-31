using System.Collections.Generic;
using UnityEngine;

namespace MFPS.ULogin
{
    public abstract class bl_LoginProBase : MonoBehaviour
    {
        private bl_ULoginWebRequest _webRequest;
        public bl_ULoginWebRequest WebRequest { get { if (_webRequest == null) { _webRequest = new bl_ULoginWebRequest(this); } return _webRequest; } }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WWWForm CreateForm(FormHashParm formHashParm = FormHashParm.Name, bool addSID = false) => bl_DataBaseUtils.CreateWWWForm(formHashParm, addSID);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WWWForm CreateForm(bool addBasicHash = true, bool addSID = false) => bl_DataBaseUtils.CreateWWWForm(addBasicHash, addSID);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> CreateHeaderContainer() => new Dictionary<string, string>();

        /// <summary>
        /// 
        /// </summary>
        public bl_DataBase DataBase => bl_DataBase.Instance;

        /// <summary>
        /// 
        /// </summary>
        public bl_LoginProDataBase ULoginSettings => bl_LoginProDataBase.Instance;

        /// <summary>
        /// 
        /// </summary>
        public ULoginUpdateFields NewUpdateFields() => new ULoginUpdateFields();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetURL(bl_LoginProDataBase.URLType url) => ULoginSettings.GetUrl(url);

        public string MD5Hash(string paramenter)
        {
            return bl_DataBaseUtils.Md5Sum(paramenter);
        }
    }
}