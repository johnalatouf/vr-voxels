using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class FileOperations : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // start a new canvas
    public void NewCanvas()
    {
        GameObject info = GameObject.FindWithTag("ProgramInfo");
        Destroy(info);
        SceneManager.LoadScene("NewModel", LoadSceneMode.Single);
    }
}
