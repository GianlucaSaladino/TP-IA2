using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidCollider : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            Debug.Log("Comida Cerca");
            //Move(Arrive(other.transform.position));
        }
        else if (other.CompareTag("Ally"))
        {

        }
    }
}
