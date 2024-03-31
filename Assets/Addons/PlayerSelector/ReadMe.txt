Thanks for purchase "Player Selector" Add-on for MFPS 2.0 from Lovatto Studio.
Version 1.7.3

Required:--------------------------------------------------------------------------------------------------------------------------------------------------------

- Unity 2019.4++
- MFPS 2.0 v1.9.1++

Integration:

- After import the package in your MFPS 2.0 project go to (Top Bar) MFPS -> Addons -> PlayerSelector -> Enable (if is disable)
- Now choose between the two modes available for Character Selection mode: InLobby and InMatch:

    InMatch:
	   - Players select his operator/soldier character in match (after enter in a room) like in some games like OverWatch,RainBow Six, etc...
	    For integrate this mode go to -> (Unity Toolbar) MFPS -> Addons -> PlayerSelector -> InMatch Integration

   InLobby:
       - Players Select his operator/soldier character in the Lobby Menu, they select the one for each team and set a favorite team to use in
	     game modes of solo player, this mode is use in some games like Call Of Duty, Fortnite,etc...
		 For integrate this mode open the MainMenu Scene then in Unity Toolbar Go to MFPS -> Addons -> PlayerSelector -> InLobby Integration

- Ready!.

Add an new player option:----------------------------------------------------------------------------------------------------------------------------------------

In order to add a new player as options in your character selection menu you need 2 things:
  - A player prefab located in a "Resources" folder as the default ones of MFPS
  - Sprite preview of Player.

  Player Prefab:
  this player prefab can contain different weapons,textures,size,model,etc..
  important thing is that it need be located in a "Resource" folder for photon can instantiated it.

  them you need add it the list that you want in the bl_Selector script located in the canvas prefab that you instantiate in the scene hierarchy
  Canvas[PlayerSelectore] -> Selector -> bl_PlayerSelector, there are 3 list, one for each team and one for ffa game mode, you can select
  where you want that this new player can be selected, add a new field in the list and drag the prefab from resources folder in the "prefab" var of new field list.
  them add also an sprite image preview for appear in the selection box.

  you have a new player to be select now!.

  any question or problem feel free to post in the forum:
  http://www.lovattostudio.com/forum/index.php

  Change Log:

  1.7.3
  Improve: Now the InMatch mode doesn't require to be integrated in each map scene, the auto integration just need to be run once.
  Fix: Error that happens when one of the selected players is removed from the AllPlayers list.
  Fix: Default MFPS players where assigned in the opposite team.
  Tweaks: To the InMatch selector UI.
  Fix: Health bar stat in the InMatch UI selector always appear empty.

  1.7.2
  Add: Support for multiple bots player models per team.

  1.7.1
  Fix: Problems with the example player prefabs.

  1.7
  Compatibility with MFPS 1.9
  Convert UI to Text Mesh Pro
  Improve: Change from a single coin support to multi coin support.

  1.6
  Fix: InMatch player selector was not interact-able.
  Change: Core implementation.

  1.5.7
  Improve: Add scrollbar to the InLobby player list.

  1.5.6
  Added: Build-in Documentation.
  Fix: Player are able to select locked players without purchased.

  1.5.5 (18/04/2020)
  Compatibility with MFPS 1.7

  1.5 (06/10/2019)
  Add: InLobby Player Selector Menu (Optional), as request of some, now you can integrate player selector menu in lobby in order
       to select the player one time in lobby instead of each time in a match.

  1.4 (30/6/2019)
  Improve: Now players are assigned in a global object.
  Improve: Integrate with Shop system addon.

  1.3.9
  Require MFPS 1.4.5

  1.3.5
Improve: Now when player change of team, the player selection will open again.

  1.3:
Added player information like speed, regeneration speed, health, etc.. it's automatically calculate from the player prefab.
Animated player selection UI.
