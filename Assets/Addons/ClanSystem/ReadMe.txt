Thanks for purchase Clan System addon for MFPS 2.0.
Version 2.0.2

Require:

ULogin Pro v2.0.0++
MFPS 1.9++

Get Started:

-After import the package in your Unity MFPS project go to (toolbar) MFPS -> Addons -> ClanSystem -> Enable
wait until script compilation finish and then open the tutorial in (toolbar) MFPS -> Addons -> ClanSystem -> Tutorial
follow the steps to set up the files.
-Add the ClanMenu scene in Build Settings, the scene is located in: Assets -> Addons -> ClanSystem -> Content -> Scene -> ClanMenu

Notes:

For create a clan for default the player must have 15000 coins, you can change the price in 'LoginDataBasePro' -> CreateClanPrice,
LoginDataBasePro is located in the Resources folder of ULogin Pro addon folder.

If you have any problem or error with this asset, don't hesitate in contact us
forum: http://www.lovattostudio.com/forum/index.php

Change Log:

2.0.2
Improved: Add option to allow transfer leadership of the clan to another member.
Improved: Allow some special characters in the clan description ().'_#@!~[]

2.0
Improve: Now the clan menu integrates in the MFPS MainMenu scene instead of load in an independent scene.
Improve: Convert all UGUI to Text Mesh Pro.
Improve: Make clan windows modular in order to allow remove unwanted windows.
Improve: Integrate the new MFPS Coin system.
Improve: Add ClanSettings Scriptable (in Resources folder) with many front-end settings for the clan system.
Add: Clan tag and clan tag color.
Improve: Clan members now gets automatically cleaned from manually delete accounts.

1.1.9
Fix: Wrong table name defined in bl_Clans.php

1.1.8
-Compatibility with ULogin Pro 1.9
-Fix: Admin and Moderators can kick themselves.
-Fix: Players can send invitations to their own just created clan.

1.1.6 
-Compatibility with ULogin Pro 1.8.5

1.1.5 (27/11/19)
-Improve: Now show the correct price for clan creation instead of the fixed 10.000 value.
-Fix: When the player leave a clan and join to another, MyClan window still show the previous team information.
-Fix: Some UI's don't update after some actions like join by invitation or leave a clan.

1.1 (25/7/19)
-Fix: Loading UI never hide after leave the clan.
-Fix: Players have to reload the game in order to send new join requests after leave a clan.
-Fix: Player join request is not removed from the clan panel if the player join by accepting the clan invitation.
-Improve: Now can check the top clan info when click on the TopClan list UI.

1.0.5 (15/6/19)
-Improve: Add option to leave clan in MyClan window.
-Improve: Now empty clans (after all member leave it) will be delete from database.