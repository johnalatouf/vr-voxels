using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class NewCanvas : MonoBehaviour {

    //sliders and values
    [SerializeField]
    private Slider sliderX;

    [SerializeField]
    private Slider sliderY;

    [SerializeField]
    private Slider sliderZ;

    [SerializeField]
    private Text valueX, valueY, valueZ;

    [SerializeField]
    private ProgramInfo info;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        valueX.text = (sliderX.value * 16).ToString();
        valueY.text = (sliderY.value * 16).ToString();
        valueZ.text = (sliderZ.value * 16).ToString();
    }

    //start a new voxel canvas
    public void GetStarted()
    {
        info.VoxelCanvasDimensions = new int[3] { (int)sliderX.value, (int)sliderY.value, (int)sliderZ.value };
        SceneManager.LoadScene("VoxelDrawing", LoadSceneMode.Single);
    }
}
