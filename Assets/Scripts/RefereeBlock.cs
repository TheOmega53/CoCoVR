using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefereeBlock : MonoBehaviour
{
    [SerializeField]
    private BuildReferee buildReferee;
    private bool filled;
    private GameObject fillingObject;

    private void OnTriggerStay(Collider other)
    {
        if (!filled)
        {
            if (other.gameObject.tag == "Building Block")
            {
                if (!GrabManager.Instance.grabbedObjects.Contains(other.gameObject))
                {
                    filled = true;
                    fillingObject = other.gameObject;
                    MyBlockFilled();
                }
            }
        }        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == fillingObject)
        {
            filled = false;
            fillingObject = null;
            MyBlockEmptied();
        }        
    }
    private void MyBlockFilled()
    {        
        buildReferee.BlockFilled(this.GetComponent<Collider>());
    }

    private void MyBlockEmptied()
    {
        buildReferee.BlockEmptied(this.GetComponent<Collider>());
    }
}
