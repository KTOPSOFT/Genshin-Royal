using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

public class bl_ULoginWebRequest 
{
    public MonoBehaviour instanceReference;

    public bl_ULoginWebRequest(MonoBehaviour reference)
    {
        instanceReference = reference;
    }

    public void POST(string uri, WWWForm postData, Action<ULoginResult> callBack, Dictionary<string, string> headers = null)
    {
        instanceReference.StartCoroutine(DoPost(uri, postData, callBack, headers));
    }

    public void GET(string uri, Action<ULoginResult> callBack, Dictionary<string,string> headers = null)
    {
        instanceReference.StartCoroutine(DoGet(uri, callBack, headers));
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator DoPost(string uri, WWWForm postData, Action<ULoginResult> callBack, Dictionary<string, string> headers)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(uri, postData))
        {
            if(headers != null && headers.Count > 0)
            {
                foreach(KeyValuePair<string,string> h in headers)
                {
                    www.SetRequestHeader(h.Key, h.Value);
                }
            }

            yield return www.SendWebRequest();
            ULoginResult result = new ULoginResult(www);
            if (callBack != null) { callBack.Invoke(result); }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator DoGet(string uri, Action<ULoginResult> callBack, Dictionary<string, string> headers)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(uri))
        {
            if(headers != null)
            {
                foreach (KeyValuePair<string,string> item in headers)
                {
                    www.SetRequestHeader(item.Key, item.Value);
                }
            }
            yield return www.SendWebRequest();
            ULoginResult result = new ULoginResult(www);
            if (callBack != null) { callBack.Invoke(result); }
        }
    }
}