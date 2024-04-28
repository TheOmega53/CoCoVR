using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(Collider))]
public class PlacementSystem : MonoBehaviour
{
    [SerializeField]
    private GameObject cellIndicator;

    [SerializeField]
    private XRInputModalityManager inputManager;

    [SerializeField]
    private Grid grid;

    private Vector3 leftHandPosition, rightHandPosition;

    [SerializeField]
    private InputActionProperty leftGripAction;
    [SerializeField]
    private InputActionProperty rightGripAction;


    [SerializeField]
    private Color TransparencyColor;

    //Dict of all indicators active on scene. Key is the original object and Value is the indicator for that object.
    private Dictionary<GameObject,GameObject> indicatorDict = new Dictionary<GameObject,GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        GrabManager.OnReleaseEvent += onHandReleased;
    }

    private void OnTriggerStay(Collider other)
    {        
        //if hand is grabbing a soma cube
        if (GrabManager.Instance.grabbedObjects.Count != 0)
        {
            Debug.Log("grabbed not zero");
            if (leftGripAction.action.ReadValue<float>() > 0 || rightGripAction.action.ReadValue<float>() > 0)
            {
                Debug.Log("Grab button is being pressed");
                foreach (var obj in GrabManager.Instance.grabbedObjects)
                {
                    Debug.Log("Grabbed object is same as collider");
                    PlaceIndicator(obj);                                   
                }
            }
        }
    }    

    public void PlaceIndicator(GameObject obj)
    {
        Vector3Int gridposition = grid.WorldToCell(obj.transform.position);
        //show preview according to calculated soma cube position        
        //Create mesh of the obj for indicator and add it to indicator dict
        GameObject indicator;
        if (indicatorDict.ContainsKey(obj))
        {
            indicator = indicatorDict[obj];
        }
        else
        {
            indicator = Instantiate(obj);
            //iterate through the child soma cubes and make them trigger + transparent
            foreach (Transform child in indicator.transform)
            {
                MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    Color defaultColor = meshRenderer.material.color;
                    meshRenderer.material.color = new Vector4(defaultColor.r, defaultColor.g, defaultColor.b, 0);
                }

                BoxCollider collider = child.GetComponent<BoxCollider>();
                if (collider != null) collider.isTrigger = true;

            }
            indicatorDict.Add(obj, indicator);
        }


        // Snap indicator transform to cell position
        indicator.transform.position = grid.CellToWorld(gridposition);


        //Snap indicator rotation to multitudes of 90                    
        Vector3 currentRotation = obj.transform.eulerAngles;
        // Calculate the snapped rotation
        Vector3 snappedRotation = new Vector3(
            SnapRotation(currentRotation.x),
            SnapRotation(currentRotation.y),
            SnapRotation(currentRotation.z)
        );
        // Apply the snapped rotation to the indicator
        indicator.transform.eulerAngles = snappedRotation;
    }

    public int SnapRotation(float value)
    {
        float result = Mathf.Round(value / 90f) * 90f;        
        return (int)result;
    }
    //if hand releases a soma cube, put them unto the prediction

    //Vector3Int gridposition = grid.WorldToCell(leftHandPosition);
    //cellIndicator.transform.position = grid.CellToWorld(gridposition);

    public void onHandReleased(GameObject obj)
    {
        Debug.Log("OnRelease was invoked");
        if(indicatorDict.ContainsKey(obj))
        {
            
                obj.transform.position = indicatorDict[obj].transform.position;
                obj.transform.rotation = indicatorDict[obj].transform.rotation;
                obj.transform.localScale = indicatorDict[obj].transform.localScale;


            //delete indicator object
            GameObject.Destroy(indicatorDict[obj].gameObject);
            //remove the dict entry
            indicatorDict.Remove(obj);
        }
    }

}


