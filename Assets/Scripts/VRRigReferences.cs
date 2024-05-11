using UnityEngine;

public class VRRigReferences : MonoBehaviour
{
    public static VRRigReferences Singleton;
    public Transform root;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Transform headVrTarget;
    public Transform leftHandVrTarget;
    public Transform rightHandVRTarget;

    private void Awake()
    {
        Singleton = this;
    }
}