Thanks for purchase ULocalization add-on for MFPS.

Version 1.2.5

Require: MFPS 1.7++

Get Started:--------------------------------------------------------------------------------------------------------------------------------

- Import the addon package in your MFPS project
- When script compilation is finish go to (Toolbar) MFPS -> Addons -> Localization -> (Click)Enable
- Then MFPS -> Addons -> Localization -> (Click)Integrate.
- Done.

Add New sentence / word:---------------------------------------------------------------------------------------------------------------------

- First you need list the world in the LocalizationData, go to Addons -> Localization -> Resources -> Localization -> click on "Open Editor"
or (Toolbar) MFPS -> Addons -> Localization -> Editor, then in the window that open click in the button "+" that is in the
upper left corner, after you click it you will see that a text field is focus, write a "Key ID" for your new sentence,
the key id should be short and preferred in lowercase for performance.

After you have it, click in the button "✔" and you will see a empty field for each language next to the new key id (horizontal)
there you need to assign the translate sentence for each language.

- Now that you have the sentence you need assign it where you need it.

There are two types of text that you may want to translate: for script and from a static UI component,
the scripts text are all the strings that you assign by script to a UI component or show in OnGUI,
for example: m_MyText.text = "THIS IS MY CUSTOM TEXT", and the other type of text is the one that is static,
that you assign in the editor to the UI Component and never change in runtime, for example a text in the UI Canvas.

- So for Locate a text from script: 
   - Normally you assign the text like this: m_MyText.text = "THIS IS MY CUSTOM TEXT";
     for localize the text you should replace with: m_MyText.text = bl_Localization.Instance.GetText("keyid");
	 where "keyid" = to the string id that you assign in the LocalizationData window, but also for better performance
	 is recommended that you identified the text by they id instead of the key id, so you can use like this:
	 m_MyText.text = bl_Localization.Instance.GetText(12); where 12 = to the index position of the sentence in the 
	 LocalizationData list.
	 
	 if you assign the text frequently like for example in a Update() function is highly recommended that you cache the locate text,
	 how? easy, you create a string for store the reference and get the localized text on Start(), then use the cache string in the Update() function.
	 
- For Locate a UI Text component:	 
    That is more simple, for it you only need add the script 'bl_LocalizationUI.cs' to the Text Component, then 
	select the Key ID from the 'Key' list or click in 'Manual' button to assign the key id manually.
	
Add a new language:---------------------------------------------------------------------------------------------------------------------------------

- Go to Addons -> Localization -> Resources -> Localization -> click on the button "Create new language data"
  After this a new scrip-table object will be focus in the Project View, there edit just the basic info (Language Name, Plural Letter and Icon) also the scrit-able object name
  then add this in the 'Language List' of Localization Data ( Addons -> Localization -> Resources -> Localization -> *)
  Now open the LocalizationData Window (Toolbar) MFPS -> Addons -> Localization -> Editor, and translate all the sentences of your new language.
  That's.

  1.2.5
  -Add: Support for custom TMP fonts per language, to allow use compatible fonts for certain languages as Chinese.
  -Fix: Error that happens when add a new localized text with an empty key.

  1.2.0
  -Translate text of MFPS 1.9
  -Improve: Support for Text Mesh Pro UI elements
  -Improve: Localization Editor Window performance by adding pagination.

  1.1
  Compatibility with MFPS 1.8
  -Improve: Add search key field in the bl_LanguageTexts.cs inspector.

  1.0.9
  -Fix: Elimination and Demolition were not localized.
  -Fix: It was possible to integrate several times.

  1.0.8
  -Add: Commands support, now you can concated strings between <localized></localized> tags to send a localized key in a mixed string (located and no located text).

  1.0.3
  -Added: Italian Language (thanks to Francesco Morganti).
  -Fix: Lobby scene was not marked as dirty after integration.

Contact:
If you have problems or any question, feel free to contact me
http://www.lovattostudio.com/en/select-support/
