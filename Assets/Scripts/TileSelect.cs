using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// the tile selection UI

public class TileSelect : MonoBehaviour {

	// the tile button and canvas
    [SerializeField]
    private Button tileButton;

    [SerializeField]
    private VoxelCanvas voxelCanvas;

    [SerializeField]
    private GameObject CanvasInfo;

    // Use this for initialization
    void Start () {
        //this will give us our tile number
        int count = (int)(1 / voxelCanvas.GetBlock(0, 0, 0).TileSize);
        //int count = 16; // TODO - placeholder
        RectTransform rect = GetComponent<RectTransform>();
        for (int y = 0; y < count; y++)
        {
            for (int x = 0; x < count; x++)
            {
                //Instantiate(tileButton, )
                Button b = Instantiate(tileButton, Vector3.zero, transform.rotation);
                //new Vector3(transform.position.x + x*(1.0f/count)*0.3f, transform.position.y + y * (1.0f / count) * 0.3f, transform.position.z)
                b.transform.SetParent(transform);
                b.transform.localScale = Vector3.one;

                RectTransform brect = b.GetComponent<RectTransform>();
                brect.localPosition = Vector3.zero - new Vector3(rect.sizeDelta.x, -rect.sizeDelta.y, 0.0f) / 2.0f + new Vector3(brect.sizeDelta.x/2.0f + brect.sizeDelta.x*x, (brect.sizeDelta.y / 2.0f + brect.sizeDelta.y * y) * (-1.0f), 0.0f);

                // set the click function to send the draw tile
                int bx = x;
                int by = count - y - 1;
                b.onClick.AddListener(delegate { TileOnClick(bx, by); });
            }
        }

    }

    // Update is called once per frame
    void Update () {
		
	}

    void TileOnClick(int x, int y)
    {
        Debug.Log("You have clicked the button! " + x + ", " + y);
        //voxelCanvas.drawColorX = x;
        //voxelCanvas.drawColorY = y;
        voxelCanvas.DrawColors = new int[2] { x, y };
        CanvasInfo.GetComponent<HUD>().ChangeDisplayCube();
    }
}
