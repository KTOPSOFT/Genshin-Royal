using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.ULogin
{
    public class bl_AdminOperations : bl_LoginProBase
    {
        [SerializeField] private InputField coinsInputField = null;
        [SerializeField] private Dropdown coinDropdown = null;

        /// <summary>
        /// 
        /// </summary>
        public void UpdateCoins(bool add)
        {
            int coins = coinsInputField.text.ToInt();
            if(coins <= 0)
            {
                Debug.LogWarning("Coins have to be > 0 to request an update operation.");
                return;
            }
            if (!bl_AdminUserPanel.Instance.HasUserFetched) return;

            ConfirmOp(() =>
            {
                Debug.Log($"Send: {coins} to coinID {coinDropdown.value}");
                var wf = CreateForm(false, true);
                wf.AddSecureField("id", bl_AdminUserPanel.Instance.CurrentUser.ID);
                wf.AddSecureField("name", bl_AdminUserPanel.Instance.CurrentUser.LoginName);
                wf.AddSecureField("typ", DBCommands.DATABASE_UPDATE_USER_COINS);
                wf.AddSecureField("values", coins);
                wf.AddSecureField("param", coinDropdown.value);
                wf.AddSecureField("key", add ? 1 : 2);
                string hash = MD5Hash(bl_AdminUserPanel.Instance.CurrentUser.LoginName + bl_LoginProDataBase.Instance.SecretKey).ToLower();
                wf.AddSecureField("hash", hash);

                WebRequest.POST(GetURL(bl_LoginProDataBase.URLType.DataBase), wf, (result) =>
                {
                    if (result.isError)
                    {
                        result.PrintError();
                        return;
                    }
                    if (ULoginSettings.FullLogs) result.Print();

                    if (result.resultState == ULoginResult.Status.Success)
                    {
                        int newCoins = 0;
                        if (int.TryParse(result.Text, out newCoins))
                        {
                            bl_AdminUserPanel.Instance.CurrentUser.SetCoinLocally(coinDropdown.value, newCoins);
                            Debug.LogFormat("User coins updated, the new total is: {0}", newCoins);
                            bl_AdminUserPanel.Instance.ShowUserInfo(null);
                            coinsInputField.text = "0";
                            coinsInputField.transform.parent.gameObject.SetActive(false);
                        }
                        else
                        {
                            Debug.LogWarning("Unknown response: " + result.RawText);
                        }
                    }
                    else result.Print(true);
                });
            });
        }

        public void ConfirmOp(Action callback)
        {
            bl_AdminUserPanel.Instance.confirmationWindow.AskConfirmation("Confirm Operation", callback);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetupCoinWindow()
        {
            coinDropdown.ClearOptions();
            var ops = new List<Dropdown.OptionData>();
            var coins = bl_MFPS.Coins.GetAllCoins();
            for (int i = 0; i < coins.Count; i++)
            {
                ops.Add(new Dropdown.OptionData()
                {
                    text = coins[i].name
                });
            }
            coinDropdown.AddOptions(ops);
        }
    }
}