using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.OpenXR.Input;


public class BuildReferee : MonoBehaviour
{
    public List<GameObject> GoalParts = new List<GameObject>();

    public List<GameObject> BuildingParts = new List<GameObject>();
    // Start is called before the first frame update

    public float ErrorMargin = 0.1f;

    private void Update()
    {
        CheckIfCorrect();
    }


    public void CheckIfCorrect()
    {
        int corrects = 0;
        if (GoalParts.Count == BuildingParts.Count && GoalParts.Count > 0 && BuildingParts.Count > 0)
        {
            Vector3 Offset = Vector3.zero;
            Vector3 NewOffset;

            for (int i = 0; i < GoalParts.Count; i++)
            {
                if (AreValuesClose(GoalParts[i].transform.rotation.eulerAngles, BuildingParts[i].transform.rotation.eulerAngles, ErrorMargin, Vector3.zero))
                {
                    NewOffset = GoalParts[i].transform.position - BuildingParts[i].transform.position;
                    if (i > 0)
                    {
                        if(AreValuesClose(Offset, NewOffset, ErrorMargin, Vector3.zero))
                        {
                            if (AreValuesClose(GoalParts[i].transform.position, BuildingParts[i].transform.position, ErrorMargin, NewOffset))
                            {
                                Debug.Log("Object " + i + " is in correct position");
                                corrects++;
                                Debug.Log(corrects + " Objects out of " + GoalParts.Count + " are correctly placed");
                            }
                        }                        
                    }
                    else
                    {
                        Debug.Log("ignoring first offset");
                        Offset = NewOffset;
                    }
                    
                }
            }
        }
    }

    public bool AreValuesClose(Vector3 vec1, Vector3 vec2, float ErrorMargin, Vector3 offset)
    {
        vec2 = vec2 + offset;

        if (vec1.x < vec2.x + ErrorMargin && vec1.x > vec2.x - ErrorMargin)
        {
            if (vec1.y < vec2.y + ErrorMargin && vec1.y > vec2.y - ErrorMargin)
            {
                if (vec1.z < vec2.z + ErrorMargin && vec1.z > vec2.z - ErrorMargin)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
