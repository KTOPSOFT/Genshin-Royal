using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.ULogin
{
    public class ULoginUpdateFields
    {
        public List<Data> datas;

        /// <summary>
        /// 
        /// </summary>
        public void AddField(string key, int value)
        {
            AddField(key, value.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddField(string key, string value)
        {
            if (datas == null) datas = new List<Data>();
            if (datas.Exists(x => x.Key == key))
            {
                Debug.LogErrorFormat("A key '{0}' already exist in this list.", key);
                return;
            }

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("The 'key' value can't be null.");
                return;
            }

            key = bl_DataBaseUtils.SanitazeString(key);
            value = bl_DataBaseUtils.SanitazeString(value);

            datas.Add(new Data()
            {
                Key = key,
                Value = value,
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wf"></param>
        public WWWForm AddToWebForm(WWWForm wf)
        {
            if (datas == null || datas.Count <= 0)
                return wf;

            wf.AddSecureField("key", GetKeysAsString());
            wf.AddSecureField("values", GetValuesAsString());
            return wf;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetKeysAsString()
        {
            return string.Join("|", datas.Select(x => x.Key));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetValuesAsString()
        {
            return string.Join("|", datas.Select(x => x.Value));
        }

        public class Data
        {
            public string Key;
            public string Value;
        }
    }
}