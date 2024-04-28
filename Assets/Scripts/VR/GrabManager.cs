using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrabManager : MonoBehaviour
{

    public List<GameObject> grabbedObjects = new List<GameObject>();


    public delegate void OnReleaseAction(GameObject obj);
    // Define the event based on the delegate
    public static event OnReleaseAction OnReleaseEvent;

    private static GrabManager instance;

    // Public property to access the instance
    public static GrabManager Instance
    {
        get
        {
            // If the instance hasn't been set yet, find it in the scene
            if (instance == null)
            {
                instance = FindObjectOfType<GrabManager>();

                // If no instance exists in the scene, create a new GameObject and add the script to it
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("SingletonExample");
                    instance = singletonObject.AddComponent<GrabManager>();
                    DontDestroyOnLoad(singletonObject); // Optional: Don't destroy the singleton between scene loads
                }
            }
            return instance;
        }
    }
    public void OnGrabbed(GameObject obj)
    {
        Debug.Log("grabbing");
        grabbedObjects.Add(obj);
    }


    public void OnReleased(GameObject obj)
    {
        Debug.Log("releasing");
        grabbedObjects.Remove(obj);

        OnReleaseEvent(obj);
    }
}
