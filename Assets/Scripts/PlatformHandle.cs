using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

public class PlatformHandle : MonoBehaviour {


    [SerializeField]
    private Platform platform;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // catch hand collisions
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("enter: " + other.name + " " + other.transform.parent.name);
        if (other.name.Equals("palm"))
        {
            //pass this info to the platform
            platform.HandlePalmEnter(this.gameObject, other);
            //gameObject.GetComponent<Renderer>().material.color = Color.red;
        }
        if (other.transform.parent.name.Equals("thumb") || other.transform.parent.name.Equals("index"))
        {
            //Debug.Log("enter thumb or index");
            platform.HandlePinchEnter(this.gameObject, other.transform.parent.gameObject);
            //gameObject.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //Debug.Log("enter: " + other.name + " " + other.transform.parent.name);
        if (other.name.Equals("palm"))
        {
            //pass this info to the platform
            platform.HandlePalmEnter(this.gameObject, other);
        }
        if (other.transform.parent.name.Equals("thumb") || other.transform.parent.name.Equals("index"))
        {
            //Debug.Log("enter thumb or index");
            platform.HandlePinchEnter(this.gameObject, other.transform.parent.gameObject);
        }
    }

    //catch hand exits
    private void OnTriggerExit(Collider other)
    {
        //Debug.Log("exit: " + other.name + " " + other.transform.parent.name);
        if (other.name.Equals("palm"))
        {
            //pass this info to the platform
            platform.HandlePalmExit(this.gameObject, other);

        }
        if (other.transform.parent.name.Equals("thumb") || other.transform.parent.name.Equals("index"))
        {
            //Debug.Log("exit thumb or index");
            platform.HandlePinchExit(this.gameObject, other.transform.parent.gameObject);

        }
    }
}
