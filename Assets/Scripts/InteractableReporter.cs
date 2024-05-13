using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableReporter : MonoBehaviour
{
    public void OnGrabbed()
    {
        GrabManager.Instance.OnGrabbed(this.gameObject);
    }

    public void OnReleased()
    {
        GrabManager.Instance.OnReleased(this.gameObject);
    }
}
