using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private List<string> mp3Directories;
    private List<AudioClip> currentPlaylist = new List<AudioClip>();
    private string musicRootPath = "music";
    
    void Start()
    {
        Debug.Log("Loading music from " + Application.dataPath);
        PlayRandomSong();
    }

    void PlayRandomSong()
    {
        var fullPath = GetRandomDirectoryPath();
        if (fullPath == "") return;
        var files = GetAllMp3InDirectory(fullPath);
        var randomFileIndex = Random.Range(1, files.Length - 1);
        StartCoroutine(GetAudioClip(files[randomFileIndex].FullName, currentPlaylist));
    }

    string GetRandomDirectoryPath()
    {
        int directoryToUse = PickRandomDirectory();
        if (directoryToUse != -1) return "";
        string directoryPath = mp3Directories[directoryToUse];
        return Path.Combine(Application.dataPath, musicRootPath, directoryPath);
    }

    int PickRandomDirectory()
    {
        if (mp3Directories.Count <= 0)
        {
            Debug.LogError("Please set mp3 directories names for sound to play");
            return -1;
        }
        return Random.Range(1, mp3Directories.Count - 1);
    }

    FileInfo[] GetAllMp3InDirectory(string path)
    {
        DirectoryInfo dir = new DirectoryInfo(path);
        return dir.GetFiles("*.mp3");
    }

    IEnumerator GetAudioClip(string fullFilePath, List<AudioClip> audioClips)
    {
        UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(fullFilePath, AudioType.MPEG);
        yield return webRequest.SendWebRequest();
        if(webRequest.isNetworkError)
        {
            Debug.LogError(webRequest.error);
        }
        else
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(webRequest);
            clip.name = fullFilePath;
            audioClips.Add(clip);
        }
    }
}
