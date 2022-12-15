using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using Agora.Util;
using FishNet.Managing.Scened;
using Logger = Agora.Util.Logger;
using UnityEngine.SceneManagement;
using SceneManager = UnityEngine.SceneManagement.SceneManager;
using System.Collections;

public class AgoraVideoManager : MonoBehaviour
{
    private string _appID = "88b1b90c1f914369aae5be3bece1347e";
    private string _token = "007eJxTYBDOqVQ8r7jhiuiGZTJG3x4en+zX9ePZag3jDi/Wvw/snNYpMFhYJBkmWRokG6ZZGpoYm1kmJqaaJqUaJ6Umpxoam5inumlOT24IZGTQCZjNyMgAgSA+G0NBflFJYg4DAwCeSyA4";
    private string _channelName = "portal";
    public Text LogText;
    internal Logger Log;
    internal IRtcEngine RtcEngine = null;
    internal static string _channelToken = "007eJxTYBDOqVQ8r7jhiuiGZTJG3x4en+zX9ePZag3jDi/Wvw/snNYpMFhYJBkmWRokG6ZZGpoYm1kmJqaaJqUaJ6Umpxoam5inumlOT24IZGTQCZjNyMgAgSA+G0NBflFJYg4DAwCeSyA4";
    internal static string _tokenBase = "http://localhost:8080";
    internal CONNECTION_STATE_TYPE _state = CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED;
    int timestamp = 0;


    private void Start()
    {
        StartVideo();
    }

    public void StartVideo()
    {
        if (CheckAppId())
        {
            InitEngine();
            JoinChannel();
        }
    }

    internal void RenewOrJoinToken(string newToken)
    {
        AgoraVideoManager._channelToken = newToken;
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
        return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
    }
    private void InitEngine()
    {
        RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
        UserEventHandler handler = new UserEventHandler(this);
        RtcEngineContext context = new RtcEngineContext(_appID, 0,
            CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
            AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
        /*       SenderOptions options = new SenderOptions();
               // options.codecType = VIDEO_CODEC_TYPE.VIDEO_CODEC_H265;
               RtcEngine.SetExternalVideoSource(true, true, EXTERNAL_VIDEO_SOURCE_TYPE.ENCODED_VIDEO_FRAME, options);
               RtcEngine.SetVideoEncoderConfiguration(new VideoEncoderConfiguration()
               {
                   bitrate = 1130,
                   frameRate = (int)FRAME_RATE.FRAME_RATE_FPS_15,
                   dimensions = new VideoDimensions() { width = Screen.width, height = Screen.height },

                   // Note if your remote user video surface to set to flip Horizontal, then we should flip it before sending
                   mirrorMode = VIDEO_MIRROR_MODE_TYPE.VIDEO_MIRROR_MODE_ENABLED
               }); */
        RtcEngine.Initialize(context);
        RtcEngine.InitEventHandler(handler);
        shareScreen();
    }

    private void JoinChannel()
    {
        RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        RtcEngine.EnableAudio();
        RtcEngine.EnableVideo();

        if (_channelToken.Length == 0)
        {
            StartCoroutine(HelperClass.FetchToken(_tokenBase, _channelName, 0, this.RenewOrJoinToken));
            return;
        }

        RtcEngine.JoinChannel(_channelToken, _channelName, "");
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
            // configure videoSurface
            if (uid == 0)
            {
                videoSurface.SetForUser(uid, channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA_PRIMARY);
            }
            else
            {
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

    IEnumerator shareScreen()
    {
        Debug.Log("Share screen called");
        yield return new WaitForEndOfFrame();
        //Read the Pixels inside the Rectangle
        // mTexture.ReadPixels(mRect, 0, 0);
        //Apply the Pixels read from the rectangle to the texture
        // mTexture.Apply();
        // Get the Raw Texture data from the the from the texture and apply it to an array of bytes
        Texture2D texture = KinectColorCameraTracker.GetColorTexture();
        Debug.Log("Got kinect color texture");
        byte[] bytes = texture.GetRawTextureData();
        Debug.Log(bytes[10]);
        // int size = Marshal.SizeOf(bytes[0]) * bytes.Length;
        // Check to see if there is an engine instance already created
        //if the engine is present
        if (RtcEngine != null)
        {
            Debug.Log("External video frame");
            //Create a new external video frame
            ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
            //Set the buffer type of the video frame
            externalVideoFrame.type = VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
            // Set the video pixel format
            //externalVideoFrame.format = ExternalVideoFrame.VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_BGRA;  // V.2.9.x
            externalVideoFrame.format = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA;  // V.3.x.x
            //apply raw data you are pulling from the rectangle you created earlier to the video frame
            externalVideoFrame.buffer = bytes;
            //Set the width of the video frame (in pixels)
            externalVideoFrame.stride = texture.width;
            //Set the height of the video frame
            externalVideoFrame.height = texture.height;
            //Remove pixels from the sides of the frame
            externalVideoFrame.cropLeft = 10;
            externalVideoFrame.cropTop = 10;
            externalVideoFrame.cropRight = 10;
            externalVideoFrame.cropBottom = 10;
            //Rotate the video frame (0, 90, 180, or 270)
            externalVideoFrame.rotation = 180;
            externalVideoFrame.timestamp = timestamp++;
            //Push the external video frame with the frame we just created
            RtcEngine.PushVideoFrame(externalVideoFrame);
            if (timestamp % 100 == 0)
            {
                Debug.LogWarning("Pushed frame = " + timestamp);
            }

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
        private static VideoSurface MakeImageSurface(string goName)
        {
            GameObject go = new GameObject();

            if (go == null)
            {
                return null;
            }

            go.name = goName;
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
    private readonly AgoraVideoManager _helloVideoTokenAgora;

    internal UserEventHandler(AgoraVideoManager helloVideoTokenAgora)
    {
        _helloVideoTokenAgora = helloVideoTokenAgora;
    }

    public override void OnError(int err, string msg)
    {
        _helloVideoTokenAgora.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
    }

    public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
    {
        base.OnJoinChannelSuccess(connection, elapsed);
        int build = 0;
        _helloVideoTokenAgora.Log.UpdateLog(string.Format("sdk version: ${0}",
            _helloVideoTokenAgora.RtcEngine.GetVersion(ref build)));
        _helloVideoTokenAgora.Log.UpdateLog(
            string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                connection.channelId, connection.localUid, elapsed));
        _helloVideoTokenAgora.Log.UpdateLog(string.Format("New Token: {0}",
            AgoraVideoManager._channelToken));
        // HelperClass.FetchToken(tokenBase, channelName, 0, this.RenewOrJoinToken);
        AgoraVideoManager.MakeVideoView(0);
     }

    public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
    {
        _helloVideoTokenAgora.Log.UpdateLog("OnRejoinChannelSuccess");
    }

    public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
    {
        _helloVideoTokenAgora.Log.UpdateLog("OnLeaveChannel");
        AgoraVideoManager.DestroyVideoView(0);
    }

    public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole,
        CLIENT_ROLE_TYPE newRole)
    {
        _helloVideoTokenAgora.Log.UpdateLog("OnClientRoleChanged");
    }

    public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
    {
        _helloVideoTokenAgora.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid,
            elapsed));
        AgoraVideoManager.MakeVideoView(uid, _helloVideoTokenAgora.GetChannelName());
    }

    public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
    {
        _helloVideoTokenAgora.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
            (int)reason));
        AgoraVideoManager.DestroyVideoView(uid);
    }

    public override void OnTokenPrivilegeWillExpire(RtcConnection connection, string token)
    {
        _helloVideoTokenAgora.StartCoroutine(HelperClass.FetchToken(AgoraVideoManager._tokenBase,
            _helloVideoTokenAgora.GetChannelName(), 0, _helloVideoTokenAgora.RenewOrJoinToken));
    }

    public override void OnConnectionStateChanged(RtcConnection connection, CONNECTION_STATE_TYPE state,
        CONNECTION_CHANGED_REASON_TYPE reason)
    {
        _helloVideoTokenAgora._state = state;
    }

    public override void OnConnectionLost(RtcConnection connection)
    {
        _helloVideoTokenAgora.Log.UpdateLog(string.Format("OnConnectionLost "));
    }
}

#endregion
