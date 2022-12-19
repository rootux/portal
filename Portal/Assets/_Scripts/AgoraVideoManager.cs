using System;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using Agora.Util;
using Logger = Agora.Util.Logger;
using Object = UnityEngine.Object;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace DefaultNamespace
{
    public class AgoraVideoManager : MonoBehaviour
    {
        private string _appID;
        public static uint _userId;
        private string _token;
        private string _channelName;
        public Text LogText;
        internal Logger Log;
        internal IRtcEngine RtcEngine = null;
        public static string _channelToken = null;
        internal static string _tokenBase;
        internal CONNECTION_STATE_TYPE _state = CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED;
        private static int widthMultiplayer = 16;
        private static int heightMultiplayer = 9;
        private static int frameRate;
        private VideoDimensions videoDimensions;
        public static VideoSurface playerVideo;

        private int agoraDeviceAudioPlayIndex;
        private IAudioDeviceManager _audioDeviceManager;
        private DeviceInfo[] _audioPlaybackDeviceInfos;

        private void Start()
        {
            _appID = GlobalSettings.Instance.agoraAppId;
            _token = GlobalSettings.Instance.agoraToken;
            _channelName = GlobalSettings.Instance.agoraChannelName;
            _tokenBase = GlobalSettings.Instance.agoraTokenBase;
            _userId = GlobalSettings.Instance.agoraUserId;
            frameRate = GlobalSettings.Instance.agoraVideoFrameRate;
            videoDimensions = new (GlobalSettings.Instance.agoraVideoWidth, GlobalSettings.Instance.agoraVideoHeight);
            agoraDeviceAudioPlayIndex = GlobalSettings.Instance.agoraDeviceAudioPlayIndex;
            if (_userId == 0)
            {
                throw new Exception("Please set user id to something that is not 0");
            }
            _channelToken = GlobalSettings.Instance.agoraToken;
            StartVideo();
        }

        public void StartVideo()
        {
            if (CheckAppId())
            {
                InitEngine();
                CallDeviceManagerApi();
                JoinChannel();
            }
        }

        private void CallDeviceManagerApi()
        {
            GetAudioPlaybackDevice();
            SetCurrentDevice();
            SetCurrentDeviceVolume();
        }
        private void GetAudioPlaybackDevice()
        {
            _audioDeviceManager = RtcEngine.GetAudioDeviceManager();
            _audioPlaybackDeviceInfos = _audioDeviceManager.EnumeratePlaybackDevices();
            Log.UpdateLog(string.Format("AudioPlaybackDevice count: {0}", _audioPlaybackDeviceInfos.Length));
            for (var i = 0; i < _audioPlaybackDeviceInfos.Length; i++)
            {
                Log.UpdateLog(string.Format("AudioPlaybackDevice device index: {0}, name: {1}, id: {2}", i,
                    _audioPlaybackDeviceInfos[i].deviceName, _audioPlaybackDeviceInfos[i].deviceId));
            }
        }
        
        private void SetCurrentDevice()
        {
            if (_audioDeviceManager != null && _audioPlaybackDeviceInfos.Length > agoraDeviceAudioPlayIndex)
            {
                Log.UpdateLog("Settings audio device to index " + agoraDeviceAudioPlayIndex + " which is " + _audioPlaybackDeviceInfos[agoraDeviceAudioPlayIndex].deviceName);
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

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10,
                "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }
        
        private void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
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
        RtcEngine.EnableAudio();
        RtcEngine.EnableVideo();
        RtcEngine.JoinChannel(_channelToken, _channelName, "", _userId);
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
            go.transform.Rotate(0f, 0.0f, 180.0f);
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
            _agoraVideoManager.Log.UpdateLog(fullError);
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            _agoraVideoManager.Log.UpdateLog(string.Format("sdk version: ${0}",
                _agoraVideoManager.RtcEngine.GetVersion(ref build)));
            _agoraVideoManager.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                    connection.channelId, connection.localUid, elapsed));
            _agoraVideoManager.Log.UpdateLog(string.Format("New Token: {0}",
                AgoraVideoManager._channelToken));
            // HelperClass.FetchToken(tokenBase, channelName, 0, this.RenewOrJoinToken);
            AgoraVideoManager.MakeVideoView(0);
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _agoraVideoManager.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _agoraVideoManager.Log.UpdateLog("OnLeaveChannel");
            AgoraVideoManager.DestroyVideoView(0);
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole,
            CLIENT_ROLE_TYPE newRole)
        {
            _agoraVideoManager.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _agoraVideoManager.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid,
                elapsed));
            AgoraVideoManager.MakeVideoView(uid, _agoraVideoManager.GetChannelName());
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _agoraVideoManager.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
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
            _agoraVideoManager.Log.UpdateLog(string.Format("OnConnectionLost "));
        }
    }
}

#endregion
