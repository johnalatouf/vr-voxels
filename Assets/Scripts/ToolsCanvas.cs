using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolsCanvas : MonoBehaviour {

    public GameObject toolsCanvas;

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ShowTools()
    {
        toolsCanvas.SetActive(true);
    }

    public void HideTools()
    {
        toolsCanvas.SetActive(false);
    }
}
