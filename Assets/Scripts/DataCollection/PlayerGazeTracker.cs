using System.Collections;
using System.Collections.Generic;
//using Unity.Netcode;
using UnityEngine;

public class PlayerGazeTracker : MonoBehaviour
{    

    private float timeSpentLooking = 0f;
    private bool isVisible = false;


    private void OnEnable()
    {
        StartCoroutine(SaveDataCoroutine());
    }

    private void OnDisable()
    {
        StopCoroutine(SaveDataCoroutine());
    }

    private void OnBecameVisible()
    {
        // When the object becomes visible, start tracking time
        Debug.Log("Is Visible!");
        isVisible = true;
    }

    private void OnBecameInvisible()
    {
        // When the object becomes invisible, stop tracking time
        Debug.Log("Is No longer Visible!");
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

    public void CountVisibilityTime()
    {
        if (isVisible)
        {

            timeSpentLooking += Time.deltaTime;
        }
    }
}
