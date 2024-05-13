using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathPlane : MonoBehaviour
{

    [SerializeField] Transform respawnPoint;
    private void OnCollisionEnter(Collision collision)
    {
        collision.gameObject.transform.position = respawnPoint.position;
    }
}
