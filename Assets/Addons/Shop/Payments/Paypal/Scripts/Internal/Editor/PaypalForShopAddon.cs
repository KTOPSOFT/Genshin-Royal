using UnityEngine;
using UnityEditor;
using MFPSEditor;
using UnityEditor.SceneManagement;

public class PaypalForShopAddon
{
    private const string DEFINE_KEY = "SHOP_PAYPAL";

#if !SHOP_PAYPAL
    [MenuItem("MFPS/Addons/Shop/Paypal/Enable")]
    private static void Enable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, true);
    }
#else
    [MenuItem("MFPS/Addons/Shop/Paypal/Disable")]
    private static void Disable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, false);
    }
#endif

    [MenuItem("MFPS/Addons/Shop/Paypal/Integration")]
    private static void Instegrate()
    {

        if (EditorSceneManager.sceneCountInBuildSettings <= 0)
        {
            Debug.LogWarning("Scenes has not been added in Build Settings, Can't integrate the Add-on.");
            return;
        }

        if (!EditorSceneManager.GetActiveScene().name.Contains("MainMenu"))
        {
            EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new UnityEngine.SceneManagement.Scene[] { EditorSceneManager.GetActiveScene() });
            var scene = EditorSceneManager.OpenScene("Assets/MFPS/Scenes/MainMenu.unity", OpenSceneMode.Single);
            if(scene == null)
            {
                Debug.LogWarning("Couldn't open the MainMenu scene to integrate the addon.");
                return;
            }
        }

        Transform lobby = Object.FindObjectOfType<bl_Lobby>().transform;

#if SHOP
        var sm = lobby.GetComponentInChildren<bl_ShopManager>(true);
        if(sm == null)
        {
            Debug.LogWarning("Couldn't found the Shop UI in the lobby, Shop addon no integrated yet maybe?");
            return;
        }

        var ppInstance = sm.GetComponentInChildren<bl_Paypal>(true);
        if(ppInstance == null)
        {
            GameObject instance = AddonIntegrationWizard.InstancePrefab("Assets/Addons/Shop/Payments/Paypal/Prefabs/Paypal.prefab", false);
            instance.transform.SetParent(sm.transform, false);
            instance.transform.SetAsLastSibling();
            EditorUtility.SetDirty(instance);
            AddonIntegrationWizard.ShowSuccessIntegrationLog(instance, "Paypal For Shop");
        }
#endif

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }
}