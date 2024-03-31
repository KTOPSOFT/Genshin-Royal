Thanks for purchase Scope Pro asset.
Version 1.3.0

Require MFPS 1.9++

Get Started with MFPS:

- After import the asset in your MFPS project in order to test the scopes you can use the player prefab
  that comes in the asset folder, located in: Assets -> Addons -> ScopePro -> Resources -> MPlayer[ScopePro],
  set this player as one of the players in GameData of MFPS, then play the game and check the Rifle and the Sniper weapon.
  
- Add Scope Pro to a sniper:
  in order to add the sniper scope effect to a new sniper, first drag the prefab 'ScopeProSetup' inside (in hierarchy) of your FPSniper,
  the prefab is located in: Assets -> Addons -> ScopePro -> Prefabs -> ScopeProSetup, 
   
   - In the ScopeProSetup object will be a bl_SniperScopeSetup.cs script attached, in this you need assign the player camera in the "Player Camera" field.
   - Your weapon model should have a Scope/glass separated mesh, you have to drag this mesh to bl_SniperScopeSetup -> Normal Mesh and them click on "Setup Scope".
   - Now in the weapon where bl_Gun.cs script is attached, add the script bl_ScopeProWeapon.cs

   -if you want to change the scope texture, you can change the scope material in the Squad mesh located in: ScopePro -> ScopeCamera -> Texture.

- Add reflex material:
  Create a new material and select the shader MFPS -> Sights -> Red Dot.
  and assign a red dot texture.
  In your sight model, select the glass separated mesh and add the material
  play with the shader properties until you get the desired results.

  Change Log:

  1.3.0
  Fix: Fixed Size mode on red dot materials.
  Compatibility with MFPS 1.9

  1.2.8
  Improve: Add URP shader support, read the documentation to use the URP scope shader.

  1.2.6
  Update the example player prefab to MFPS 1.8

  1.2.5
  -Improve: Optimize and simplify the ScopePro shader.
  -Improve: Now the scope reticle will be fixed on the center of the scope instead of manually place it.
  -Fix: The scope view keep showing when the weapon is reloading.

  -1.2:
  -Improve: Rewrite the Scope Pro system, now is easier to set up and use a custom shader.
  -Added: Build-In documentation.