using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {

    //items for displaying information
    [SerializeField]
    private GameObject displayCube;

    [SerializeField]
    private Texture currentTexture;

    [SerializeField]
    private VoxelCanvas voxelCanvas;

    [SerializeField]
    private Text currentTool;
    private string curTool;
    public string CurTool
    {
        get { return curTool; }
        set { curTool = value; }
    }

    [SerializeField]
    private Text currentBrush;
    private string curBrush;
    public string CurBrush
    {
        get { return curBrush; }
        set { curBrush = value; }
    }

    [SerializeField]
    private Text auto;
    private string autoString;
    public string AutoString
    {
        get { return autoString; }
        set { autoString = value; }
    }


    
	// Use this for initialization
	void Start () {
        
        ChangeDisplayCube();

        autoString = "Autodraw: On";
        curTool = "Current tool: Pinch Draw";
    }
	
	// Update is called once per frame
	void Update () {
		
	}

	// these update the display text
    public void UpdateCurrentTool(string s)
    {
        curTool = s;
        currentTool.text = curTool;
    }

    public void UpdateCurrentBrush(string s)
    {
        curBrush = s;
        currentBrush.text = curBrush;
    }

    public void UpdateAutoString(string s)
    {
        autoString = s;
        auto.text = autoString;
    }

	// when the user selects a texture tile, this updates the cube in the display
    public void ChangeDisplayCube()
    {
        int[] drawCoord = voxelCanvas.DrawColors;
        float tileSize = voxelCanvas.GetBlock(0, 0, 0).TileSize;

        Mesh mesh = displayCube.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] UVs = new Vector2[vertices.Length];

        for (int i = 0; i < UVs.Length - 3; i += 4)
        {
            UVs[i] = new Vector2(tileSize * drawCoord[0] + tileSize,
                tileSize * drawCoord[1]);
            UVs[i + 1] = new Vector2(tileSize * drawCoord[0] + tileSize,
                tileSize * drawCoord[1] + tileSize);
            UVs[i + 2] = new Vector2(tileSize * drawCoord[0],
                tileSize * drawCoord[1] + tileSize);
            UVs[i + 3] = new Vector2(tileSize * drawCoord[0],
                tileSize * drawCoord[1]);
        }

        displayCube.GetComponent<MeshFilter>().mesh.uv = UVs;
        Debug.Log(drawCoord[0] + " " + drawCoord[1]);
    }
}
