using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a simple script for activating the children of attached UI elements

public class ActivateChildren : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // only activate this layer of children
    public void UpdateSingleLayer(bool active)
    {
        foreach (Transform child in transform)
        {
            if (active)
            {
                child.gameObject.SetActive(true);
            }

            else
            {
                child.gameObject.SetActive(false);
            }
        }

    }

    // recursively activate children because otherwise we have to set a ton of things to active
    public void UpdateCurrentPanel(bool active)
    {
        foreach (Transform child in transform)
        {
            if (active)
            {
                child.gameObject.SetActive(true);
                UpdateCurrentPanel(active, child);
            }

            else
            {
                child.gameObject.SetActive(false);
                UpdateCurrentPanel(active, child);
            }
        }

    }

    public void UpdateCurrentPanel(bool active, Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (active)
            {
                child.gameObject.SetActive(true);
                UpdateCurrentPanel(active, child);
            }

            else
            {
                child.gameObject.SetActive(false);
                UpdateCurrentPanel(active, child);
            }
        }

    }
}
