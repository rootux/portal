using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using Agora.Util;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace DefaultNamespace
{
    public class AgoraVideoManager : MonoBehaviour
    {
        private string _appID;
        public static uint _userId;
        private string _channelName;
        internal IRtcEngine RtcEngine = null;
        public static string _channelToken = null;
        internal static string _tokenBase;
        internal CONNECTION_STATE_TYPE _state = CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED;
        private static int frameRate;
        private VideoDimensions videoDimensions;
        public static VideoSurface playerVideo;
        private int _streamId = -1;
        public static string HERE_MESSAGE = "HERE";

        private int agoraDeviceAudioPlayIndex;
        private IAudioDeviceManager _audioDeviceManager;
        private DeviceInfo[] _audioPlaybackDeviceInfos;
        public List<uint> userIds = new();

        private static AgoraVideoManager instance = null;
        private void Awake()
        {
            if (instance == null)
            {
                Debug.Log("Agora video manager - dont destroy");
                instance = this;
                DontDestroyOnLoad(this);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Debug.Log("Agora video already exists - destroy");
                DestroyImmediate(this.gameObject);
            }
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            Debug.Log("OnSceneLoaded" + scene.name);
            if (scene.name == "Video")
            {
                EnableAudioVideo();
            }
            else
            {
                DisableAudioVideo();
            }
        }
        

        private void Start()
        {
            _appID = GlobalSettings.Instance.agoraAppId;
            _channelName = GlobalSettings.Instance.agoraChannelName;
            _tokenBase = GlobalSettings.Instance.agoraTokenBase;
            _userId = GlobalSettings.Instance.agoraUserId;
            frameRate = GlobalSettings.Instance.agoraVideoFrameRate;
            videoDimensions = new(GlobalSettings.Instance.agoraVideoWidth, GlobalSettings.Instance.agoraVideoHeight);
            agoraDeviceAudioPlayIndex = GlobalSettings.Instance.agoraDeviceAudioPlayIndex;
            _channelToken = GlobalSettings.Instance.agoraToken;
            
            if (_userId == 0)
            {
                throw new Exception("Please set user id to something that is not 0");
            }

            SetCanvasScalarResolution();
            StartVideo();
        }

        private void SetCanvasScalarResolution()
        {
            int screenRefWidth = GlobalSettings.Instance.screenRefWidth; 
            int screenRefHeight = GlobalSettings.Instance.screenRefHeight;
            Debug.Log("setting the canvas scalar resolution to: " + screenRefWidth + ":" + screenRefHeight);
            var canvasScaler = GameObject.FindWithTag("VideoCanvas").GetComponent<CanvasScaler>();
            canvasScaler.referenceResolution = new Vector2(screenRefWidth, screenRefHeight);
        }

        public void StartVideo()
        {
            InitEngine();
            CallDeviceManagerApi();
            SetBeautyEffect();
            JoinChannel();
        }

        private void CallDeviceManagerApi()
        {
            GetAudioPlaybackDevice();
            SetCurrentDevice();
            SetCurrentDeviceVolume();
        }

        private void SetBeautyEffect()
        {
            var beautyOptions = new BeautyOptions();
            beautyOptions.lighteningContrastLevel = LIGHTENING_CONTRAST_LEVEL.LIGHTENING_CONTRAST_HIGH;

            beautyOptions.smoothnessLevel = 1.0f;
            beautyOptions.rednessLevel = 0.5f;
            beautyOptions.sharpnessLevel = 0.3f;

            RtcEngine.SetBeautyEffectOptions(true, beautyOptions);
        }

        private void GetAudioPlaybackDevice()
        {
            _audioDeviceManager = RtcEngine.GetAudioDeviceManager();
            _audioPlaybackDeviceInfos = _audioDeviceManager.EnumeratePlaybackDevices();
            Debug.Log(string.Format("AudioPlaybackDevice count: {0}", _audioPlaybackDeviceInfos.Length));
            for (var i = 0; i < _audioPlaybackDeviceInfos.Length; i++)
            {
                Debug.Log(string.Format("AudioPlaybackDevice device index: {0}, name: {1}, id: {2}", i,
                    _audioPlaybackDeviceInfos[i].deviceName, _audioPlaybackDeviceInfos[i].deviceId));
            }
        }

        private void SetCurrentDevice()
        {
            if (_audioDeviceManager != null && _audioPlaybackDeviceInfos.Length > agoraDeviceAudioPlayIndex)
            {
                Debug.Log("Settings audio device to index " + agoraDeviceAudioPlayIndex + " which is " +
                              _audioPlaybackDeviceInfos[agoraDeviceAudioPlayIndex].deviceName);
                _audioDeviceManager.SetPlaybackDevice(_audioPlaybackDeviceInfos[agoraDeviceAudioPlayIndex].deviceId);

            }
        }

        private void SetCurrentDeviceVolume()
        {
            if (_audioDeviceManager != null) _audioDeviceManager.SetRecordingDeviceVolume(100);
            if (_audioDeviceManager != null) _audioDeviceManager.SetPlaybackDeviceVolume(100);
        }

        internal void RenewOrJoinToken(string newToken)
        {
            Debug.Log("Using token " + newToken);
            _channelToken = newToken;
            if (_state == CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED
                || _state == CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED
                || _state == CONNECTION_STATE_TYPE.CONNECTION_STATE_FAILED
               )
            {
                // If we are not connected yet, connect to the channel as normal
                JoinChannel();
            }
            else
            {
                // If we are already connected, we should just update the token
                UpdateToken();
            }
        }

        // Update is called once per frame
        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene(0);
            }
        }

        private void UpdateToken()
        {
            RtcEngine.RenewToken(AgoraVideoManager._channelToken);
        }

        private void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
        }

        public void SendImHereMessage()
        {
            int streamId = CreateDataStreamId();
            if (streamId < 0)
            {
                Debug.LogError("CreateDataStream failed!");
            }
            else
            {
                SendStreamMessage(streamId, HERE_MESSAGE);
            }
        }

        private int CreateDataStreamId()
        {
            if (_streamId == -1)
            {
                var config = new DataStreamConfig();
                config.syncWithAudio = false;
                config.ordered = true;
                var nRet = RtcEngine.CreateDataStream(ref this._streamId, config);
                Debug.Log(string.Format("CreateDataStream: nRet{0}, streamId{1}", nRet, _streamId));
            }

            return _streamId;
        }

        private void SendStreamMessage(int streamId, string message)
        {
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(message);
            var nRet = RtcEngine.SendStreamMessage(streamId, byteArray, Convert.ToUInt32(byteArray.Length));
            Debug.Log("SendStreamMessage :" + nRet);
        }

        public void JoinChannel()
        {
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            Debug.Log(_channelToken);
            if (string.IsNullOrEmpty(_channelToken))
            {
                StartCoroutine(HelperClass.FetchToken(_tokenBase, _channelName, _userId, this.RenewOrJoinToken));
                return;
            }

            VideoEncoderConfiguration config = new VideoEncoderConfiguration();
            config.dimensions = videoDimensions;
            config.frameRate = frameRate;
            config.bitrate = 0;
            config.orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_ADAPTIVE;
            RtcEngine.SetVideoEncoderConfiguration(config);
            RtcEngine.JoinChannel(_channelToken, _channelName, "", _userId);
        }

        public void EnableAudioVideo()
        {
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            MakeVideoView(0);
            // restore existing user videos
            foreach (var uid in userIds)
            {
                MakeVideoView(uid, GetChannelName());
            }
            GameObject.FindWithTag("VideoCanvas").GetComponent<Canvas>().enabled = true;
            SendImHereMessage();
        }
        
        public void DisableAudioVideo()
        {
            RtcEngine.DisableAudio();
            RtcEngine.DisableVideo();
            GameObject.FindWithTag("VideoCanvas").GetComponent<Canvas>().enabled = false;
            GameObject currentVideoCanvas = GameObject.Find("0");
            Destroy(currentVideoCanvas);
            foreach (var uid in userIds)
            {
                GameObject otherUser = GameObject.Find(uid.ToString());
                if (otherUser != null)
                {
                    DestroyVideoView(uid);
                }
            }
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
        }

        internal string GetChannelName()
        {
            return _channelName;
        }

        #region -- Video Render UI Logic ---

        internal static void MakeVideoView(uint uid, string channelId = "")
        {
            GameObject go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                return; // reuse
            }

            // create a GameObject and assign to this new user
            VideoSurface videoSurface = MakeImageSurface(uid.ToString());
            if (!ReferenceEquals(videoSurface, null))
            {
                // configure videoSurface for self or other
                if (uid == 0)
                {
                    playerVideo = videoSurface;
                    videoSurface.SetForUser(uid, channelId);
                }
                else
                {
                    // another person joined - disable our video
                    playerVideo.Enable = false;
                    playerVideo.GetComponent<RawImage>().color = Color.black;
                    videoSurface.SetForUser(uid, channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
                }

                videoSurface.OnTextureSizeModify += (int width, int height) =>
                {
                    float scale = (float)height / (float)width;
                    videoSurface.transform.localScale = new Vector3(-5, 5 * scale, 1);
                    Debug.Log("OnTextureSizeModify: " + width + "  " + height);
                };

                videoSurface.SetEnable(true);
            }
        }

        // VIDEO TYPE 1: 3D Object
        private static VideoSurface MakePlaneSurface(string goName)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);

            if (go == null)
            {
                return null;
            }

            go.name = goName;
            // set up transform
            go.transform.Rotate(-90.0f, 0.0f, 0.0f);
            go.transform.position = Vector3.zero;
            go.transform.localScale = new Vector3(0.25f, 0.5f, .5f);

            // configure videoSurface
            var videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }

        // Video TYPE 2: RawImage
        private static VideoSurface MakeImageSurface(string uid)
        {
            GameObject go = new GameObject();

            if (go == null)
            {
                return null;
            }

            go.name = uid;
            // to be renderered onto
            go.AddComponent<RawImage>();
            // make the object draggable
            go.AddComponent<UIElementDrag>();
            GameObject canvas = GameObject.Find("AgoraCanvas");
            if (canvas != null)
            {
                go.transform.parent = canvas.transform;
                Debug.Log("add video view");
            }
            else
            {
                Debug.Log("Canvas is null video view");
            }

            // set up transform
            go.transform.Rotate(0f, 0.0f, -90.0f);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = new Vector3(3f, 4f, 1f);

            // configure videoSurface
            var videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }

        internal static void DestroyVideoView(uint uid)
        {
            GameObject go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                Object.Destroy(go);
            }
        }

        #endregion
    }

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly AgoraVideoManager _agoraVideoManager;

        internal UserEventHandler(AgoraVideoManager agoraVideoManager)
        {
            _agoraVideoManager = agoraVideoManager;
        }

        public override void OnError(int err, string msg)
        {
            string fullError = string.Format("OnError err: {0}, msg: {1}", err, msg);
            Debug.LogError(fullError);
            if (err == 110 || err == 109)
            {
                // reset token and join channel will trigger a token renews
                AgoraVideoManager._channelToken = null;
                _agoraVideoManager.JoinChannel();
            }
            Debug.Log(fullError);
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            Debug.Log(string.Format("sdk version: ${0}",
                _agoraVideoManager.RtcEngine.GetVersion(ref build)));
            Debug.Log(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                    connection.channelId, connection.localUid, elapsed));
            Debug.Log(string.Format("New Token: {0}",
                AgoraVideoManager._channelToken));
            // HelperClass.FetchToken(tokenBase, channelName, 0, this.RenewOrJoinToken);
            AgoraVideoManager.MakeVideoView(0);
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            Debug.Log("OnLeaveChannel");
            AgoraVideoManager.DestroyVideoView(0);
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole,
            CLIENT_ROLE_TYPE newRole)
        {
            Debug.Log("OnClientRoleChanged");
        }
        
        
        public override void OnStreamMessage(RtcConnection connection, uint remoteUid, int streamId, byte[] data, uint length, ulong sentTs)
        {
            try
            {
                string streamMessage = System.Text.Encoding.Default.GetString(data);
                Debug.Log("Got message from " + remoteUid);
                Debug.Log(streamMessage);
                if (streamMessage == AgoraVideoManager.HERE_MESSAGE)
                {
                    OnHereMessage();
                }
            }
            catch
            {
                Debug.LogError("error while parsing stream message");
                Debug.LogError(System.Text.Encoding.Default.GetString(data));
            }
        }

        private void OnHereMessage()
        {
            var isVideoScene = SceneManager.GetActiveScene().name == "Video";
            if(!isVideoScene) {
                GameObject.FindWithTag("ToastManager").GetComponent<ToastManager>().ShowYouHaveCall();    
            }
        }

        public override void OnStreamMessageError(RtcConnection connection, uint remoteUid, int streamId, int code, int missed, int cached)
        {
            Debug.LogError(string.Format(
                "OnStreamMessageError remoteUid: {0}, streamId: {1}, code: {2}, missed: {3}, cached: {4}", remoteUid,
                streamId, code, missed, cached));
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            if (!_agoraVideoManager.userIds.Contains(uid))
            {
                _agoraVideoManager.userIds.Add(uid);
            }

            Debug.Log(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid,
                elapsed));
            AgoraVideoManager.MakeVideoView(uid, _agoraVideoManager.GetChannelName());
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            if (_agoraVideoManager.userIds.Contains(uid))
            {
                _agoraVideoManager.userIds.Remove(uid);
            }

            Debug.Log(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            AgoraVideoManager.DestroyVideoView(uid);
            var isAnotherUser = (uid != AgoraVideoManager._userId);
            if (isAnotherUser)
            {
                // reactivate our view
                AgoraVideoManager.playerVideo.GetComponent<RawImage>().color = Color.white;
                AgoraVideoManager.playerVideo.Enable = true;
            }
        }

        public override void OnTokenPrivilegeWillExpire(RtcConnection connection, string token)
        {
            _agoraVideoManager.StartCoroutine(HelperClass.FetchToken(AgoraVideoManager._tokenBase,
                _agoraVideoManager.GetChannelName(), 0, _agoraVideoManager.RenewOrJoinToken));
        }

        public override void OnConnectionStateChanged(RtcConnection connection, CONNECTION_STATE_TYPE state,
            CONNECTION_CHANGED_REASON_TYPE reason)
        {
            _agoraVideoManager._state = state;
        }

        public override void OnConnectionLost(RtcConnection connection)
        {
            Debug.Log("OnConnectionLost ");
        }
    }
}

#endregion
