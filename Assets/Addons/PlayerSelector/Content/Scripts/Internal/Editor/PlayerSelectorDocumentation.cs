using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFPSEditor;
using UnityEditor;

public class PlayerSelectorDocumentation : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/player-selector/";
    private NetworkImages[] ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-0.png", Image = null},
        new NetworkImages{Name = "img-1.png", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.jpg", Image = null},
    };
    private readonly GifData[] AnimatedImages = new GifData[]
    {
        new GifData{ Path = "none.gif" },

    };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "Integration", StepsLenght = 0, DrawFunctionName = nameof(DrawGetStarted) },
     new Steps { Name = "Add Player", StepsLenght = 0, DrawFunctionName = nameof(AddPlayersDoc) },
     new Steps { Name = "Assign Player Team", StepsLenght = 0, DrawFunctionName = nameof(PlayerTeamDoc) },
     new Steps { Name = "Add Bots", StepsLenght = 0, DrawFunctionName = nameof(AddBotsDoc) },
     new Steps { Name = "Edit InMatch UI", StepsLenght = 0, DrawFunctionName = nameof(EditInMatchUIDoc) },
    };

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }
    //final required////////////////////////////////////////////////

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(ServerImages, AllSteps, ImagesFolder, AnimatedImages);
        GUISkin gs = Resources.Load<GUISkin>("content/MFPSEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
            base.m_GUISkin = gs;
        }
    }

    void DrawGetStarted()
    {
        DrawText("•  After import the package in your MFPS 2.0 project go to (Top Bar) MFPS -> Addons -> PlayerSelector -> Enable\n\n•  Now choose between the two modes available for Character Selection mode: InLobby and InMatch:\n\n<b><size=22>In Match:</size></b>\n\n- Players select his operator/soldier character in match <i>(after enter in a room)</i> like in some games like OverWatch,RainBow Six, etc...\n\nFor integrate this mode go to -> (Unity Toolbar) MFPS -> Addons -> PlayerSelector -> <b>InMatch Integrate</b>\n\n<b><size=22>InLobby:</size></b>\n\n- Players Select his operator/soldier character in the Lobby Menu, they select the one for each team and set a favorite team to use ingame modes of solo player, this mode is use in some games like Call Of Duty, Fortnite,etc...\n\nFor integrate this mode open the <b>MainMenu</b> Scene then in Unity Toolbar Go to MFPS -> Addons -> PlayerSelector -> <b>InLobby Integrate</b>\n\n- Ready!.\n");
    }

    void AddPlayersDoc()
    {
        DrawText("In order to add a new player prefab, you need just 2 things:\n\n- The <b>Player prefab</b>, like the normal player prefabs that you set in GameData, this player prefab has to be located in a <b>Resources</b> folder.\n\n- <b>A Sprite of this player</b>, showcasing the player skin.\n\nOnce you have these two, you simple have to add this prefab in the <b>AllPlayer</b> list of <b>PlayerSelector</b> which is located in the Resources folder of the addon (<i>Assets->Addons->PlayerSelector->Content->Resources-><b>PlayerSelector</b></i>) like this:\n");
        DrawServerImage(0);
        DownArrow();
        DrawText("<b>Price:</b> if you set the price > 0, means that this player require to be purchase in order to select it, if you want it to be free, just leave the price as 0.\n");
    }

    void PlayerTeamDoc()
    {
        DrawText("Once you added a new player in the <b>AllPlayer</b> list you have to assign that player in the Team in which it can be selected, that in order to give you an option to set different players options for each team, but you can set the player in both teams so that it's available to select in both teams.\n\nYou can assign the player in a Team by adding a new field in the Team list <i>(Team1 or Team2)</i> in PlayerSelector, in the new field select the player name and that's.\n");
        DrawServerImage(1);
        DownArrow();
        DrawText("<b>FFA Player</b> list are the players that will be available to select for <b>ANY</b> solo player game mode.\n");
    }

    void AddBotsDoc()
    {
        DrawSuperText("By default in MFPS you can only assign a bot prefab per team, with Player Selector you can also add as many bot players variations as you want to each team.\n \nTo add a new bot prefab add it to the <b>Team 1 Bots</b> or <b>Team 2 Bots</b> list in <?link=asset:Assets/Addons/PlayerSelector/Resources/PlayerSelector.asset>PlayerSelector</link>.");
        DrawServerImage("img-2.png");
    }

    void EditInMatchUIDoc()
    {
        DrawSuperText("<i>Since version 1.7.3 the InMatch integration doesn't instance the UI selector in the map scenes, instead, it is instanced automatically in-runtime.</i>\n \nIf you want to modify the InMatch selector UI you simply need to drag the <?link=asset:Assets/Addons/PlayerSelector/Content/Prefabs/UI/Canvas[PlayerSelector].prefab>Canvas[PlayerSelector]</link> prefab located in <i>Assets ➔ Addons ➔ PlayerSelector ➔ Content ➔ Prefabs ➔ UI ➔ <?link=asset:Assets/Addons/PlayerSelector/Content/Prefabs/UI/Canvas[PlayerSelector].prefab>Canvas[PlayerSelector]</link>.prefab</i>, instance in any scene hierarchy > active the <b>Content</b> child inside the prefab so you can preview the UI > make the desired changes > <b>apply the changes to the prefab to save them</b>.");
        DrawServerImage("img-3.png");
    }

    [MenuItem("MFPS/Tutorials/Player Selector")]
    private static void Open()
    {
        EditorWindow.GetWindow(typeof(PlayerSelectorDocumentation));
    }

    [MenuItem("MFPS/Addons/PlayerSelector/Documentation")]
    private static void Open2()
    {
        EditorWindow.GetWindow(typeof(PlayerSelectorDocumentation));
    }
}