# Editor settings

The Core package lets you declare settings for your editor tools, store them outside of your project's assets, and edit them from the *Project Settings* or *Preferences* window.

It works the same way as Unity's `ScriptableSingleton<T>`, but also supports Unity 2019.4, where that class is not available.

## Creating a settings menu

Creating a settings menu takes two scripts: a class that declares the settings, and a class that registers the page displaying them.

These scripts use the editor API, so place them in an `Editor` folder, or in an editor-only assembly.

The following examples create the settings of a demo *Screenshot Tool*:

- **Project settings**, shared by everyone working on the project: the folder where screenshots are saved, and the prefix of their file names.
- **Preferences**, specific to each user: whether to open the output folder after each capture.

### Declaring the settings

Create a class that inherits from `EditorConfig<T>`, where `T` is the class itself, and add the `[EditorConfig]` attribute to define where the settings are stored. Then, declare your settings as serialized fields, like you would in a `ScriptableObject`:

```cs
using UnityEngine;
using SideXP.Core.EditorOnly;

[EditorConfig(EEditorConfigScope.Project)]
public class ScreenshotProjectSettings : EditorConfig<ScreenshotProjectSettings>
{
    [SerializeField]
    [Tooltip("The folder where screenshots are saved, relative to the project's root directory.")]
    private string _outputFolder = "Screenshots";

    [SerializeField]
    [Tooltip("The text added at the beginning of each screenshot's file name.")]
    private string _fileNamePrefix = "Capture_";

    public string OutputFolder => _outputFolder;

    public string FileNamePrefix => _fileNamePrefix;
}
```

```cs
using UnityEngine;
using SideXP.Core.EditorOnly;

[EditorConfig(EEditorConfigScope.Editor)]
public class ScreenshotPreferences : EditorConfig<ScreenshotPreferences>
{
    [SerializeField]
    [Tooltip("If enabled, the output folder is opened after each capture.")]
    private bool _shouldOpenFolderAfterCapture = true;

    public bool ShouldOpenFolderAfterCapture
    {
        get => _shouldOpenFolderAfterCapture;
        set => _shouldOpenFolderAfterCapture = value;
    }
}
```

The scope passed to `[EditorConfig]` defines where the settings are stored, and which window they belong to:

| Scope | Storage | Shared with | Window |
|---|---|---|---|
| `Project` | `ProjectSettings/<Key>.json` | Everyone working on the project (the file can be versioned with the project) | *Project Settings* |
| `User` | `UserSettings/<Key>.json` | Nobody: the settings are specific to the user and the project | *Preferences* |
| `Editor` | `EditorPrefs` registry | All the projects opened by the user on the same machine | *Preferences* |

By default, `<Key>` is the full name of the class (including its namespace). If you rename or move the class later, the stored settings won't be found anymore. To avoid that, you can set an explicit key, which won't change when the class does:

```cs
[EditorConfig(EEditorConfigScope.Project, Key = "ScreenshotTool.ProjectSettings")]
public class ScreenshotProjectSettings : EditorConfig<ScreenshotProjectSettings>
```

> The `[EditorConfig]` attribute is required. Without it, the settings can't be loaded or saved, and an error is logged in the console.

### Registering the page

Create a static class with a method that uses Unity's `[SettingsProvider]` attribute, and return a page created with `EditorConfigSettingsProvider.Create<T>()`:

```cs
using UnityEditor;
using SideXP.Core.EditorOnly;

public static class ScreenshotSettingsProviders
{
    [SettingsProvider]
    private static SettingsProvider RegisterProjectSettings()
    {
        return EditorConfigSettingsProvider.Create<ScreenshotProjectSettings>("Project/Screenshot Tool", SettingsScope.Project);
    }

    [SettingsProvider]
    private static SettingsProvider RegisterPreferences()
    {
        return EditorConfigSettingsProvider.Create<ScreenshotPreferences>("Preferences/Screenshot Tool", SettingsScope.User);
    }
}
```

- The **path** defines where the page appears in the window's tree. By convention, Unity's own pages start with `Project/` in the *Project Settings* window, and `Preferences/` in the *Preferences* window. Use more `/` separators to group several pages under the same parent, as in `Project/My Company/Screenshot Tool`.
- The **settings scope** defines which window displays the page: `SettingsScope.Project` for *Project Settings*, or `SettingsScope.User` for *Preferences*. Match it with the scope of your settings, as described in the table above.
- The **keywords** used to find the page from the window's search field are generated from your settings' field names. You can pass your own as a third parameter instead.

You can now open `Edit > Project Settings > Screenshot Tool` and `Edit > Preferences > Screenshot Tool` to edit these settings.

> The page draws the inspector of your settings class, so you can use property attributes to customize the fields, or even write a `[CustomEditor]` for the class to change the whole layout.

### Using the settings from code

Use the `Instance` property (or its `I` shortcut) to get your settings. They are loaded from their storage the first time you access them:

```cs
using System;
using System.IO;
using UnityEditor;

public static class ScreenshotTool
{
    public static void SaveScreenshot(byte[] pngData)
    {
        string fileName = $"{ScreenshotProjectSettings.I.FileNamePrefix}{DateTime.Now:yyyyMMdd_HHmmss}.png";
        string path = Path.Combine(ScreenshotProjectSettings.I.OutputFolder, fileName);

        Directory.CreateDirectory(ScreenshotProjectSettings.I.OutputFolder);
        File.WriteAllBytes(path, pngData);

        if (ScreenshotPreferences.I.ShouldOpenFolderAfterCapture)
            EditorUtility.RevealInFinder(path);
    }
}
```

The changes made from the settings window are saved right away. The changes made from code are saved automatically when you save the project, before scripts are reloaded, and when the editor quits. Call `Save()` to write them immediately:

```cs
ScreenshotPreferences.I.ShouldOpenFolderAfterCapture = false;
ScreenshotPreferences.I.Save();
```
