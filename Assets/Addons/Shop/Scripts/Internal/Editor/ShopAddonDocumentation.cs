using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;

public class ShopAddonDocumentation : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/shop/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.jpg", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
        new NetworkImages{Name = "img-5.jpg", Image = null},
        new NetworkImages{Name = "img-6.jpg", Image = null},
        new NetworkImages{Name = "img-7.jpg", Image = null},
        new NetworkImages{Name = "img-8.jpg", Image = null},
    };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "Integrate", StepsLenght = 0 },
    new Steps { Name = "Add Weapon", StepsLenght = 0 },
    new Steps { Name = "Coins", StepsLenght = 0 },
    new Steps { Name = "Add Payment", StepsLenght = 0 },
    new Steps { Name = "Add Skin", StepsLenght = 0 },
    new Steps { Name = "Add Camo", StepsLenght = 0 },
    };
    //final required////////////////////////////////////////////////

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder);
        GUISkin gs = Resources.Load<GUISkin>("content/MFPSEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
        }
    }

    public override void WindowArea(int window)
    {
        if (window == 0)
        {
            DrawIntegrate();
        }
        else if (window == 1)
        {
            DrawAddWeapon();
        }
        else if (window == 2)
        {
            DrawAddCoins();
        }
        else if (window == 3)
        {
            DrawAddPayment();
        }else if(window == 4) { DrawAddSkin(); }
        else if (window == 5) { DrawAddCamo(); }
    }

    void DrawIntegrate()
    {
        DrawText("<b>Require:</b>\nMFPS 1.9++\nULogin Pro 2.0.0++\n \nIn order to integrate, first make sure you have the addon enable:");
#if SHOP
        DrawText("<color=green>Addon is Enabled</color>");
#else
                DrawText("Addon is not enabled yet, for enable it click in the button bellow");
        if (DrawButton("ENABLE"))
        {
            EditorUtils.SetEnabled("SHOP", true);
        }
#endif
        DrawHorizontalSeparator();

        DrawText("<b><size=14>Integration in the MainMenu scene.</size></b>\n\nThe addon requires to be integrate once in the <i>MainMenu</i> scene, for it simply click in the button below");
        Space(10);
        if(GUILayout.Button("MainMenu Integration", MFPSEditorStyles.EditorSkin.customStyles[11], GUILayout.Width(200)))
        {
            ShopAddonIntegration.Open();
        }
        DrawHorizontalSeparator();

        DrawText("Finally, you have to upload a PHP script in the same directory where you uploaded the ULogin Pro PHP scripts to your server.\n \nThe script that you have to upload is located in: <i>Assets ➔ Addons ➔ Shop ➔ Scripts ➔ Php ➔ <b>bl_Shop.php</b></i>, if you don't know how to upload files to your server, please check the ULogin Pro documentation.\n \nOnce you upload this file you are all set and you can start using the shop addon in your game.");
    }

    void DrawAddWeapon()
    {
        DrawSuperText("In order to make a weapon to be listed in the shop, you simply have to setup the unlockeability information of the weapon, you can do this in the weapon information in the <?link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link> ➔ <b>AllWeapons</b> list.\n  \nGo to MFPS <i><size=8><color=#76767694>(folder)</color></size></i> ➔ Resources ➔ <?link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link> ➔ AllWeapons ➔ *, now in each weapon that you wanna listed in the shop, foldout the <?underline=>Unlockeability</underline> section and in the <?underline=>Unlock Method</underline> dropdown select <b>Purchase Only</b> or <b>Purchase Or Level Up</b> ➔ then set the weapon price in the <b>Price</b> field.");
        DrawServerImage("img-6.png");
        DrawText("Once you set a price > 0 the weapon will automatically appear in the Shop menu.");
    }

    void DrawAddCoins()
    {
        DrawHyperlinkText("This system comes with 4 example <b>Coin Packs</b> that player can buy in-game, but you can add or edit as many as you wish.\n\nFor it, you simply have to add / modify the pack information in: Addons (Folder) ➔ Shop ➔ Resources ➔ <link=asset:Assets/Addons/Shop/Resources/ShopData.asset>ShopData</link> ➔ <i><size=8><color=#76767694>(Inspector window)</color></size></i> <b>CoinsPack</b> ➔ In that list you can add more options or edit one of the existing ones:");
        DrawServerImage("img-7.png");
        DrawHorizontalColumn("Name", "The name of the coin pack");
        DrawHorizontalColumn("ID", "An unique identifier for this coin pack, some stores like Google Play required this ID to identify the IAP products.");
        DrawHorizontalColumn("Coins", "The amount of coins that this pack give");
        DrawHorizontalColumn("Bonus Coins", "The amount of coins that this pack give as a bonus/extra.");
        DrawHorizontalColumn("Price", "The real price (in a real currency) that this pack cost.");
        DrawHorizontalColumn("Pack Icon", "A sprite that represent this coin pack, this will be shown in the coins pack store list.");
        DrawHorizontalColumn("Highlight", "Set true for the package that you consider as the basic or more way to go.");
    }

    void DrawAddPayment()
    {
        DrawText("The coins are the virtual money in the game, in MFPS for default players earn coins playing the game (by kills, wins, etc...), so these coins are used to purchase in this shop system," +
    " if player want to get more coins without playing they have the options to purchase these, now this shops system implement just the half of the integration of this 'coins purchase'," +
    "this system have all the local logic, show coins pack options, save coins in database, but doesn't include any real payment system, like for example paypal or UnityIAP, why? well basically due 2 main reasons, first because there are many " +
    " platforms and almost each one of them require that use their own IAP / API system for process real money purchases, for example Android, IOS, Steam, Kongregate, etc... so integrate for example Paypal IPN " +
    "will be useless in any of these platforms, so don't worth it, second reason is security.\n \nSo that is why this system doesn't comes with a payment system and is all up to the dev to integrated it," +
    " even though the system comes with all setup to make easy integrate any payment system, there is how: well just superficially since each API is a bit different:\n \n<b>#1</b> -You may ask where should I process the payment (after player " +
    "have selected the coin pack and click on the purchase button), here is where: <b>bl_ShopManager.cs ->  public void BuyCoinPack(int id))</b>, you'll see a <b>Switch</b> conditional with some payment options," +
    "use any of these for example is you are trying to integrate UnityIAP, put the code after: <b>case bl_ShopData.ShopPaymentTypes.UnityIAP:</b>,\n \n now there are some basic information that you may need to send to the payment API, " +
    "like a name and the price to charge, you can get it from the local variable <b>coinPack</b> -> coinPack.Name, coinPack.Price, etc...");
        DownArrow();
        DrawText("Right after you have implement the Payment system and you have tested you have to save these coins to the player info in your database (after the payment is confirmed), for this simply " +
            "where you receive the local confirmation from your payment api you call <b>bl_DataBase.Instance.SaveNewCoins(coinPack.Amount);</b> if you are calling inside bl_ShopManager.cs, if you are calling from other script:" +
            " <b>bl_DataBase.Instance.SaveNewCoins(bl_ShopManager.Instance.coinPack.Amount);</b>\n \nThat's.");
    }

    void DrawAddSkin()
    {
        DrawHyperlinkText("In order to add a player skin in the shop to be available only by purchase you simply need the <link=https://www.lovattostudio.com/en/shop/addons/player-selector/>Player Selector</link> addon.\n \nIf you already have imported <link=https://www.lovattostudio.com/en/shop/addons/player-selector/>Player Selector</link> in your project and added your customs player prefabs, then all what you need is setup the Unlockability of the  player info.\n\nFor it go to PlayerSelector located in: Assets ➔ Addons ➔ PlayerSelector ➔  Resources ➔ PlayerSelector ➔ fold out the player prefab info <i>(from <b>All Players</b> list)</i> ➔ foldout the <b>Unlockability</b> section ➔ in the <b>Unlock Method</b> set <b>Purchase Only</b> or <b>Purchase or Level Up</b>, then set the price for the player and that's.");
        DrawServerImage("img-8.png");
    }

    void DrawAddCamo()
    {
        DrawHyperlinkText("In order to add a weapon camo in the shop to be available only by purchase you need <link=https://www.lovattostudio.com/en/shop/addons/customizer/>Customizer</link> addon.\n \nOnce you import Customizer in your project and added your weapon camos, then all what you need is to setup the Unlockability information of the Global Camos that you wanna sell.\n\nFor it, go to <i>Assets ➔ Addons ➔ Customizer ➔ Resources ➔ <b>CustomizerData</b></i> ➔ In the inspector window you will have the <b>Global Camos</b> list, foldout the camo that you wanna setup for sell in the shop ➔ foldout the Unlockability section ➔ in the <b>Unlock Method</b> set <b>Purchase Only</b> or <b>Purchase or Level Up</b>, then set the price for the camo and that's.");
        DrawServerImage("img-9.png");
    }

    [MenuItem("MFPS/Tutorials/Shop")]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ShopAddonDocumentation));
    }

    [MenuItem("MFPS/Addons/Shop/Tutorial")]
    private static void ShowWindow2()
    {
        EditorWindow.GetWindow(typeof(ShopAddonDocumentation));
    }
}