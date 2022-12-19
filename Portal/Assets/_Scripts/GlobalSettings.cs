using UnityEngine;
using System;
using System.IO;

namespace DefaultNamespace
{
    /**
 * Thread safe singleton settings
 */
    [Serializable]
    public sealed class GlobalSettings
    {
        public bool isDebug;
        public bool isServer;
        public string musicPath;
        public string[] musicFoldersArray;
        public string serverStaticIp;
        public string agoraAppId;
        public string agoraTokenBase;
        public uint agoraUserId;
        public string agoraToken;
        public string agoraChannelName;
        public int agoraVideoHeight;
        public int agoraVideoWidth;
        public int agoraVideoFrameRate;
        public int agoraDeviceAudioPlayIndex;

        [field: NonSerialized()] private static readonly GlobalSettings instance = ImportSettingsFile();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static GlobalSettings()
        {
        }

        private GlobalSettings()
        {
        }

        public static GlobalSettings Instance
        {
            get { return instance; }
        }

        // The path we’re providing should not contain the .json extension.
        // Because we’re using Resources.Load, it assumes that the path is prefixed by the resources path: Assets/Resources folder.
        private static GlobalSettings ImportSettingsFile(string filePath = "settings")
        {
            var fullPath = Path.Combine(Application.persistentDataPath, filePath + ".json");
            Debug.Log("Trying to load from : " + fullPath);
            if (File.Exists(fullPath))
            {
                Debug.LogWarning("Found settings file in: " + fullPath);
                string fileAsText = File.ReadAllText(fullPath);
                return JsonUtility.FromJson<GlobalSettings>(fileAsText);
            }
            else
            {
                Debug.LogWarning("Cant find settings file in: " + fullPath);
                Debug.LogWarning("Loading settings from resources");
                return ImportFromResources(filePath);
            }
        }

        private static GlobalSettings ImportFromResources(string filePath)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(filePath);
            Debug.Log(filePath);
            Debug.Log(textAsset);
            
            return JsonUtility.FromJson<GlobalSettings>(textAsset.text);
        }

    }
}