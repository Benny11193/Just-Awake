using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsTriggerCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerExit(Collider other) {
        if(other.tag == "IslandMark")
        {
            Destroy(other.transform.gameObject);
        }
    }
}