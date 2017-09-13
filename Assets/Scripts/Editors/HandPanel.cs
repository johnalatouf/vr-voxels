using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// activate the proper UI elements when the hand gestures towards the camera

public class HandPanel : MonoBehaviour {

    [SerializeField]
    private Transform[] childItems;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // only activate this layer of children
    public void ActivateChildren(bool active)
    {
        for (int i=0; i<childItems.Length; i++)
        {
            if (active)
            {
                childItems[i].gameObject.SetActive(true);
                UpdateCurrentPanel(active, childItems[i]);
            }

            else
            {
                childItems[i].gameObject.SetActive(false);
                UpdateCurrentPanel(active, childItems[i]);
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
