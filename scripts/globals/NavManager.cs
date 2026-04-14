using Godot;
using System;
using System.Collections.Generic;

public partial class NavManager : Node
{
    // Use a C# event instead of Godot signal to avoid interop issues
    public event Action<string> OnScreenChanged;

    public string CurrentScreen { get; private set; } = "";
    public List<string> ScreenStack { get; private set; } = new();

    public void NavigateTo(string screenName)
    {
        if (screenName == CurrentScreen)
            return;

        if (!string.IsNullOrEmpty(CurrentScreen))
            ScreenStack.Add(CurrentScreen);

        CurrentScreen = screenName;
        OnScreenChanged?.Invoke(screenName);
    }

    public void GoBack()
    {
        if (!CanGoBack())
            return;

        string previous = ScreenStack[^1];
        ScreenStack.RemoveAt(ScreenStack.Count - 1);
        CurrentScreen = previous;
        OnScreenChanged?.Invoke(previous);
    }

    public void SetRootScreen(string screenName)
    {
        ScreenStack.Clear();
        CurrentScreen = screenName;
        OnScreenChanged?.Invoke(screenName);
    }

    public bool CanGoBack() => ScreenStack.Count > 0;

    public void ClearStack() => ScreenStack.Clear();
}
