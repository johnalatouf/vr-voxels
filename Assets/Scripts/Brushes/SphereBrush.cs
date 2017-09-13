using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is used for sphere and rect brushes
// it triggers when the user is trying to scale a brush

public class SphereBrush : MonoBehaviour {

    [SerializeField]
    private GameObject voxelCanvas;
    [SerializeField]
    private VoxelEditor voxelEditor;

    private VoxelCanvasPos brushChunkPos;

    public VoxelCanvasPos BrushChunkPos
    {
        set { brushChunkPos = value; }
        get { return brushChunkPos; }
    }

    private bool trigger;

    public bool Trigger
    {
        set { trigger = value; }
        get { return trigger; }
    }

    private bool trigger_l;

    public bool TriggerL
    {
        set { trigger_l = value; }
        get { return trigger_l; }
    }

    private bool trigger_r;

    public bool TriggerR
    {
        set { trigger_r = value; }
        get { return trigger_r; }
    }

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    
    

    // check when fingertips enter
    void OnTriggerEnter(Collider other)
    {
        trigger = true;

        
        if (((other.name.Equals("bone3")) && other.transform.parent.transform.parent.name.Equals("RigidRoundHand_R(Clone)")) || other.name.Equals("PinchDetector_R"))
        {
            Debug.Log("triggered right hand " + other.name + " " + other.transform.parent.transform.parent.name);
            trigger_r = true;
            //Debug.Log("trigger enter " + other.name);
        }
        if (((other.name.Equals("bone3")) && other.transform.parent.transform.parent.name.Equals("RigidRoundHand_L(Clone)")) || other.name.Equals("PinchDetector_L"))
        {
            trigger_l = true;
        }

    }


    // check when fingertips are staying
    void OnTriggerStay(Collider other)
    {
        trigger = true;


        if (((other.name.Equals("bone3")) && other.transform.parent.transform.parent.name.Equals("RigidRoundHand_R(Clone)")) || other.name.Equals("PinchDetector_R"))
        {
            Debug.Log("triggered right hand " + other.name + " " + other.transform.parent.transform.parent.name);
            trigger_r = true;
        }
        if (((other.name.Equals("bone3")) && other.transform.parent.transform.parent.name.Equals("RigidRoundHand_L(Clone)")) || other.name.Equals("PinchDetector_L"))
        {
            trigger_l = true;
        }

    }

	// check when fingertips are exiting
    void OnTriggerExit(Collider other)
    {
        trigger = false;

        if (((other.name.Equals("bone3")) && other.transform.parent.transform.parent.name.Equals("RigidRoundHand_R(Clone)")) || other.name.Equals("PinchDetector_R"))
        {
            trigger_r = false;
        }
        if (((other.name.Equals("bone3")) && other.transform.parent.transform.parent.name.Equals("RigidRoundHand_L(Clone)")) || other.name.Equals("PinchDetector_L"))
        {
            trigger_l = false;
        }
    }

}
