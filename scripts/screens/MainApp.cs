using Godot;
using System;
using System.Collections.Generic;

public partial class MainApp : Control
{
    private Control _currentScreen;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        var navManager = GetNodeOrNull<NavManager>("/root/NavManager");
        if (navManager != null)
        {
            navManager.OnScreenChanged += OnScreenChanged;
        }

        CallDeferred(nameof(LoadInitialScreen));
    }

    private void LoadInitialScreen()
    {
        var navManager = GetNodeOrNull<NavManager>("/root/NavManager");
        var gameData = GetNodeOrNull<GameData>("/root/GameData");

        string role = "";
        if (gameData?.CurrentUser?.ContainsKey("role") == true)
        {
            var roleVal = gameData.CurrentUser["role"];
            if (roleVal.VariantType != Variant.Type.Nil)
                role = roleVal.ToString();
        }

        if (navManager != null)
        {
            string screen = string.IsNullOrEmpty(role) ? "onboarding" : HomeScreenForRole(role);
            navManager.SetRootScreen(screen);
        }
    }

    private void OnScreenChanged(string screenName)
    {
        if (_currentScreen != null && IsInstanceValid(_currentScreen))
        {
            _currentScreen.QueueFree();
            _currentScreen = null;
        }

        string baseName = screenName;
        string param = "";
        if (screenName.Contains(':'))
        {
            int idx = screenName.IndexOf(':');
            baseName = screenName[..idx];
            param = screenName[(idx + 1)..];
        }

        Control screen = CreateScreen(baseName);
        if (screen == null)
        {
            GD.PushWarning($"MainApp: Could not create screen '{baseName}'");
            return;
        }

        screen.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        // Add to tree FIRST so the screen can access autoloads in Setup/_Ready
        AddChild(screen);
        _currentScreen = screen;

        // Pass parameter AFTER adding to tree
        if (!string.IsNullOrEmpty(param) && screen.HasMethod("Setup"))
        {
            screen.Call("Setup", param);
        }
    }

    /// <summary>
    /// Directly instantiate C# screen classes by name.
    /// This avoids the SetScript/ObjectDisposed issue.
    /// </summary>
    private static Control CreateScreen(string name)
    {
        return name switch
        {
            "onboarding" => new Onboarding(),
            "packer_queue" => new PackerQueue(),
            "rig_detail" => new RigDetail(),
            "pack_confirmation" => new PackConfirmation(),
            "packer_history" => new PackerHistory(),
            "packer_billing" => new PackerBilling(),
            "operator_dashboard" => new OperatorDashboard(),
            "rig_management" => new RigManagement(),
            "rig_profile" => new RigProfile(),
            "alert_center" => new AlertCenter(),
            "skydiver_rig" => new SkydiverRig(),
            "skydiver_tab" => new SkydiverTab(),
            "packer_profile" => new PackerProfile(),
            _ => null,
        };
    }

    private static string HomeScreenForRole(string role)
    {
        return role switch
        {
            "packer" => "packer_queue",
            "operator" => "operator_dashboard",
            "skydiver" => "skydiver_rig",
            _ => "onboarding",
        };
    }
}
