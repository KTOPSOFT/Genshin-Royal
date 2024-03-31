using System;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Security;

public class bl_RSA
{
    private const int KEY_LENGHT = 2048;

    /// <summary>
    /// 
    /// </summary>
    public static string Encrypter(string data, string publicKey)
    {
        if(string.IsNullOrEmpty(publicKey) || publicKey.Length < 100)
        {
            Debug.LogError("RSA key is null or not valid.");
            return string.Empty;
        }

        try
        {
            var csp = new RSACryptoServiceProvider(KEY_LENGHT);
            csp.FromXmlString(publicKey);
            var byteArray = Encoding.UTF8.GetBytes(data);
            var bytesCypherText = csp.Encrypt(byteArray, false);
            return Convert.ToBase64String(bytesCypherText);
        }
#if NET_4_6
        catch(XmlSyntaxException e)
        {
            Debug.LogError($"XML parsing error: {e.Message} key: {publicKey}");
            return "";
        }
#endif
        catch (XmlException e)
        {
            Debug.LogError($"XML error: {e.Message} key: {publicKey}");
            return "";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string Decrypt(string encrypted, string publicKey)
    {
        if (string.IsNullOrEmpty(publicKey) || publicKey.Length < 100)
        {
            Debug.LogError("RSA key is null or not valid.");
            return string.Empty;
        }

        var csp = new RSACryptoServiceProvider(KEY_LENGHT);
        csp.FromXmlString(publicKey);
        var byteArray = Encoding.UTF8.GetBytes(encrypted);
        var bytesCypherText = csp.Decrypt(byteArray, false);
        return Convert.ToBase64String(bytesCypherText);
    }
}