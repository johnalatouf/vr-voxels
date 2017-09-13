using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// use this to carry mesh info between scenes

public class ProgramInfo : MonoBehaviour {

    private static ProgramInfo info;

    //keeps track of the dimensions set
    [SerializeField]
    private int[] voxelCanvasDimensions;

    public int[] VoxelCanvasDimensions
    {
        get { return voxelCanvasDimensions; }
        set { voxelCanvasDimensions = value; }
    }

    // Use this for initialization
    void Start () {
        voxelCanvasDimensions = new int[3] { 16, 16, 16 };

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void Awake()
    {
        //save this info for the next scenes
        DontDestroyOnLoad(transform.gameObject);

        // avoid duplicating this object
        if (info == null)
        {
            info = this;
        }
        else
        {
            DestroyObject(gameObject);
        }
    }
}
