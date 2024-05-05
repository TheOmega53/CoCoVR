using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Vivox;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Unity.Services.Authentication;
using UnityEngine.Android;
#if AUTH_PACKAGE_PRESENT
using Unity.Services.Authentication;
#endif

public class VoiceManager : MonoBehaviour
{
    public const string LobbyChannelName = "lobbyChannel";


    int m_PermissionAskedCount;

    float _nextUpdate = 0;

    [SerializeField]
    GameObject _localPlayerGameObject;

    private void Start()
    {
        VivoxService.Instance.LoggedIn += OnUserLoggedIn;
        VivoxService.Instance.LoggedOut += OnUserLoggedOut;
    }

    void OnDestroy()
    {
        VivoxService.Instance.LoggedIn -= OnUserLoggedIn;
        VivoxService.Instance.LoggedOut -= OnUserLoggedOut;
    }

    public async Task LoginToVivoxAsync()
    {
        await LoginToVivoxService();
    }

    public async Task JoinPositionalChannelAsync()
    {
        string channelToJoin = LobbyChannelName;
        Channel3DProperties channelProperties = new Channel3DProperties(50, 10, 0.1f, AudioFadeModel.LinearByDistance);
        await VivoxService.Instance.JoinPositionalChannelAsync(channelToJoin, ChatCapability.AudioOnly,channelProperties);
    }
    public async Task InitializeAsync(string playerName)
    {

#if AUTH_PACKAGE_PRESENT
        if (!CheckManualCredentials())
        {
            AuthenticationService.Instance.SwitchProfile(playerName);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }s
#endif

        await VivoxService.Instance.InitializeAsync();
    }

#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
    bool IsAndroid12AndUp()
    {
        // android12VersionCode is hardcoded because it might not be available in all versions of Android SDK
        const int android12VersionCode = 31;
        AndroidJavaClass buildVersionClass = new AndroidJavaClass("android.os.Build$VERSION");
        int buildSdkVersion = buildVersionClass.GetStatic<int>("SDK_INT");

        return buildSdkVersion >= android12VersionCode;
    }

    string GetBluetoothConnectPermissionCode()
    {
        if (IsAndroid12AndUp())
        {
            // UnityEngine.Android.Permission does not contain the BLUETOOTH_CONNECT permission, fetch it from Android
            AndroidJavaClass manifestPermissionClass = new AndroidJavaClass("android.Manifest$permission");
            string permissionCode = manifestPermissionClass.GetStatic<string>("BLUETOOTH_CONNECT");

            return permissionCode;
        }

        return "";
    }
#endif

    bool IsMicPermissionGranted()
    {
        bool isGranted = Permission.HasUserAuthorizedPermission(Permission.Microphone);
#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
        if (IsAndroid12AndUp())
        {
            // On Android 12 and up, we also need to ask for the BLUETOOTH_CONNECT permission for all features to work
            isGranted &= Permission.HasUserAuthorizedPermission(GetBluetoothConnectPermissionCode());
        }
#endif
        return isGranted;
    }

    void AskForPermissions()
    {
        string permissionCode = Permission.Microphone;

#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
        if (m_PermissionAskedCount == 1 && IsAndroid12AndUp())
        {
            permissionCode = GetBluetoothConnectPermissionCode();
        }
#endif
        m_PermissionAskedCount++;
        Permission.RequestUserPermission(permissionCode);
    }

    bool IsPermissionsDenied()
    {
#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
        // On Android 12 and up, we also need to ask for the BLUETOOTH_CONNECT permission
        if (IsAndroid12AndUp())
        {
            return m_PermissionAskedCount == 2;
        }
#endif
        return m_PermissionAskedCount == 1;
    }

    async Task LoginToVivoxService()
    {
        if (IsMicPermissionGranted())
        {
            // The user authorized use of the microphone.
            await LoginToVivox();
        }
        else
        {
            // We do not have the needed permissions.
            // Ask for permissions or proceed without the functionality enabled if they were denied by the user
            if (IsPermissionsDenied())
            {
                m_PermissionAskedCount = 0;
                await LoginToVivox();
            }
            else
            {
                AskForPermissions();
            }
        }
    }

    async Task LoginToVivox()
    {
        LoginOptions options = new LoginOptions();
        var systInfoDeviceName = String.IsNullOrWhiteSpace(SystemInfo.deviceName) == false ? SystemInfo.deviceName : Environment.MachineName;
        string UserDisplayName = Environment.MachineName.Substring(0, Math.Min(30, Environment.MachineName.Length));
        var correctedDisplayName = Regex.Replace(UserDisplayName, "[^a-zA-Z0-9_-]", "");
        string DisplayNameInput = correctedDisplayName.Substring(0, Math.Min(correctedDisplayName.Length, 30));

        options.DisplayName = DisplayNameInput;
        options.EnableTTS = true;
        await VivoxService.Instance.LoginAsync(options);
    }

    void OnUserLoggedIn()
    {        
        Debug.Log("Successfully connected to Vivox");
    }

    void OnUserLoggedOut()
    {
        Debug.Log("Disconnecting from voice channel ");
    }

    private void Update()
    {
        if(VivoxService.Instance.ActiveChannels.Count == 0)
        {
            return;
        } else
        {
            if (Time.time > _nextUpdate)
            {
                VivoxService.Instance.Set3DPosition(_localPlayerGameObject, LobbyChannelName);
                _nextUpdate += 0.5f;
            }
        }
    }
}
