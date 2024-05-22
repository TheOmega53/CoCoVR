using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using Unity.Services.Vivox;
using Unity.Services.Authentication;
using static Unity.Collections.AllocatorManager;
using System;
using UnityEngine.UI;
using Unity.Services.CloudSave;



public class GameManager : MonoBehaviour
{
    static object m_Lock = new object();
    static GameManager m_Instance;

    [SerializeField]
    private NetworkConnect networkConnect;

    [SerializeField]
    private VoiceManager voiceManager;

    [SerializeField]
    public Dictionary<ulong, ClientData> ClientData => networkConnect.ClientData;

    public Dictionary<string, int> profileDict;

    private float sessionStartTime;
    public float sessionTime;    
    public int sessionId;    
    public float sessionPercentage { get; set; }

    public bool coroutineRunning = false;
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

        profileDict = new Dictionary<string, int>();

        DontDestroyOnLoad(this);

        await networkConnect.InitializeUnityServices();
        await VivoxService.Instance.InitializeAsync();

        sessionId = UnityEngine.Random.Range(0, 420);

    }


    public async void ConnectToLobby()
    {
        await networkConnect.Join();
        await voiceManager.LoginToVivoxAsync();
        await voiceManager.JoinPositionalChannelAsync();
    }


    public async void CreateLobby()
    {
        Debug.Log("Creating lobby");
        await networkConnect.Create();
        await voiceManager.LoginToVivoxAsync();
        await voiceManager.JoinPositionalChannelAsync();
    }

    internal void StartGame()
    {
        // Get the current time
        DateTime currentTime = DateTime.UtcNow;

        // Define the Unix epoch
        DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Calculate the elapsed time since the Unix epoch
        TimeSpan elapsedTime = currentTime - epochStart;

        // Convert to a float representing seconds
        float unixTimestamp = (float)elapsedTime.TotalSeconds;

        sessionStartTime = unixTimestamp;
        var saveData = new Dictionary<string, object>{
                  {"StartTimeID"+sessionId, sessionStartTime},
                };
        SaveData(saveData);


        sessionTime = 0f;
        networkConnect.StartGame();
        if (!coroutineRunning)
        {
            StartCoroutine(AutoSaveSessionTime());
        }
    }

    internal void SetCharacter(ulong clientId, int characterId)
    {
        networkConnect.SetCharacter(clientId, characterId);
    }



    // Function that you want to execute every 5 seconds
    public async void SaveData(Dictionary<string,object> saveData)
    {
        //Debug.Log("Saving data");                
        await CloudSaveService.Instance.Data.Player.SaveAsync(saveData);
        //Debug.Log($"Saved data {string.Join(',', saveData)}");
    }

    internal void QuitGame()
    {
        coroutineRunning = false;
        Application.Quit();
    }



    private IEnumerator AutoSaveSessionTime()
    {
        coroutineRunning = true;
        while (true)
        {
            // Wait for 5 seconds
            yield return new WaitForSeconds(1f);

            var saveData = new Dictionary<string, object>{
                  {"sessionTimeOfID"+sessionId, sessionTime},
                };
            SaveData(saveData);


            var saveData2 = new Dictionary<string, object>{
                  {"CompletionRateofID"+sessionId, sessionPercentage},
                };
            SaveData(saveData2);
        }
    }

    private void Update()
    {
        if (networkConnect.gameHasStarted)
        {
                sessionTime += Time.deltaTime;
        }
    }

    internal void SaveCompletionTime()
    {
        var saveData = new Dictionary<string, object>{
                  {"CompletionTimeofID"+sessionId, sessionTime},
                };
        SaveData(saveData);
    }
}
