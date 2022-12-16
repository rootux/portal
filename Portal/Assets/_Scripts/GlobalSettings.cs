using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GlobalSettings
{
    public bool isServer;
    public string[] musicFoldersArray;
    public string serverStaticIp;
    // The path we’re providing should not contain the .json extension.
    // Because we’re using Resources.Load, it assumes that the path is prefixed by the resources path: Assets/Resources folder.
    public string path = "settings";
}

/* example
{
  "is_server": "true",
  "music_folders_array": ["folder1", "folder2"],
  "server_static_ip": "192.7.7.2"
}
*/