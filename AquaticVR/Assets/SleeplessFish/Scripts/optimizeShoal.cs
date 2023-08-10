using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class optimizeShoal : MonoBehaviour
{
    //private bool invisible = true;
    //private GameObject player;
    private Entity parentEntity;
    // Use this for initialization
    void Start () {
        parentEntity = transform.parent.gameObject.GetComponent<Entity>();
    }

    void OnBecameInvisible()
    {
        parentEntity.enabled = false;
        
    }
    void OnBecameVisible()
    {
        parentEntity.enabled = true;
    }
}
