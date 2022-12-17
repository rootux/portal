using UnityEngine;
using System;

/**
 * Thread safe singleton settings
 */
[Serializable]
public sealed class GlobalSettings
{
    public bool isServer;
    public string[] musicFoldersArray;
    public string serverStaticIp;
    public string agoraAppId;
    public string agoraToken;
    public string agoraChannelName;

    [field: NonSerialized()]
    private static readonly GlobalSettings instance = ImportJson();
    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static GlobalSettings() { }
    private GlobalSettings() { }
    public static GlobalSettings Instance { get { return instance; } }
    // The path we’re providing should not contain the .json extension.
    // Because we’re using Resources.Load, it assumes that the path is prefixed by the resources path: Assets/Resources folder.
    private static GlobalSettings ImportJson(string path = "settings")
    {
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        return JsonUtility.FromJson<GlobalSettings>(textAsset.text);
    }
   
}