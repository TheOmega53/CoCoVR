using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerGazeTracker : NetworkBehaviour
{    

    private float timeSpentLooking = 0f;
    private bool isVisible = false;


    private void Awake()
    {
        StartCoroutine(SaveDataCoroutine());
    }

    private void OnBecameVisible()
    {
        // When the object becomes visible, start tracking time
        isVisible = true;
    }

    private void OnBecameInvisible()
    {
        // When the object becomes invisible, stop tracking time
        isVisible = false;
    }
    private IEnumerator SaveDataCoroutine()
    {
        while (true)
        {
            // Wait for 5 seconds
            yield return new WaitForSeconds(5f);

            // Call the function that you want to execute every 5 seconds

            var saveData = new Dictionary<string, object>{
                  {"gazeDurationFromSession"+GameManager.Instance.sessionId, timeSpentLooking},
                };
            GameManager.Instance.SaveData(saveData);
        }
    }

    private void Update()
    {
        if (!IsLocalPlayer)
        {
            if (isVisible)
            {
                Debug.Log("Player is visible");

                // If the object is visible, increment the time spent looking
                timeSpentLooking += Time.deltaTime;
            }
        }
    }
}
