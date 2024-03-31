using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using MFPS.Shop.Paypal;
using MFPS.Shop;
#if ULSP
using MFPS.ULogin;
#endif

public class bl_Paypal : MonoBehaviour
{
    private AccessTokenResponse tokenResponse;
    private CapturePayment capturePayment;
    private OrderCaptureResponse orderCapture;

    [Header("References")]
    public GameObject loadingUI;
    public GameObject cancelButton;

    private string requestID;
    private bool isWaiting, isChecking = false;
    private bl_ShopData.ShopVirtualCoins requestingCoinPack;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        cancelButton.SetActive(false);
        loadingUI.SetActive(false);
    }

#if SHOP
    /// <summary>
    /// Start a new coin pack order
    /// </summary>
    public void PurchaseCoinPack(bl_ShopData.ShopVirtualCoins coinPack)
    {
        requestingCoinPack = coinPack;
        loadingUI.SetActive(true);

        //if the access token has not been fetch yet.
        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.access_token))
        {
            //get access token
            GetAccessToken(() =>
            {
                CaptureOrderOfCoinPack();
            });
        }
        else
        {
            CaptureOrderOfCoinPack();
        }
    }
#endif

    /// <summary>
    /// Purchase the previous requested coin pack
    /// </summary>
    public void CaptureOrderOfCoinPack()
    {
        //create the order info
        bl_PaypalOrder order = new bl_PaypalOrder();
        //set the order product
        order.AddUnit(requestingCoinPack.Name, requestingCoinPack.Name, requestingCoinPack.Price);
        //dispatch the order
        CreateOrder(order);
    }

    /// <summary>
    /// 
    /// </summary>
    public void CancelPurchase()
    {
        loadingUI.SetActive(false);
        cancelButton.SetActive(false);
        isWaiting = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void GetAccessToken(Action onSuccess = null)
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(DoGetToken(onSuccess));
        else
            bl_LobbyUI.Instance.StartCoroutine(DoGetToken(onSuccess));

        IEnumerator DoGetToken(Action callback)
        {
            WWWForm wf = new WWWForm();
            wf.AddField("grant_type", "client_credentials");
            using (UnityWebRequest w = UnityWebRequest.Post(GetAPIEndPoint("v1/oauth2/token"), wf))
            {
                w.SetRequestHeader("Authorization", bl_PaypalSettings.Instance.GetAuthorizationHeader());
                w.SetRequestHeader("Content-Type", "application/json");
                w.SetRequestHeader("Accept-Language", "en_US");
                yield return w.SendWebRequest();

                if (!bl_UtilityHelper.IsNetworkError(w))
                {
                    string text = w.downloadHandler.text;
                    tokenResponse = JsonUtility.FromJson<AccessTokenResponse>(text);
                    if (tokenResponse != null)
                    {
                        callback?.Invoke();
                    }
                    else
                    {
                        Debug.LogWarning($"Unknown response: {text}");
                        loadingUI.SetActive(false);
                    }
                }
                else
                {
                    Debug.LogError(w.error);
                    loadingUI.SetActive(false);
                }
            }

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void CreateOrder(bl_PaypalOrder orderBody)
    {
        if (string.IsNullOrEmpty(tokenResponse.access_token))
        {
            Debug.LogWarning("Access Token has not been fetch yet.");
            return;
        }

        if (gameObject.activeInHierarchy)
            StartCoroutine(DoCreatePayment());
        else
            bl_LobbyUI.Instance.StartCoroutine(DoCreatePayment());
        
        IEnumerator DoCreatePayment()
        {
            string jsonBody = JsonUtility.ToJson(orderBody);
            requestID = Guid.NewGuid().ToString();
            string url = GetAPIEndPoint($"v2/checkout/orders");
            using (UnityWebRequest w = UnityWebRequest.PostWwwForm(url, jsonBody))
            {
                w.SetRequestHeader("Authorization", bl_PaypalSettings.Instance.GetAccessTokenHeader(tokenResponse));
                w.SetRequestHeader("PayPal-Request-Id", requestID);
                SetJsonBodyToWWW(w, jsonBody);

                yield return w.SendWebRequest();

                if (!bl_UtilityHelper.IsNetworkError(w))
                {
                    string text = w.downloadHandler.text;
                    if (w.responseCode == 201)
                    {
                        capturePayment = JsonUtility.FromJson<CapturePayment>(text);
                        isWaiting = true;
                        Application.OpenURL(capturePayment.GetApproveURL);
                    }
                    else
                    {
                        Debug.LogWarning($"HTTPCode: {w.responseCode} - {text}");
                        loadingUI.SetActive(false);
                    }
                }
                else
                {
                    Debug.LogError(w.error);
                    loadingUI.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckPayment()
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(DoCheckPayment());
        else
            bl_LobbyUI.Instance.StartCoroutine(DoCheckPayment());

        IEnumerator DoCheckPayment()
        {
            isChecking = true;
            string url = GetAPIEndPoint($"v2/checkout/orders/{capturePayment.id}/capture");
            using (UnityWebRequest w = UnityWebRequest.PostWwwForm(url, ""))
            {
                w.SetRequestHeader("Authorization", bl_PaypalSettings.Instance.GetAccessTokenHeader(tokenResponse));
                w.SetRequestHeader("PayPal-Request-Id", requestID);
                w.SetRequestHeader("Content-Type", "application/json");

                yield return w.SendWebRequest();

                if (!bl_UtilityHelper.IsNetworkError(w))
                {
                    string text = w.downloadHandler.text;
                    if (w.responseCode == 201)
                    {
                        orderCapture = JsonUtility.FromJson<OrderCaptureResponse>(text);
                        if (orderCapture != null)
                        {
                            if (orderCapture.isCompleted)
                            {
                                ConfirmCoinPurchase(orderCapture);
                            }
                            else
                            {
                                loadingUI.SetActive(false);
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"HTTPCode: {w.responseCode} - {text}");
                            loadingUI.SetActive(false);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"HTTPCode: {w.responseCode} - {text}");
                        loadingUI.SetActive(false);
                    }
                    isWaiting = false;
                }
                else
                {
                    if (w.responseCode != 422)
                    {
                        Debug.LogError(w.error);
                        isWaiting = false;
                        loadingUI.SetActive(false);
                    }
                    else
                    {
                        Debug.Log("Payment is not captured yet.");
                        cancelButton.SetActive(true);
                    }
                }
            }
            isChecking = false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void ConfirmCoinPurchase(OrderCaptureResponse orderCapture)
    {
#if ULSP
        //build the purchase data
        var data = new CoinPurchaseData();
        data.coins = requestingCoinPack.GetCoins();
        data.productID = requestingCoinPack.ID;
        data.receipt = orderCapture.GetMinimalReceipt();
#if SHOP
        data.coinID = bl_ShopData.Instance.CoinToApplyPurchases;
#endif

        if (bl_DataBase.Instance == null)
        {
            Debug.Log($"Purchase has been complete but user is not logged, coins are not going to be stored");
            return;
        }
        //save a copy of the purchase information in our game database for the record
        bl_DataBase.Instance.SetCoinPurchase(data, (success) =>
        {
            //data saved
            if (success)
            {
                //mark purchase as complete
                bl_EventHandler.DispatchCoinUpdate(null);
#if SHOP
                bl_ShopNotification.Instance?.Show($"<b><size=20>PURCHASE COMPLETE</size></b>\n{requestingCoinPack.GetCoins()} COINS HAS BEEN ADDED TO YOUR ACCOUNT.").Hide(7);
#endif
            }
            else
            {
                Debug.LogError("Couldn't save purchase in DataBase!");
            }
            loadingUI.SetActive(false);
        });
#else
        bl_GameData.Instance.VirtualCoins.AddCoins(requestingCoinPack.GetCoins(), bl_PhotonNetwork.LocalPlayer.NickName);
        bl_EventHandler.DispatchCoinUpdate(null);
#endif
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus && isWaiting && !isChecking)
        {
            CheckPayment();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetJsonBodyToWWW(UnityWebRequest w, string json)
    {
        var postData = Encoding.UTF8.GetBytes(json);
        w.uploadHandler = new UploadHandlerRaw(postData);
        w.SetRequestHeader("Content-Type", "application/json");
    }

    string GetAPIEndPoint(string endPoint) => bl_PaypalSettings.Instance.GetAPIEndPoint(endPoint);
    public static bl_PaypalSettings Settings => bl_PaypalSettings.Instance;

    private static bl_Paypal _instance;
    public static bl_Paypal Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_Paypal>(); }
            if (_instance == null && bl_LobbyUI.Instance != null) _instance = bl_LobbyUI.Instance.GetComponentInChildren<bl_Paypal>(true);
            return _instance;
        }
    }
}