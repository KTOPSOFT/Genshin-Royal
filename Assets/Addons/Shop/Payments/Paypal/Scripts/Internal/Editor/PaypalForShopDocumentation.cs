using UnityEngine;
using MFPSEditor;
using UnityEditor;

public class PaypalForShopDocumentation : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/shop/paypal/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
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
     new Steps { Name = "Setup Paypal", StepsLenght = 0, DrawFunctionName = nameof(SetupPaypalDoc) },
     new Steps { Name = "Test Mode", StepsLenght = 0, DrawFunctionName = nameof(TestModeDoc) },
     new Steps { Name = "Redirect URL", StepsLenght = 0, DrawFunctionName = nameof(RedirectURLDoc) },
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
        GUISkin gs = Resources.Load<GUISkin>("content/MFPSEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
            base.m_GUISkin = gs;
        }
    }

    void DrawGetStarted()
    {
        DrawText("<b><size=20>REQUIRE:</size></b>\n\n■ MFPS 1.7++\n■ Shop 1.2.0++\n■ ULogin Pro 1.9++\n■ SSL Certificate in your server.\n\nThis addon allows process real payments using <b>Paypal Checkout API</b>, the implementation doesn't require third-party plugins or library but is required that you set up some things on the server-side and your Paypal account which will be explained in this documentation.\n\nTo use this addon, first, you have to enable it by going to MFPS ➔ Addons ➔ Shop ➔ Paypal ➔ Enable, or simply clicking on the button below.\n");
#if !SHOP_PAYPAL
        if(Buttons.FlowButton("Enable Paypal For Shop"))
        {
            EditorUtils.SetEnabled("SHOP_PAYPAL", true);
        }
#else
          DrawNote("The addon is already enabled.");
#endif
        DrawText("After enable it, open the MainMenu scene and go to the Editor top navigation bar ➔ <i>MFPS ➔ Addons ➔ Shop ➔ Paypal ➔ <b>Integrate</b>.</i>");
        DrawHyperlinkText("Once the addon is enable and Integrated in the MainMenu scene, you only have to set Paypal as the default payment processor in the <link=asset:Assets/Addons/Shop/Resources/ShopData.asset>ShopData</link> ➔ Shop Payment ➔ *Select Paypal*");

        DownArrow();
        DrawText("<b><size=20>How it works?</size></b>\n\n■ Your buyer clicks on the Pay button in game.\n■ The button calls the PayPal API to set up the payment.\n■ The button starts the checkout flow in the platform browser.\n\n<b>Before you begin your Checkout integration, you must set up your development environment.</b>\n");

    }

    void SetupPaypalDoc()
    {
        DrawHyperlinkText("Before you can integrate the Paypal Checkout, you must set up your development environment. After you get a token that lets you access protected REST API resources, you create sandbox accounts to test your payment in-game or editor.\n\n<b><size=20>Get API credentials.</size></b>\n\nYour API credentials are a <b>client ID</b> and <b>secret</b>, which authenticate API requests from your account. While you're developing, use these credentials when you test API calls in our sandbox (test) environment. You get these credentials from a REST API app in the Developer Dashboard.\n\nTo get these credentials:\n\n1.<link=https://www.paypal.com/signin?returnUri=https%3A%2F%2Fdeveloper.paypal.com%2Fdeveloper%2Fapplications> Log in to the Developer Dashboard</link> with your PayPal account.\n\n2. Under the <b>DASHBOARD</b> menu, select <b>My Apps & Credentials</b>.\n\nOn the new page, you will have two tabs Sandbox and Live, only the Live app credentials are required but you can create a Sandbox app to test the payment system with a Paypal test account.");
        DrawServerImage(0);
        DrawSuperText("Under the <b>App Name</b> column, select <b>Default Application</b>, which PayPal creates with a new Developer Dashboard account. Select <b>Create App</b> if you don't see the default app.\n\n<b><size=16>Step result</size></b>\n\nThe Default Application page displays your API credentials, including your <b>client ID</b> and <b>secret</b> you need assign these two in the <?link=asset:Assets/Addons/Shop/Resources/PaypalSettings.asset>PaypalSettings</link> ➔ <b>Client ID</b> and <b>Client Secret</b> respectively or in <b>Sandbox Client ID</b> and <b>Sandbox Client Secret</b> if you copy the credentials from the Sandbox Tab.");
    }

    void TestModeDoc()
    {
        DrawHyperlinkText("<b><size=22>Get sandbox account information</size></b>\n\nUse your sandbox accounts to test purchases without affecting real money. For example, when you initiate a purchase through a sandbox account, PayPal creates a test purchase that simulates a purchase in the live environment.\n\nTo test purchases, you need login information for your personal and business sandbox accounts. To get that account information:\n\n1. <link=https://www.paypal.com/signin?returnUri=https%3A%2F%2Fdeveloper.paypal.com%2Fdeveloper%2Fapplications>Log in to the Developer Dashboard</link> with your PayPal account.\n\n2. In the Developer Dashboard, under <b>SANDBOX</b>, select <b>Accounts</b>.\n\n3. Under <b>Account Name</b>, find your personal sandbox account.\n\n4. Under <b>Manage Accounts</b>, select the <b>(...)</b> button for your personal account.\n\n5. Select <b>View/Edit Account</b> to display your email ID and a system-generated password.\n\n6. Repeat these steps for the business account.\n\n7. In the Unity Editor in the <link=asset:Assets/Addons/Shop/Resources/PaypalSettings.asset>PaypalSettings</link> ➔ Enable the <b>Sandbox Mode</b> toggle.\n\nOnce you have your sandbox login information, use your sandbox to test the result of API calls while you're developing. With your login, you can:\n\n<link=https://sandbox.paypal.com/>Log into the sandbox</link> with your personal sandbox login information to simulate a buyer making a payment.\n\n<link=https://sandbox.paypal.com/>Log into the sandbox</link> with your business sandbox login information to simulate the merchant receiving the payment.\n");
    }

    void RedirectURLDoc()
    {
        DrawHyperlinkText("The Redirect URL is the URL that the user will be automatically redirected to <i>(from the Paypal page)</i> when completing a purchase.\n \nThis should be a simple page that shows to the user that their purchase has been completed and that he can return to the game now to complete the process.\n \nThe add-on includes an example page that you can use, it's located at: <i>Assets->Addons->Shop->Payments->Paypal->Scripts->Server-><color=#33B75AFF>payment-done.php</color></i>, what you have to do is upload that file to your server/host of public access -> get the URL of that file in your host directory and then paste that URL in the <link=asset:Assets/Addons/Shop/Resources/PaypalSettings.asset>Paypal Settings</link> -> Redirect URL.\n \nThat's.");
    }

    [MenuItem("MFPS/Tutorials/Shop Paypal")]
    private static void Open()
    {
        EditorWindow.GetWindow(typeof(PaypalForShopDocumentation));
    }

    [MenuItem("MFPS/Addons/Shop/Paypal/Documentation")]
    private static void Open2()
    {
        EditorWindow.GetWindow(typeof(PaypalForShopDocumentation));
    }
}