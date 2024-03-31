using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFPSEditor;
using UnityEditor;

public class LevelSystemDocumentation : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/level-system/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-0.jpg", Image = null},
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.png", Image = null},
    };
    private readonly GifData[] AnimatedImages = new GifData[]
    {
        new GifData{ Path = "createwindowobj.gif" },
        new GifData{ Path = "addnewwindowfield.gif" },
    };
    private Steps[] AllSteps = new Steps[] {
    new Steps { Name = "Get Started", StepsLenght = 0, DrawFunctionName = nameof(DocGetStarted) },
    new Steps { Name = "Add Level", StepsLenght = 0, DrawFunctionName = nameof(DocAddLevel) },
    new Steps { Name = "Lock Items by Level", StepsLenght = 0, DrawFunctionName = nameof(DocLockItems) },
    };

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }
    //final required////////////////////////////////////////////////

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder, AnimatedImages);
        //FetchWebTutorials("mfps2/tutorials/level/");
        allowTextSuggestions = true;
    }

    void DocGetStarted()
    {
        DrawSuperText("<?title=18>LEVEL SYSTEM</title>\n\n<b><size=16>GET STARTED:</size></b>\n\n1. <b>Enable the addon:</b>\n  - In the Editor top menu, go to: MFPS ➔ Addons ➔ LevelManager ➔ <i>(Click)</i><b>Enable.</b>\n\n2. <b>Integrate:</b>\n  - Open the <?link=asset:Assets/MFPS/Scenes/MainMenu.unity>MainMenu</link> scene and then in the Editor top menu again, go to: MFPS ➔ Addons ➔ LevelManager ➔ <b>Integrate</b>.\n\n3. <b>Integrate the Mid game notifier (optional)</b>\nIf you want to player to see a popup message when they level up in the middle of a match, you can integrate the Runtime Notifier:\n\n- Open the map scene in the Editor <i>(You have to do this in each map of your game).</i>\n- Go to MFPS ➔ Addons ➔ LevelManager ➔ <b>Integrate RLN</b>.\n- Save the change to the scene.");
    }

    void DocAddLevel()
    {
        DrawSuperText("The package comes with 78 pre-setup example levels, but you can modify these, add more or remove them easily,\nall you have to do is simply add/remove an item from the list of the levels in the inspector window, or in the case you wanna modify one of the existing ones, you simply have to modify the properties fields in the inspector as well.\n \n<?title=16>ADD A NEW LEVEL</title>\n\n- Select the <?link=asset:Assets/Addons/LevelSystem/Resources/LevelManager.asset>LevelManager</link> object which is located at <i>Assets ➔ Addons ➔ LevelSystem ➔ Resources ➔ <b>LevelManager</b></i>, ➔ in the inspector window, you will see the list <b>Levels</b>, in this list you define all the levels available with their respective information, so in order to add a new level you simply have to add a new item to the list and fill the information of the level.");
        DrawServerImage("img-0.png");
    }

    void DocLockItems()
    {
        DrawSuperText("Using this addon you can lock some MFPS 'items' by level, which means that you can block these items until the player reach certain level in-game.\n\nBy default you can use this feature with:\n\n<?list=■>Weapons\nWeapon Camos\nPlayer Skins</list>\nAll you have to do for lock one of these items is setup the Unlockability information of the item.\n\nFor the weapons, you can find the <b>Unlockability</b> section in the <b>GunInfo</b>, GameData ➔ AllWeapons ➔ *Foldout the Weapon* ➔ foldout the Unlockability section ➔ *");
        DrawServerImage("img-1.png");
        DrawSuperText("Then in <?underline=>Unlockability</underline> information ➔ <b>Unlock Method</b> ➔ you have to set the <b>Level Up Only</b> or <b>Purchase Or Level Up</b> option, then set the level in which this item will be unlock in the <b>Unlock At Level</b> field and that's");
        DrawHorizontalSeparator();
        DrawText("Same process is for the Weapon Camos and Player Skin, you can find the Unlockability information for those in:\n\n<b>Weapon Camos:</b> <i>Assets ➔ Addons ➔ Customizer ➔ Resources ➔ CustomizerData ➔ Global Camos ➔ <i>*Foldout the camo*</i> ➔ *</i>\n\n<b>Player Skins:</b> <i>Assets ➔ Addons ➔ PlayerSelector ➔ Resources ➔ PlayerSelector ➔ All Players ➔ <i>*Foldout the player info*</i> ➔ *</i>");
    }

    [MenuItem("MFPS/Addons/LevelManager/Documentation")]
    [MenuItem("MFPS/Tutorials/Level Manager")]
    static void Open()
    {
        GetWindow<LevelSystemDocumentation>();
    }
}