using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using Unity.Services.Vivox;
using Unity.Services.Authentication;
using static Unity.Collections.AllocatorManager;



public class GameManager : MonoBehaviour
{
    static object m_Lock = new object();
    static GameManager m_Instance;

    [SerializeField]
    private NetworkConnect networkConnect;

    [SerializeField]
    private VoiceManager voiceManager;


    /// <summary>
    /// Access singleton instance through this propriety.
    /// </summary>
    public static GameManager Instance
    {
        get
        {
            lock (m_Lock)
            {
                if (m_Instance == null)
                {
                    // Search for existing instance.
                    m_Instance = (GameManager)FindObjectOfType(typeof(GameManager));

                    // Create new instance if one doesn't already exist.
                    if (m_Instance == null)
                    {
                        // Need to create a new GameObject to attach the singleton to.
                        var singletonObject = new GameObject();
                        m_Instance = singletonObject.AddComponent<GameManager>();
                        singletonObject.name = typeof(GameManager).ToString() + " (Singleton)";
                    }
                }
                // Make instance persistent even if its already in the scene
                DontDestroyOnLoad(m_Instance.gameObject);
                return m_Instance;
            }
        }
    }

    private async void Awake()
    {
        if (m_Instance != this && m_Instance != null)
        {
            Debug.LogWarning(
                "Multiple GameManagers detected in the scene. Only one VivoxVoiceManager can exist at a time. The duplicate VivoxVoiceManager will be destroyed.");
            Destroy(this);
        }

        await networkConnect.InitializeUnityServices();
        await VivoxService.Instance.InitializeAsync();
        await networkConnect.JoinOrCreate();
        await voiceManager.LoginToVivoxAsync();
        await voiceManager.JoinPositionalChannelAsync();
        

    }
}
