using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

/**
 * Shuffles music folders
 * Pick Next folder
 * Shuffle the files
 * Play them one after the other
 * And Repeat the process
 */
namespace DefaultNamespace {
    public class MusicManager : MonoBehaviour
    {
        [SerializeField] private List<string> mp3Directories;
        private List<AudioClip> currentPlaylist = new();
        private string musicRootPath = "music";
        private AudioSource source;
        private string chosenFolder;
        private FileInfo[] chosenFolderFiles;
        private List<int> filesPlayOrder;
        private int currentChosenFolderIndex = 0;
        private List<int> foldersPlayOrder = new();

        void Start()
        {
            source = GetComponent<AudioSource>();
            Debug.Log("Loading music from " + Application.dataPath);
            InitMusic();
        }

        void InitMusic()
        {
            Debug.Log("Init Music");
            if (mp3Directories.Count <= 0)
            {
                Debug.LogError("Please set mp3 directories names for sound to play");
                return;
            }

            foreach (var dir in mp3Directories)
            {
                var fullDir = chosenFolder = Path.Combine(Application.dataPath, musicRootPath, dir);
                if (!Directory.Exists(fullDir))
                {
                    Debug.LogError("Cant play music - directory " + fullDir + " does not exists");
                    return;
                }
            }

            ShuffleFolders();
            PlayNextFolder();
        }

        void PlayNextFolder()
        {
            PickNextFolder();
            ShuffleSongsInFolder();
            PlayRandomSong();
        }

        private void ShuffleFolders()
        {
            foldersPlayOrder = Enumerable.Range(0, mp3Directories.Count).ToList();
            KnuthShuffleArray(foldersPlayOrder);
        }

        private void PickNextFolder()
        {
            var isFinishedLoopingAllFolder = (currentChosenFolderIndex == mp3Directories.Count);
            if (isFinishedLoopingAllFolder)
            {
                currentChosenFolderIndex = 0;
            }

            var folderIndexToPlay = foldersPlayOrder[currentChosenFolderIndex++];
            chosenFolder = Path.Combine(Application.dataPath, musicRootPath, mp3Directories[folderIndexToPlay]);
            Debug.Log("Chosen " + chosenFolder + " music");
        }

        private void ShuffleSongsInFolder()
        {
            Debug.Log("Playing next random");
            chosenFolderFiles = GetAllMp3InDirectory(chosenFolder);
            filesPlayOrder = Enumerable.Range(0, chosenFolderFiles.Length).ToList();
            KnuthShuffleArray(filesPlayOrder);
        }

        void PlayRandomSong()
        {
            var hasFinishedPlayingAllFilesInFolder = (filesPlayOrder.Count <= 0);
            if (hasFinishedPlayingAllFilesInFolder)
            {
                PlayNextFolder();
                return;
            }

            var fileIndexToPlay = filesPlayOrder[0];
            filesPlayOrder.RemoveAt(0);
            var fileToPlay = chosenFolderFiles[fileIndexToPlay];
            StartCoroutine(PlayAudio(fileToPlay.FullName, fileToPlay.Name, currentPlaylist));
        }

        FileInfo[] GetAllMp3InDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            return dir.GetFiles("*.mp3");
        }

        IEnumerator PlayAudio(string fullFilePath, string fileName, List<AudioClip> audioClips)
        {
            if (!File.Exists(fullFilePath))
            {
                Debug.LogError("File doesnt exist: " + fullFilePath);
                yield break;
            }

            Debug.Log("Loading " + fullFilePath);
            UnityWebRequest webRequest =
                UnityWebRequestMultimedia.GetAudioClip("file://" + fullFilePath, AudioType.MPEG);
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("Cant play music");
                Debug.LogError(webRequest.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(webRequest);
                clip.name = fileName;
                audioClips.Add(clip);
                PlayNext(clip);
            }
        }

        private void PlayNext(AudioClip clip)
        {
            Debug.Log("Playing Music" + clip.name);
            source.clip = clip;
            source.Play();
            // Play another random - upon song finished playing
            Debug.Log("Playing another song in " + clip.length + "Time");
            Invoke(nameof(PlayRandomSong), clip.length);
        }

        void KnuthShuffleArray(List<int> array)
        {
            for (int t = 0; t < array.Count; t++)
            {
                var tmp = array[t];
                var r = Random.Range(t, array.Count);
                array[t] = array[r];
                array[r] = tmp;
            }
        }
    }
}
