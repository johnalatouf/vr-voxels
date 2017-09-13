using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

// All of the drawing and painting happens in this file

public class VoxelEditor : MonoBehaviour {

	//pinch detectors
    [Tooltip("Each pinch detector can draw one voxel")]
    [SerializeField]
    private PinchDetector[] _pinchDetectors;
	
	// canvas and canvas informations
    [SerializeField]
    private GameObject voxelCanvas;
    private Vector3 canvasPosition;
    private Vector3 canvasScale;
	private VoxelCanvas vc;
	
	// save the pinch points on release
    private Vector3[] releaseDrawPoint;

	// finger detection
    [SerializeField]
    private ExtendedFingerDetector[] _extendedFingerDetector;
    [SerializeField]
    private Transform[] tipDetector;
	
	// direction of raycast when painting
    private enum HitDirection { None, Top, Bottom, Forward, Back, Left, Right }
    Block.Direction[] bh = new Block.Direction[] { Block.Direction.up, Block.Direction.up };
    Vector3[] hitPoint = new Vector3[] { Vector3.zero, Vector3.zero };
    private bool leftIndexExtended = false;
    private bool rightIndexExtended = false;

    private Vector3 norm = new Vector3(0.4f, 0.05f, 0.4f);
    private Vector3 brushPoint;

    
	// HUD for updates
    [SerializeField]
    private Canvas HUDCanvas;
    private HUD hud;


    LeapProvider provider;
	
	// state machine for drawing states
    private enum DrawState
    {
        add_auto,
        delete_auto,
        add_release,
        delete_release,
        auto_paint,
        auto_block_paint,
        sel_voxel,
        sel_face,
        desel_voxel,
        desel_face
    }
    private bool autoDraw = true;
    private bool selecting = true;

    private DrawState drawState = DrawState.add_auto;

    //state machine for brushes
    private enum BrushState
    {
        hand,
        sphere,
        rect,
        pen
    }

    private BrushState brushState = BrushState.hand;
    
    //brushes
    //Sphere Auto
    [SerializeField]
    private GameObject[] sphereBrush;
    private Vector3[] sphereBrushInitPos;
    private Vector3[] sphereBrushInitScale;

    //Sphere release
    [SerializeField]
    private GameObject sphereBrushRelease;
    private Vector3 sphereBrushReleaseInitPos;
    private Vector3 sphereBrushReleaseInitScale;

    //Rect Auto
    [SerializeField]
    private GameObject[] rectBrush;
    private Vector3[] rectBrushInitPos;
    private Vector3[] rectBrushInitScale;

    private float lastDistance;
    private bool startSphere = false;

    //Rect release
    [SerializeField]
    private GameObject rectBrushRelease;
    private Vector3 rectBrushReleaseInitPos;
    private Vector3 rectBrushReleaseInitScale;

    private bool startRect = false;

    //Rect brush
    [SerializeField]
    private GameObject[] penBrush;
    //Rect brush
    [SerializeField]
    private GameObject[] penTips;
    private Vector3[] penBrushInitPos;

    private int[] penhold;

    [SerializeField]
    private GameObject platform;

    //[SerializeField]
    //private GameObject[] trackers;

    void Awake()
    {
        if (_pinchDetectors.Length == 0)
        {
            Debug.LogWarning("No pinch detectors were specified.");
        }
    }

    void Start()
    {
		// initializing all of the brush and canvas position info
        vc = voxelCanvas.GetComponent<VoxelCanvas>();
        sphereBrushInitPos = new Vector3[] { Vector3.zero, Vector3.zero };
        sphereBrushInitScale = new Vector3[] { Vector3.zero, Vector3.zero };
        rectBrushInitPos = new Vector3[] { Vector3.zero, Vector3.zero };
        rectBrushInitScale = new Vector3[] { Vector3.zero, Vector3.zero };
        releaseDrawPoint = new Vector3[] { Vector3.zero, Vector3.zero };
        provider = FindObjectOfType<LeapProvider>() as LeapProvider;
        penhold = new int[] { 0, 0 };
        canvasPosition = voxelCanvas.transform.position;
        canvasScale = voxelCanvas.transform.localScale;
        hud = HUDCanvas.GetComponent<HUD>();

        for (int i = 0; i < _pinchDetectors.Length; i++)
        {
            
        }

        // set up the sphere brushes
        for (int i = 0; i < sphereBrush.Length; i++)
        {
            sphereBrushInitPos[i] = sphereBrush[i].transform.localPosition;
            sphereBrushInitScale[i] = sphereBrush[i].transform.localScale;
            Debug.Log("sphere set " + sphereBrushInitPos[i]);
            sphereBrush[i].SetActive(false);
        }
        sphereBrushReleaseInitPos = sphereBrushRelease.transform.position;
        sphereBrushReleaseInitScale = sphereBrushRelease.transform.localScale;
        sphereBrushRelease.SetActive(false);

        // set up the rect brushes
        for (int i = 0; i < rectBrush.Length; i++)
        {
            rectBrushInitPos[i] = rectBrush[i].transform.localPosition;
            rectBrushInitScale[i] = rectBrush[i].transform.localScale;
            Debug.Log("rect set " + rectBrushInitPos[i]);
            rectBrush[i].SetActive(false);
        }
        rectBrushReleaseInitPos = rectBrushRelease.transform.position;
        rectBrushReleaseInitScale = rectBrushRelease.transform.localScale;
        rectBrushRelease.SetActive(false);

        // set up the pen brushes
        for (int i = 0; i < penBrush.Length; i++)
        {
            penBrush[i].SetActive(false);
        }

        DeactivateBrush("sphere");
        DeactivateBrush("rect");
        DeactivateBrush("pen");
    }

    //for checking for selections
    public bool CheckSel(int x, int y, int z)
    {
        if (vc.SelectionListCount < 1)
        {
            return true;
        }
        return vc.VoxelInSelection(x, y, z);
    }

    //for checking for selections
    public bool CheckSel(int x, int y, int z, Block.Direction direction)
    {
        if (vc.SelectionListCount < 1)
        {
            return true;
        }
        return vc.FaceInSelection(x, y, z, direction);
    }


    void Update()
    {
        canvasPosition = voxelCanvas.transform.position;



		// check brush and draw states and run coresponding function
        if (brushState == BrushState.rect)
        {
            RectBrushScaleCheck();
            for (int i = 0; i < rectBrush.Length; i++)
            {
                rectBrush[i].transform.rotation = Quaternion.Euler(0, platform.transform.rotation.eulerAngles.y, 0);
            }
            rectBrushRelease.transform.rotation = Quaternion.Euler(0, platform.transform.rotation.eulerAngles.y, 0);
        }
        if (drawState == DrawState.add_auto)
        {
            if (brushState == BrushState.hand)
            {
                AutoDraw();
            }
            else if (brushState == BrushState.sphere)
            {
                SphereBrushScaleCheck();
                for (int i = 0; i < sphereBrush.Length; i++)
                {
                    sphereBrush[i].SetActive(true);
                }
                SphereBrush();
            }
            else if (brushState == BrushState.rect)
            {
                RectBrushScaleCheck();
                for (int i = 0; i < rectBrush.Length; i++)
                {
                    rectBrush[i].SetActive(true);
                }
                RectBrush();
            }
            else if (brushState == BrushState.pen)
            {
                for (int i = 0; i < penBrush.Length; i++)
                {
                    penBrush[i].SetActive(true);
                }
                PenBrush();
            }
        } else if (drawState == DrawState.delete_auto)
        {
            if (brushState == BrushState.hand)
                AutoDelete();
            else if (brushState == BrushState.sphere)
            {
                SphereBrushScaleCheck();
                for (int i = 0; i < sphereBrush.Length; i++)
                {
                    sphereBrush[i].SetActive(true);
                }
                SphereBrush();
            }
            else if (brushState == BrushState.rect)
            {
                RectBrushScaleCheck();
                for (int i = 0; i < rectBrush.Length; i++)
                {
                    rectBrush[i].SetActive(true);
                }
                RectBrush();
            }
            else if (brushState == BrushState.pen)
            {
                for (int i = 0; i < penBrush.Length; i++)
                {
                    penBrush[i].SetActive(true);
                }
                PenBrush();
            }
        } else if (drawState == DrawState.add_release)
        {
            if (brushState == BrushState.hand)
                ReleaseDraw();
            else if(brushState == BrushState.sphere)
            {
                sphereBrushRelease.SetActive(true);
                SingleSphereBrush();
            }
            else if (brushState == BrushState.rect)
            {
                rectBrushRelease.SetActive(true);
                SingleRectBrush();
            }
            else if (brushState == BrushState.pen)
            {
                for (int i = 0; i < penBrush.Length; i++)
                {
                    penBrush[i].SetActive(true);
                }
                PenBrush();
            }
        }
        else if (drawState == DrawState.delete_release)
        {
            if (brushState == BrushState.hand)
                ReleaseDelete();
            else if (brushState == BrushState.sphere)
            {
                SingleSphereBrush();
            }
            else if (brushState == BrushState.rect)
            {
                SingleRectBrush();
            }
            else if (brushState == BrushState.pen)
            {
                for (int i = 0; i < penBrush.Length; i++)
                {
                    penBrush[i].SetActive(true);
                }
                PenBrush();
            }
        }
        else if (drawState == DrawState.auto_paint)
        {
            if (brushState == BrushState.sphere)
            {
                SphereBrushScaleCheck();
                //SphereBrushPaint();
                for (int i = 0; i < sphereBrush.Length; i++)
                {
                    sphereBrush[i].SetActive(true);
                }
                sphereBrushRelease.SetActive(false);
            }
            else if (brushState == BrushState.rect)
            {
                RectBrushScaleCheck();
                //RectBrushPaint();
                for (int i = 0; i < rectBrush.Length; i++)
                {
                    rectBrush[i].SetActive(true);
                }
                rectBrushRelease.SetActive(false);
            }
        }
        else if(drawState == DrawState.auto_block_paint)
        {
            if (brushState == BrushState.hand)
            {
                SphereBrushScaleCheck();
                AutoBlockPaint();
            }

            if (brushState == BrushState.sphere)
            {
                SphereBrushScaleCheck();
                SphereBrushPaint();
                for (int i = 0; i < sphereBrush.Length; i++)
                {
                    sphereBrush[i].SetActive(true);
                }
                sphereBrushRelease.SetActive(false);
            }
            else if (brushState == BrushState.rect)
            {
                RectBrushScaleCheck();
                RectBrushPaint();
                for (int i = 0; i < rectBrush.Length; i++)
                {
                    rectBrush[i].SetActive(true);
                }
                rectBrushRelease.SetActive(false);
            } else if (brushState == BrushState.pen)
            {
                AutoPenPaint();
            }
        }
        else if (drawState == DrawState.sel_face)
        {
            //FaceSelect();
        }
        else if (drawState == DrawState.sel_voxel)
        {
            VoxelSelect();
        }
        else if (drawState == DrawState.desel_face)
        {
            //FaceDeselect();
        }
        else if (drawState == DrawState.desel_voxel)
        {
            VoxelDeselect();
        }

    }

    private void FixedUpdate()
    {
        if (drawState == DrawState.auto_paint)
        {
            
            if (brushState == BrushState.hand)
            {
                AutoPaint();
            }
            else if (brushState == BrushState.sphere)
            {
                //SphereBrushScaleCheck();
                SphereBrushPaint();
            }
            else if (brushState == BrushState.rect)
            {
                //SphereBrushScaleCheck();
                RectBrushPaint();
            } else if(brushState ==BrushState.pen)
            {
                AutoPenPaintFace();
            }
        }
        else if (drawState == DrawState.sel_face)
        {
            FaceSelect();
        }
        else if (drawState == DrawState.desel_face)
        {
            FaceDeselect();
        }
    }

    // set the draw state
    public void SetState(string state)
    {
        
        switch (state)
        {
            case "add":
                if (autoDraw)
                {
                    drawState = DrawState.add_auto;
                }
                else
                {
                    drawState = DrawState.add_release;
                }
                hud.UpdateCurrentTool("Current tool: Draw");
                for (int i = 0; i < sphereBrush.Length; i++)
                {
                    if (sphereBrush[i] != null && sphereBrush[i].activeInHierarchy)
                        sphereBrush[i].transform.localPosition = sphereBrushInitPos[i];
                }
                break;
            case "delete":
                if (autoDraw)
                {
                    drawState = DrawState.delete_auto;
                }
                else
                {
                    drawState = DrawState.delete_release;
                }
                hud.UpdateCurrentTool("Current tool: Delete");
                for (int i = 0; i < sphereBrush.Length; i++)
                {
                    if (sphereBrush[i].activeInHierarchy)
                        sphereBrush[i].transform.localPosition = sphereBrushInitPos[i];
                }
                break;
            case "paint":
                drawState = DrawState.auto_paint;
                hud.UpdateCurrentTool("Current tool: Paint Side");
                for (int i = 0; i < sphereBrush.Length; i++)
                {
                    sphereBrush[i].transform.position = _extendedFingerDetector[i].transform.position;
                    rectBrush[i].transform.position = _extendedFingerDetector[i].transform.position;
                }
                break;
            case "blockpaint":
                drawState = DrawState.auto_block_paint;
                hud.UpdateCurrentTool("Current tool: Paint Voxel");
                for (int i = 0; i < sphereBrush.Length; i++)
                {
                    if (sphereBrush[i].activeInHierarchy)
                        sphereBrush[i].transform.localPosition = sphereBrushInitPos[i];
                }
                for (int i = 0; i < rectBrush.Length; i++)
                {
                    if (rectBrush[i].activeInHierarchy)
                        rectBrush[i].transform.localPosition = rectBrushInitPos[i];
                }
                break;
            case "sel_voxel":
            case "desel_voxel":
                SetBrush("hand");
                if (selecting)
                {
                    drawState = DrawState.sel_voxel;
                    hud.UpdateCurrentTool("Current tool: Voxel Select");
                }
                else
                {
                    drawState = DrawState.desel_voxel;
                    hud.UpdateCurrentTool("Current tool: Voxel Deselect");
                }
                break;
            case "sel_face":
            case "desel_face":
                SetBrush("hand");
                if (selecting)
                {
                    drawState = DrawState.sel_face;
                    hud.UpdateCurrentTool("Current tool: Face Select");
                }
                else
                {
                    drawState = DrawState.desel_face;
                    hud.UpdateCurrentTool("Current tool: Face Deselect");
                }
                break;
            default:
                drawState = DrawState.add_auto;
                break;
        }

        Debug.Log("Changed draw state to: " + drawState.ToString());

    }

	// shuts off all non-selected brushes
    public void DeactivateBrush(string brush)
    {
        switch (brush)
        {
            case "sphere":
                for (int i = 0; i < sphereBrush.Length; i++)
                {
                    sphereBrush[i].SetActive(false);
                }
                sphereBrushRelease.SetActive(false);
                break;
            case "rect":
                for (int i = 0; i < rectBrush.Length; i++)
                {
                    rectBrush[i].SetActive(false);
                }
                rectBrushRelease.SetActive(false);
                break;
            case "pen":
                for (int i = 0; i < penBrush.Length; i++)
                {
                    penBrush[i].SetActive(false);
                }
                break;
            default:
                break;

        }
    }

	// sets the brush state
    public void SetBrush(string brush)
    {
        switch (brush)
        {
            case "hand":
                if (autoDraw)
                {
                    brushState = BrushState.hand;
                }
                else
                {
                    brushState = BrushState.hand;
                }
                DeactivateBrush("sphere");
                DeactivateBrush("rect");
                DeactivateBrush("pen");
                hud.UpdateCurrentBrush("Current brush: hand");
                break;
            case "sphere":
                if (autoDraw || (drawState == DrawState.auto_block_paint || drawState == DrawState.auto_paint))
                {
                    brushState = BrushState.sphere;
                    for (int i = 0; i < sphereBrush.Length; i++)
                    {
                        sphereBrush[i].SetActive(true);
                        sphereBrush[i].transform.localScale = sphereBrushInitScale[i];
                    }
                    sphereBrushRelease.SetActive(false);
                }
                else if (!(drawState == DrawState.auto_block_paint || drawState == DrawState.auto_paint))
                {
                    brushState = BrushState.sphere;
                    for (int i = 0; i < sphereBrush.Length; i++)
                    {
                        sphereBrush[i].SetActive(false);
                    }
                    sphereBrushRelease.SetActive(true);
                    sphereBrushRelease.transform.localScale = sphereBrushReleaseInitScale;
                }

                DeactivateBrush("rect");
                DeactivateBrush("pen");
                hud.UpdateCurrentBrush("Current brush: sphere");
                break;
            case "rect":
                if (autoDraw || (drawState == DrawState.auto_block_paint || drawState == DrawState.auto_paint))
                {
                    brushState = BrushState.rect;
                    for (int i = 0; i < rectBrush.Length; i++)
                    {
                        rectBrush[i].SetActive(true);
                        rectBrush[i].transform.localScale = rectBrushInitScale[i];
                    }
                    rectBrushRelease.SetActive(false);
                }
                else if (!(drawState == DrawState.auto_block_paint || drawState == DrawState.auto_paint))
                {
                    brushState = BrushState.rect;
                    for (int i = 0; i < rectBrush.Length; i++)
                    {
                        rectBrush[i].SetActive(false);
                    }
                    rectBrushRelease.SetActive(true);
                    rectBrushRelease.transform.localScale = rectBrushReleaseInitScale;
                }
                DeactivateBrush("sphere");
                DeactivateBrush("pen");
                break;

            case "pen":
                brushState = BrushState.pen;
                for (int i = 0; i < penBrush.Length; i++)
                {
                    penBrush[i].SetActive(true);
                }

                DeactivateBrush("sphere");
                DeactivateBrush("rect");
                hud.UpdateCurrentBrush("Current brush: pen");
                break;
            default:
                drawState = DrawState.add_auto;
                break;
        }
    }

    // auto draw is toggled
    public void ToggleAutoDraw()
    {
        string brush = brushState.ToString();
        if (autoDraw)
        {
            autoDraw = false;
            hud.UpdateAutoString("AutoDraw: Off");
        }
        else { 
            autoDraw = true;
            hud.UpdateAutoString("AutoDraw: On");
        }

        if (!(drawState == DrawState.auto_block_paint || drawState == DrawState.auto_paint))
            SetState(drawState.ToString().Split('_')[0]);
        SetBrush("hand");
        SetBrush(brush);
        Debug.Log("Changed draw state to: " + drawState.ToString());
    }

	// selection state
    public void ToggleSelecting()
    {
        if (selecting)
        {
            selecting = false;
        } else
        {
            selecting = true;
        }

        SetState(drawState.ToString());
        SetBrush(brushState.ToString());
        Debug.Log("Changed draw state to: " + drawState.ToString());
    }

    // auto draw
    public void AutoDraw()
    {
        //Debug.Log("The selection list: " + voxelCanvas.GetComponent<VoxelCanvas>().SelectionListCount);
        for (int i = 0; i < _pinchDetectors.Length; i++)
        {
            var detector = _pinchDetectors[i];

            if (detector.DidStartHold)
            {

                Vector3 pinchRelative = voxelCanvas.transform.InverseTransformPoint(detector.Position);
                if (CheckSel((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z))
                {
                    voxelCanvas.GetComponent<VoxelCanvas>().SetBlock((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, new BlockFull());
                    voxelCanvas.GetComponent<VoxelCanvas>().GetBlock((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z).SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawWholeColor((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1]));

                }
            }

            if (detector.DidRelease)
            {
                //Debug.Log("pinching release " + i);
            }

            if (detector.IsHolding)
            {

                Vector3 pinchRelative = voxelCanvas.transform.InverseTransformPoint(detector.Position);
                if (CheckSel((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z))
                {
                    voxelCanvas.GetComponent<VoxelCanvas>().SetBlock((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, new BlockFull());
                    voxelCanvas.GetComponent<VoxelCanvas>().GetBlock((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z).SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawWholeColor((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1]));

                }
            }
        }
    }

    // release draw
    public void ReleaseDraw()
    {

        for (int i = 0; i < _pinchDetectors.Length; i++)
        {
            var detector = _pinchDetectors[i];

            if (detector.IsHolding || detector.DidStartHold)
            {
                releaseDrawPoint[i] = voxelCanvas.transform.InverseTransformPoint(detector.Position);
            }

            if (detector.DidRelease)
            {
                
                //Vector3 pinchRelative = voxelCanvas.transform.InverseTransformPoint(detector.Position);

                voxelCanvas.GetComponent<VoxelCanvas>().SetBlock((int)releaseDrawPoint[i].x, (int)releaseDrawPoint[i].y, (int)releaseDrawPoint[i].z, new BlockFull());
                voxelCanvas.GetComponent<VoxelCanvas>().GetBlock((int)releaseDrawPoint[i].x, (int)releaseDrawPoint[i].y, (int)releaseDrawPoint[i].z).SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawWholeColor((int)releaseDrawPoint[i].x, (int)releaseDrawPoint[i].y, (int)releaseDrawPoint[i].z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1]));
            }
        }
    }

    // auto delete
    public void AutoDelete()
    {

        for (int i = 0; i < _pinchDetectors.Length; i++)
        {
            var detector = _pinchDetectors[i];
            

            if (detector.DidStartHold)
            {

                //Vector3 pinch = (detector.Position - canvasPosition)* 20.0f + canvasPosition;
                Vector3 pinchRelative = voxelCanvas.transform.InverseTransformPoint(detector.Position);
                //Debug.Log("pinching here in the canvas: " + pinch + " " + pinchRelative);
                if (CheckSel((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z))
                {
                    voxelCanvas.GetComponent<VoxelCanvas>().SetBlock((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, new BlockEmpty());
                }
                voxelCanvas.GetComponent<VoxelCanvas>().SetBlock((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, new BlockEmpty());
                //voxelCanvas.GetComponent<VoxelCanvas>().GetBlock((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z).SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawWholeColor((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1]));
            }

            if (detector.DidRelease)
            {
                Debug.Log("pinching release " + i);
            }

            if (detector.IsHolding)
            {

                //Vector3 pinch = (detector.Position - canvasPosition) * 20.0f + canvasPosition;
                Vector3 pinchRelative = voxelCanvas.transform.InverseTransformPoint(detector.Position);
                if (CheckSel((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z))
                {
                    voxelCanvas.GetComponent<VoxelCanvas>().SetBlock((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, new BlockEmpty());
                }
                //voxelCanvas.GetComponent<VoxelCanvas>().GetBlock((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z).SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawWholeColor((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1]));
            }
        }
    }

    // release delete
    public void ReleaseDelete()
    {

        for (int i = 0; i < _pinchDetectors.Length; i++)
        {
            var detector = _pinchDetectors[i];

            if (detector.IsHolding || detector.DidStartHold)
            {
                releaseDrawPoint[i] = voxelCanvas.transform.InverseTransformPoint(detector.Position);
            }

            if (detector.DidRelease)
            {

                //Vector3 pinchRelative = voxelCanvas.transform.InverseTransformPoint(detector.Position);

                voxelCanvas.GetComponent<VoxelCanvas>().SetBlock((int)releaseDrawPoint[i].x, (int)releaseDrawPoint[i].y, (int)releaseDrawPoint[i].z, new BlockEmpty());
            }
        }
    }

    #region Selection

    // select voxels individually
    public void VoxelSelect()
    {

        Vector3 pinchRelative;
        for (int i = 0; i < _pinchDetectors.Length; i++)
        {
            var detector = _pinchDetectors[i];

            if (detector.DidStartHold)
            {
                //fingerposition = detector.HandModel.GetLeapHand().Fingers[1].TipPosition.ToVector3();
                pinchRelative = voxelCanvas.transform.InverseTransformPoint(detector.Position);
                voxelCanvas.GetComponent<VoxelCanvas>().AddVoxelToSelection((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z);
            }

            if (detector.DidRelease)
            {
                //Debug.Log("pinching release " + i);
            }

            if (detector.IsHolding)
            {
                //fingerposition = detector.HandModel.GetLeapHand().Fingers[1].TipPosition.ToVector3();
                pinchRelative = voxelCanvas.transform.InverseTransformPoint(detector.Position);
                voxelCanvas.GetComponent<VoxelCanvas>().AddVoxelToSelection((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z);
            }
        }
    }

    // select voxels individually
    public void VoxelDeselect()
    {
        Vector3 fingerposition;
        Vector3 pinchRelative;
        for (int i = 0; i < _pinchDetectors.Length; i++)
        {
            var detector = _pinchDetectors[i];

            if (detector.DidStartHold)
            {
                fingerposition = detector.HandModel.GetLeapHand().Fingers[1].TipPosition.ToVector3();
                pinchRelative = voxelCanvas.transform.InverseTransformPoint(detector.Position);
                voxelCanvas.GetComponent<VoxelCanvas>().DeselectVoxelToSelection((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z);
            }

            if (detector.DidRelease)
            {
                //Debug.Log("pinching release " + i);
            }

            if (detector.IsHolding)
            {
                fingerposition = detector.HandModel.GetLeapHand().Fingers[1].TipPosition.ToVector3();
                pinchRelative = voxelCanvas.transform.InverseTransformPoint(detector.Position);
                voxelCanvas.GetComponent<VoxelCanvas>().DeselectVoxelToSelection((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z);
            }
        }
    }

    // select faces individually
    public void FaceSelect()
    {
        for (int i = 0; i < _extendedFingerDetector.Length; i++)
        {
            float hitDistance = 0.0f;
            var detector = _extendedFingerDetector[i];
            HitDirection h = ReturnDirection(detector.gameObject.transform.position, voxelCanvas, detector.gameObject.transform.forward, ref hitDistance, ref hitPoint[i]);
            if (detector.isActiveAndEnabled && h != HitDirection.None)
            {

                switch (h)
                {
                    case HitDirection.Top:
                        bh[i] = Block.Direction.up;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Bottom:
                        bh[i] = Block.Direction.down;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Forward:
                        bh[i] = Block.Direction.north;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Right:
                        bh[i] = Block.Direction.east;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Back:
                        bh[i] = Block.Direction.south;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Left:
                        bh[i] = Block.Direction.west;
                        //Debug.Log(bh[i]);
                        break;
                    default:
                        //bh[i] = Block.Direction.up;
                        break;
                }
            }

            if (detector.isActiveAndEnabled)
            {
                Vector3 pinchRelative = voxelCanvas.transform.InverseTransformPoint(tipDetector[i].position);
                Vector3 hitRelative = voxelCanvas.transform.InverseTransformPoint(hitPoint[i]);
                Block blockTouch = voxelCanvas.GetComponent<VoxelCanvas>().GetBlock((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z);
                Block blockHit = voxelCanvas.GetComponent<VoxelCanvas>().GetBlock((int)hitRelative.x, (int)hitRelative.y, (int)hitRelative.z);

                if (blockTouch.IsSolid(bh[i]))
                {

                    voxelCanvas.GetComponent<VoxelCanvas>().AddFaceToSelection((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, bh[i]);
                }
                else if (blockHit.IsSolid(bh[i]))
                {
                    if (hitDistance <= 0.005 && hitDistance > 0)
                    {
                        voxelCanvas.GetComponent<VoxelCanvas>().AddFaceToSelection((int)hitRelative.x, (int)hitRelative.y, (int)hitRelative.z, bh[i]);
                    }
                    //voxelCanvas.GetComponent<VoxelCanvas>().AddFaceToSelection((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, bh[i]);
                }
            }

        }
    }

    // select faces individually
    public void FaceDeselect()
    {
        for (int i = 0; i < _extendedFingerDetector.Length; i++)
        {
            float hitDistance = 0.0f;
            var detector = _extendedFingerDetector[i];
            HitDirection h = ReturnDirection(detector.gameObject.transform.position, voxelCanvas, detector.gameObject.transform.forward, ref hitDistance, ref hitPoint[i]);
            if (detector.isActiveAndEnabled && h != HitDirection.None)
            {

                switch (h)
                {
                    case HitDirection.Top:
                        bh[i] = Block.Direction.up;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Bottom:
                        bh[i] = Block.Direction.down;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Forward:
                        bh[i] = Block.Direction.north;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Right:
                        bh[i] = Block.Direction.east;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Back:
                        bh[i] = Block.Direction.south;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Left:
                        bh[i] = Block.Direction.west;
                        //Debug.Log(bh[i]);
                        break;
                    default:
                        //bh[i] = Block.Direction.up;
                        break;
                }
            }

            if (detector.isActiveAndEnabled)
            {
                Vector3 pinchRelative = voxelCanvas.transform.InverseTransformPoint(tipDetector[i].position);
                Vector3 hitRelative = voxelCanvas.transform.InverseTransformPoint(hitPoint[i]);
                Block blockTouch = voxelCanvas.GetComponent<VoxelCanvas>().GetBlock((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z);
                Block blockHit = voxelCanvas.GetComponent<VoxelCanvas>().GetBlock((int)hitRelative.x, (int)hitRelative.y, (int)hitRelative.z);

                if (blockTouch.IsSolid(bh[i]))
                {

                    voxelCanvas.GetComponent<VoxelCanvas>().DeselectFaceToSelection((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, bh[i]);
                }
                else if (blockHit.IsSolid(bh[i]))
                {
                    if (hitDistance <= 0.005 && hitDistance > 0)
                    {
                        voxelCanvas.GetComponent<VoxelCanvas>().DeselectFaceToSelection((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, bh[i]);
                    }
                    voxelCanvas.GetComponent<VoxelCanvas>().DeselectFaceToSelection((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, bh[i]);
                }
            }

        }
    }

#endregion

    // in paint mode, get side with raycast
    // borrowing this from here http://answers.unity3d.com/questions/339532/how-can-i-detect-which-side-of-a-box-i-collided-wi.html
    private HitDirection ReturnDirection(Vector3 Object, GameObject ObjectHit, Vector3 direction, ref float hitDistance, ref Vector3 hitPoint)
    {

        HitDirection hitDirection = HitDirection.None;
        RaycastHit MyRayHit;
        direction = new Vector3(direction.x, direction.y, direction.z);
        Ray MyRay = new Ray(Object, direction);
        Debug.DrawRay(Object, direction * 20, Color.green);

        if (Physics.Raycast(MyRay, out MyRayHit))
        {
            // on collision, get direction compared to normal
            if (MyRayHit.collider != null && MyRayHit.collider.gameObject.tag != "PlatformHandle")
            {
                hitDistance = MyRayHit.distance;
                Vector3 MyNormal = MyRayHit.normal;
                MyNormal = MyRayHit.transform.TransformDirection(MyNormal);

                Debug.DrawRay(MyRayHit.point, MyRayHit.transform.up * 2, Color.blue);
                Debug.DrawRay(MyRayHit.point, MyRayHit.transform.forward * 2, Color.gray);
                Debug.DrawRay(MyRayHit.point, MyRayHit.transform.forward * 2, Color.red);
                Debug.DrawRay(MyRayHit.point, MyNormal * 20, Color.yellow);

                // change values based on the rotation of the canvas
                if (MyRayHit.collider.gameObject.transform.rotation.eulerAngles.y > 315.0f || MyRayHit.collider.gameObject.transform.rotation.eulerAngles.y <= 45.0f)
                {
                    if (Mathf.Abs(MyNormal.y - MyRayHit.transform.up.y) <= norm.y) { hitDirection = HitDirection.Top; }
                    if (Mathf.Abs(MyNormal.y + MyRayHit.transform.up.y) <= norm.y) { hitDirection = HitDirection.Bottom; }

                    if (Mathf.Abs(MyNormal.x - MyRayHit.transform.right.x) <= norm.x) { hitDirection = HitDirection.Right; }
                    if (Mathf.Abs(MyNormal.x + MyRayHit.transform.right.x) <= norm.x) { hitDirection = HitDirection.Left; }

                    if (Mathf.Abs(MyNormal.z - MyRayHit.transform.forward.z) <= norm.z) { hitDirection = HitDirection.Forward; }
                    if (Mathf.Abs(MyNormal.z + MyRayHit.transform.forward.z) <= norm.z) { hitDirection = HitDirection.Back; }
                }
                else if (MyRayHit.collider.gameObject.transform.rotation.eulerAngles.y > 45.0f || MyRayHit.collider.gameObject.transform.rotation.eulerAngles.y <= 135.0f)
                {

                    if (Mathf.Abs(MyNormal.y - MyRayHit.transform.up.y) <= norm.y) { hitDirection = HitDirection.Top; }
                    if (Mathf.Abs(MyNormal.y + MyRayHit.transform.up.y) <= norm.y) { hitDirection = HitDirection.Bottom; }

                    if (Mathf.Abs(MyNormal.x - MyRayHit.transform.right.x) <= norm.x) { hitDirection = HitDirection.Back; }
                    if (Mathf.Abs(MyNormal.x + MyRayHit.transform.right.x) <= norm.x) { hitDirection = HitDirection.Forward; }

                    if (Mathf.Abs(MyNormal.z - MyRayHit.transform.forward.z) <= norm.z) { hitDirection = HitDirection.Left; }
                    if (Mathf.Abs(MyNormal.z + MyRayHit.transform.forward.z) <= norm.z) { hitDirection = HitDirection.Right; }
                }
                else if (MyRayHit.collider.gameObject.transform.rotation.eulerAngles.y > 135.0f || MyRayHit.collider.gameObject.transform.rotation.eulerAngles.y <= 225.0f)
                {
                    if (Mathf.Abs(MyNormal.y - MyRayHit.transform.up.y) <= norm.y) { hitDirection = HitDirection.Top; }
                    if (Mathf.Abs(MyNormal.y + MyRayHit.transform.up.y) <= norm.y) { hitDirection = HitDirection.Bottom; }

                    if (Mathf.Abs(MyNormal.x - MyRayHit.transform.right.x) <= norm.x) { hitDirection = HitDirection.Left; }
                    if (Mathf.Abs(MyNormal.x + MyRayHit.transform.right.x) <= norm.x) { hitDirection = HitDirection.Right; }

                    if (Mathf.Abs(MyNormal.z - MyRayHit.transform.forward.z) <= norm.z) { hitDirection = HitDirection.Back; }
                    if (Mathf.Abs(MyNormal.z + MyRayHit.transform.forward.z) <= norm.z) { hitDirection = HitDirection.Forward; }
                }
                else if (MyRayHit.collider.gameObject.transform.rotation.eulerAngles.y > 225.0f || MyRayHit.collider.gameObject.transform.rotation.eulerAngles.y <= 315.0f)
                {

                    if (Mathf.Abs(MyNormal.y - MyRayHit.transform.up.y) <= norm.y) { hitDirection = HitDirection.Top; }
                    if (Mathf.Abs(MyNormal.y + MyRayHit.transform.up.y) <= norm.y) { hitDirection = HitDirection.Bottom; }

                    if (Mathf.Abs(MyNormal.x - MyRayHit.transform.right.x) <= norm.x) { hitDirection = HitDirection.Forward; }
                    if (Mathf.Abs(MyNormal.x + MyRayHit.transform.right.x) <= norm.x) { hitDirection = HitDirection.Back; }

                    if (Mathf.Abs(MyNormal.z - MyRayHit.transform.forward.z) <= norm.z) { hitDirection = HitDirection.Right; }
                    if (Mathf.Abs(MyNormal.z + MyRayHit.transform.forward.z) <= norm.z) { hitDirection = HitDirection.Left; }
                }

                Debug.Log("hit: " + MyNormal + " " + MyRayHit.transform.up + " " + MyRayHit.transform.forward + " " + MyRayHit.transform.right + " rotation: " + MyRayHit.collider.gameObject.transform.rotation.eulerAngles);
            }
            
            hitPoint = MyRayHit.point + (MyRayHit.point - Object) * 2*voxelCanvas.transform.lossyScale.x;
        }
        return hitDirection;
    }

    #region Painting
    //auto paint blocks
    public void AutoBlockPaint()
    {
        for (int i = 0; i < _extendedFingerDetector.Length; i++)
        {
            var detector = _extendedFingerDetector[i];
            

            if (detector.isActiveAndEnabled)
            {
                Vector3 pinchRelative = voxelCanvas.transform.InverseTransformPoint(tipDetector[i].position);
                Block blockTouch = voxelCanvas.GetComponent<VoxelCanvas>().GetBlock((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z);
                if (CheckSel((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z) && blockTouch.IsSolid(bh[i]))
                {

                    blockTouch.SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawWholeColor((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1]));

                }
            }

        }
    }

    //auto paint
    public void AutoPaint()
    {
        for (int i = 0; i < _extendedFingerDetector.Length; i++)
        {
            float hitDistance = 0.0f;
            var detector = _extendedFingerDetector[i];
            HitDirection h = ReturnDirection(detector.gameObject.transform.position, voxelCanvas, detector.gameObject.transform.forward, ref hitDistance, ref hitPoint[i]);
            if (detector.isActiveAndEnabled && h != HitDirection.None)
            {

                switch (h)
                {
                    case HitDirection.Top:
                        bh[i] = Block.Direction.up;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Bottom:
                        bh[i] = Block.Direction.down;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Forward:
                        bh[i] = Block.Direction.north;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Right:
                        bh[i] = Block.Direction.east;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Back:
                        bh[i] = Block.Direction.south;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Left:
                        bh[i] = Block.Direction.west;
                        //Debug.Log(bh[i]);
                        break;
                    default:
                        //bh[i] = Block.Direction.up;
                        break;
                }
            }

                if (detector.isActiveAndEnabled)
            {

                Vector3 pinchRelative = voxelCanvas.transform.InverseTransformPoint(tipDetector[i].position);
                Vector3 hitRelative = voxelCanvas.transform.InverseTransformPoint(hitPoint[i]);
                Block blockTouch = voxelCanvas.GetComponent<VoxelCanvas>().GetBlock((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z);
                Block blockHit = voxelCanvas.GetComponent<VoxelCanvas>().GetBlock((int)hitRelative.x, (int)hitRelative.y, (int)hitRelative.z);
                
                if (CheckSel((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, bh[i]) && blockTouch.IsSolid(bh[i]))
                {

                    blockTouch.SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawFaceColor((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1], bh[i]));

                }else if (blockHit.IsSolid(bh[i]))
                {

                    if (CheckSel((int)hitRelative.x, (int)hitRelative.y, (int)hitRelative.z, bh[i]) && hitDistance <= 0.005 && hitDistance > 0)
                    {
                        Debug.Log("Drawing " + bh[i] + " " + (int)hitRelative.x + " " + (int)hitRelative.y + " " + (int)hitRelative.z + " hit distance: " + hitDistance + " " + (int)pinchRelative.x + " " + (int)pinchRelative.y + " " + (int)pinchRelative.z + " " + hitDistance);
                        blockHit.SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawFaceColor((int)hitRelative.x, (int)hitRelative.y, (int)hitRelative.z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1], bh[i]));
                    }
                    blockTouch.SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawFaceColor((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1], bh[i]));

                }
            }
            
        }
    }
    #endregion

    #region Extended bools
    public void LeftIndexExtended(bool extended)
    {
        leftIndexExtended = extended;
    }

    public void RightIndexExtended(bool extended)
    {
        rightIndexExtended = extended;
    }
#endregion

    // when hand active, activate these
    public void activateBrush()
    {
        SetBrush(brushState.ToString());
    }

    #region Sphere Brush

    // set up a sphere draw
    public void SingleSphereBrushScale(Vector3 left, Vector3 right)
    {
        float distance = Vector3.Distance(left, right);
        float change = distance - lastDistance;
        if (sphereBrushRelease.transform.localScale.x > 0.02) { 
            sphereBrushRelease.transform.localScale += new Vector3(change, change, change);
        }
        else
        {
            sphereBrushRelease.transform.localScale += Vector3.one * 0.021f;
        }
        lastDistance = distance;

    }

    // scale the hand spheres
    public void SphereBrushScale(GameObject sphere, Vector3 pinch)
    {
        float offset = 0;
        float distance = Vector3.Distance(sphere.transform.position, pinch);
        float change;
        if (distance - lastDistance < 0)
        {
            offset = -0.0002f;
        } else
        {
            offset = 0.0002f;
        }
        change = distance - lastDistance + offset;
        if (sphere.transform.localScale.x > 0.03f)
        {
            sphere.transform.localScale += new Vector3(change, change, change) * 1.01f;
        } else
        {
            sphere.transform.localScale = Vector3.one * 0.035f;
            sphere.transform.localScale += new Vector3(change, change, change) * 1.01f;
        }
        
        lastDistance = distance;

    }

    //check and scale brushes
    public void SphereBrushScaleCheck()
    {
        if ( sphereBrush[1].GetComponent<SphereBrush>().TriggerL && _pinchDetectors[0].IsPinching && !_pinchDetectors[1].IsPinching) {
            SphereBrushScale(sphereBrush[1], _pinchDetectors[0].Position);
        }
        else if (sphereBrush[0].GetComponent<SphereBrush>().TriggerR && _pinchDetectors[1].IsPinching && !_pinchDetectors[0].IsPinching)
        {
            SphereBrushScale(sphereBrush[0], _pinchDetectors[1].Position);
        }
    }

    public void SingleSphereBrush()
    {

        if ((_pinchDetectors[0].DidStartHold && _pinchDetectors[1].DidStartHold) || (_pinchDetectors[0].IsHolding && _pinchDetectors[1].IsHolding))
        {
            SingleSphereBrushScale(_pinchDetectors[0].Position, _pinchDetectors[1].Position);

            startSphere = true;
        }

        if (startSphere && (_pinchDetectors[0].DidRelease || _pinchDetectors[1].DidRelease))
        {
            DrawInSphere(sphereBrushRelease.transform.position, (sphereBrushRelease.transform.lossyScale.x / 2) / voxelCanvas.transform.lossyScale.x);
            startSphere = false;
        }
    }

    // set up attach sphere brush
    public void SphereBrush()
    {
        // activate the sphere brush
        for(int i=0; i<sphereBrush.Length; i++)
        {
            if (_pinchDetectors[i].IsHolding)
            {
                DrawInSphere(sphereBrush[i].transform.position, (sphereBrush[i].transform.lossyScale.x / 2) / voxelCanvas.transform.lossyScale.x);
            }
        }
    }


    // draw within this sphere
    public void DrawInSphere(Vector3 center, float radius, VoxelCanvasPos pos)
    {
        int xend = pos.x + 16;
        int yend = pos.y + 16;
        int zend = pos.z + 16;
        Vector3 c = voxelCanvas.transform.InverseTransformPoint(center);
        for (int x = (int)pos.x; x < xend; x++)
        {
            for (int y = (int)pos.y; y < yend; y++)
            {
                for (int z = (int)pos.z; z < zend; z++)
                {
                    if (Vector3.Distance(new Vector3(x, y, z), c) < radius)
                    {
                        if (drawState == DrawState.add_auto)
                        {
                            if (CheckSel(x, y, z))
                            {
                                voxelCanvas.GetComponent<VoxelCanvas>().SetBlock(x, y, z, new BlockFull());
                                voxelCanvas.GetComponent<VoxelCanvas>().GetBlock(x, y, z).SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawWholeColor(x, y, z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1]));
                            }
                        } else
                        {
                            if (CheckSel(x, y, z))
                            {
                                voxelCanvas.GetComponent<VoxelCanvas>().SetBlock(x, y, z, new BlockEmpty());
                            }
                        }
                    }
                }
            }
        }
    }

    //drawing accross whole canvas. Less optimal
    public void DrawInSphere(Vector3 center, float radius)
    {
        int xend = voxelCanvas.GetComponent<VoxelCanvas>().VoxelCanvasDimensions[0] * voxelCanvas.GetComponent<VoxelCanvas>().chunkDimension;
        int yend = voxelCanvas.GetComponent<VoxelCanvas>().VoxelCanvasDimensions[1] * voxelCanvas.GetComponent<VoxelCanvas>().chunkDimension;
        int zend = voxelCanvas.GetComponent<VoxelCanvas>().VoxelCanvasDimensions[2] * voxelCanvas.GetComponent<VoxelCanvas>().chunkDimension;
        Vector3 c = voxelCanvas.transform.InverseTransformPoint(center);
        for (int x = 0; x < xend; x++)
        {
            for (int y = 0; y < yend; y++)
            {
                for (int z = 0; z < zend; z++)
                {
                    //Debug.Log(x + " " + y + " " + z + " " + c);
                    if (Vector3.Distance(new Vector3(x, y, z), c) < radius && CheckSel(x, y, z))
                    {
                        //Debug.Log(x + " " + y + " " + z + " " + c);
                        if (drawState == DrawState.add_auto || drawState == DrawState.add_release)
                        {
                            voxelCanvas.GetComponent<VoxelCanvas>().SetBlock(x, y, z, new BlockFull());
                            voxelCanvas.GetComponent<VoxelCanvas>().GetBlock(x, y, z).SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawWholeColor(x, y, z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1]));
                        }
                        else
                        {
                            voxelCanvas.GetComponent<VoxelCanvas>().SetBlock(x, y, z, new BlockEmpty());
                            //voxelCanvas.GetComponent<VoxelCanvas>().GetBlock(x, y, z).SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawWholeColor(x, y, z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1]));
                        }
                    }
                }
            }
        }
    }



    //brush sphere paint
    public void SphereBrushPaint()
    {
        // activate the sphere brush
        for (int i = 0; i < sphereBrush.Length; i++)
        {
            if (drawState == DrawState.auto_paint)
            {
                PaintInSphereFace(sphereBrush[i].transform.position, (sphereBrush[i].transform.lossyScale.x / 2) / voxelCanvas.transform.lossyScale.x);
            } else if (drawState == DrawState.auto_block_paint)
            {
                PaintInSphere(sphereBrush[i].transform.position, (sphereBrush[i].transform.lossyScale.x / 2) / voxelCanvas.transform.lossyScale.x);
            }
        }
    }

	// paint texture inside a sphere
    public void PaintInSphere(Vector3 center, float radius)
    {
        int xend = voxelCanvas.GetComponent<VoxelCanvas>().VoxelCanvasDimensions[0] * voxelCanvas.GetComponent<VoxelCanvas>().chunkDimension;
        int yend = voxelCanvas.GetComponent<VoxelCanvas>().VoxelCanvasDimensions[1] * voxelCanvas.GetComponent<VoxelCanvas>().chunkDimension;
        int zend = voxelCanvas.GetComponent<VoxelCanvas>().VoxelCanvasDimensions[2] * voxelCanvas.GetComponent<VoxelCanvas>().chunkDimension;
        Vector3 c = voxelCanvas.transform.InverseTransformPoint(center);
        for (int x = 0; x < xend; x++)
        {
            for (int y = 0; y < yend; y++)
            {
                for (int z = 0; z < zend; z++)
                {
                    if (Vector3.Distance(new Vector3(x, y, z), c) < radius)
                    {
                        if (CheckSel(x, y, z) && drawState == DrawState.auto_block_paint)
                        {
                            voxelCanvas.GetComponent<VoxelCanvas>().GetBlock(x, y, z).SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawWholeColor(x, y, z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1]));
                        }
                    }
                }
            }
        }
    }


	// only paint a sphere of faces in one direction
    public void PaintInSphereFace(Vector3 center, float radius)
    {
        for (int i = 0; i < _extendedFingerDetector.Length; i++)
        {
            float hitDistance = 0.0f;
            var detector = _extendedFingerDetector[i];
            HitDirection h = ReturnDirection(detector.gameObject.transform.position, voxelCanvas, detector.gameObject.transform.forward, ref hitDistance, ref hitPoint[i]);
            if (detector.isActiveAndEnabled && h != HitDirection.None)
            {

                switch (h)
                {
                    case HitDirection.Top:
                        bh[i] = Block.Direction.up;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Bottom:
                        bh[i] = Block.Direction.down;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Forward:
                        bh[i] = Block.Direction.north;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Right:
                        bh[i] = Block.Direction.east;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Back:
                        bh[i] = Block.Direction.south;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Left:
                        bh[i] = Block.Direction.west;
                        //Debug.Log(bh[i]);
                        break;
                    default:
                        //bh[i] = Block.Direction.up;
                        break;
                }
            }

            int xend = voxelCanvas.GetComponent<VoxelCanvas>().VoxelCanvasDimensions[0] * voxelCanvas.GetComponent<VoxelCanvas>().chunkDimension;
            int yend = voxelCanvas.GetComponent<VoxelCanvas>().VoxelCanvasDimensions[1] * voxelCanvas.GetComponent<VoxelCanvas>().chunkDimension;
            int zend = voxelCanvas.GetComponent<VoxelCanvas>().VoxelCanvasDimensions[2] * voxelCanvas.GetComponent<VoxelCanvas>().chunkDimension;
            Vector3 c = voxelCanvas.transform.InverseTransformPoint(center);
            for (int x = 0; x < xend; x++)
            {
                for (int y = 0; y < yend; y++)
                {
                    for (int z = 0; z < zend; z++)
                    {
                        //Debug.Log(x + " " + y + " " + z + " " + c);
                        if (Vector3.Distance(new Vector3(x, y, z), c) < radius)
                        {
                            //Debug.Log(x + " " + y + " " + z + " " + c);
                            if (CheckSel(x, y, z, bh[i]) && drawState == DrawState.auto_paint)
                            {
                                voxelCanvas.GetComponent<VoxelCanvas>().GetBlock(x, y, z).SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawFaceColor(x, y, z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1], bh[i]));
                            }
                        }
                    }
                }
            }

        }
    }

    #endregion

    #region RectBrush

	// the single cube brush, scaled by both hands
    public void SingleRectBrush()
    {

        if ((_pinchDetectors[0].DidStartHold && _pinchDetectors[1].DidStartHold) || (_pinchDetectors[0].IsHolding && _pinchDetectors[1].IsHolding))
        {
            SingleRectBrushScale(_pinchDetectors[0].Position, _pinchDetectors[1].Position);

            startRect = true;
        }

        if (startRect && (_pinchDetectors[0].DidRelease || _pinchDetectors[1].DidRelease))
        {
            DrawInRect(rectBrushRelease.transform.position, rectBrushRelease.transform.lossyScale.y, rectBrushRelease.transform.lossyScale.x, rectBrushRelease.transform.lossyScale.z);
            startRect = false;
        }
    }

    // set up a rect draw
    public void SingleRectBrushScale(Vector3 left, Vector3 right)
    {
        rectBrushRelease.transform.rotation = platform.transform.rotation;
        float distance = Vector3.Distance(left, right);
        float change = distance - lastDistance;
        rectBrushRelease.transform.localScale += new Vector3(change, change, change);
        lastDistance = distance;

    }

    // scale the hand rects
    public void RectBrushScale(GameObject rect, Vector3 pinch)
    {
        
        float distance = Vector3.Distance(rect.transform.position, pinch);
        float change;
        change = distance - lastDistance;
        if (rect.transform.localScale.x > 0.02)
        {
            rect.transform.localScale += new Vector3(change, change, change);
        } else
        {
            rect.transform.localScale += Vector3.one * 0.021f;
        }

        lastDistance = distance;

    }


    // set up attach sphere brush
    public void RectBrush()
    {
        // activate the sphere brush
        for (int i = 0; i < rectBrush.Length; i++)
        {
            rectBrush[i].transform.rotation = platform.transform.rotation;
            if (_pinchDetectors[i].IsHolding)
            {

                DrawInRect(rectBrush[i].transform.position, rectBrush[i].transform.lossyScale.y, rectBrush[i].transform.lossyScale.x, rectBrush[i].transform.lossyScale.z);
            }
        }
    }


    //rect brush draw
    public void DrawInRect(Vector3 brush, float height, float width, float depth)
    {
        Vector3 brushrel = voxelCanvas.transform.InverseTransformPoint(brush);
        int[] pos = new int[] { (int)brushrel.x, (int)brushrel.y, (int)brushrel.z };

        int halfx = (int)(0.5f * width / voxelCanvas.transform.lossyScale.x);
        int halfy = (int)(0.5f * height / voxelCanvas.transform.lossyScale.y);
        int halfz = (int)(0.5f * depth / voxelCanvas.transform.lossyScale.z);

        int xend = (int)(width / voxelCanvas.transform.lossyScale.x) - halfx;
        int yend = (int)(height / voxelCanvas.transform.lossyScale.y) - halfy;
        int zend = (int)(depth / voxelCanvas.transform.lossyScale.z) - halfz;

        

        Debug.Log("widths: " + width + " " + xend + " " + pos[0]);
        for (int x = pos[0]-halfx; x < pos[0] + xend; x++)
        {
            for (int y = pos[1]-halfy; y < pos[1] + yend; y++)
            {
                for (int z = pos[2]-halfz; z < pos[2] + zend; z++)
                {
                    if (CheckSel(x, y, z) && (drawState == DrawState.add_auto || drawState == DrawState.add_release))
                    {
                        vc.SetBlock(x, y, z, new BlockFull());
                        vc.GetBlock(x, y, z).SetTiles(vc.DrawWholeColor(x, y, z, vc.DrawColors[0], vc.DrawColors[1]));
                    } else if (CheckSel(x, y, z))
                    {
                        vc.SetBlock(x, y, z, new BlockEmpty());
                    }
                }
            }
        }


    }

    //check and scale brushes
    public void RectBrushScaleCheck()
    {
        if (rectBrush[1].GetComponent<SphereBrush>().TriggerL && _pinchDetectors[0].IsPinching && !_pinchDetectors[1].IsPinching)
        {
            RectBrushScale(rectBrush[1], _pinchDetectors[0].Position);
        }
        else if (rectBrush[0].GetComponent<SphereBrush>().TriggerR && _pinchDetectors[1].IsPinching && !_pinchDetectors[0].IsPinching)
        {
            RectBrushScale(rectBrush[0], _pinchDetectors[1].Position);
        }
    }


    //brush rect paint
    public void RectBrushPaint()
    {
        // activate the rect brush
        for (int i = 0; i < rectBrush.Length; i++)
        {
            if (drawState == DrawState.auto_paint)
            {
                PaintInRectFace(rectBrush[i].transform.position, rectBrush[i].transform.lossyScale.y, rectBrush[i].transform.lossyScale.x, rectBrush[i].transform.lossyScale.z);
            }
            else if (drawState == DrawState.auto_block_paint)
            {
                PaintInRect(rectBrush[i].transform.position, rectBrush[i].transform.lossyScale.y, rectBrush[i].transform.lossyScale.x, rectBrush[i].transform.lossyScale.z);
            }
        }
    }

	// paints textures inside a rectangle
    public void PaintInRect(Vector3 brush, float height, float width, float depth)
    {

        Vector3 brushrel = voxelCanvas.transform.InverseTransformPoint(brush);
        int[] pos = new int[] { (int)brushrel.x, (int)brushrel.y, (int)brushrel.z };

        int xend = (int)(width / voxelCanvas.transform.lossyScale.x + 1) - (int)(0.5f * width / voxelCanvas.transform.lossyScale.x);
        int yend = (int)(height / voxelCanvas.transform.lossyScale.y + 1) - (int)(0.5f * height / voxelCanvas.transform.lossyScale.y);
        int zend = (int)(depth / voxelCanvas.transform.lossyScale.z + 1) - (int)(0.5f * depth / voxelCanvas.transform.lossyScale.z);

        for (int x = pos[0]; x <= pos[0] + xend; x++)
        {
            for (int y = pos[1]; y <= pos[1] + yend; y++)
            {
                for (int z = pos[2]; z <= pos[2] + zend; z++)
                {
                    if (CheckSel(x, y, z) && drawState == DrawState.auto_block_paint)
                    {
                        voxelCanvas.GetComponent<VoxelCanvas>().GetBlock(x, y, z).SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawWholeColor(x, y, z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1]));
                    }
                }
            }
        }
        
    }

	// paints inside a rectangle in a single direction
    public void PaintInRectFace(Vector3 brush, float height, float width, float depth)
    {
        for (int i = 0; i < _extendedFingerDetector.Length; i++)
        {
            float hitDistance = 0.0f;
            var detector = _extendedFingerDetector[i];
            HitDirection h = ReturnDirection(detector.gameObject.transform.position, voxelCanvas, detector.gameObject.transform.forward, ref hitDistance, ref hitPoint[i]);
            if (detector.isActiveAndEnabled && h != HitDirection.None)
            {

                switch (h)
                {
                    case HitDirection.Top:
                        bh[i] = Block.Direction.up;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Bottom:
                        bh[i] = Block.Direction.down;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Forward:
                        bh[i] = Block.Direction.north;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Right:
                        bh[i] = Block.Direction.east;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Back:
                        bh[i] = Block.Direction.south;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Left:
                        bh[i] = Block.Direction.west;
                        //Debug.Log(bh[i]);
                        break;
                    default:
                        //bh[i] = Block.Direction.up;
                        break;
                }
            }

            Vector3 brushrel = voxelCanvas.transform.InverseTransformPoint(brush);
            int[] pos = new int[] { (int)brushrel.x, (int)brushrel.y, (int)brushrel.z };

            int xend = (int)(width / voxelCanvas.transform.lossyScale.x + 1) - (int)(0.5f * width / voxelCanvas.transform.lossyScale.x);
            int yend = (int)(height / voxelCanvas.transform.lossyScale.y + 1) - (int)(0.5f * height / voxelCanvas.transform.lossyScale.y);
            int zend = (int)(depth / voxelCanvas.transform.lossyScale.z + 1) - (int)(0.5f * depth / voxelCanvas.transform.lossyScale.z);

            for (int x = pos[0]; x <= pos[0] + xend; x++)
            {
                for (int y = pos[1]; y <= pos[1] + yend; y++)
                {
                    for (int z = pos[2]; z <= pos[2] + zend; z++)
                    {
                        if (CheckSel(x, y, z) && drawState == DrawState.auto_paint)
                        {
                            voxelCanvas.GetComponent<VoxelCanvas>().GetBlock(x, y, z).SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawFaceColor(x, y, z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1], bh[i]));
                        }
                    }
                }
            }
            

        }
    }

    #endregion

    #region penbrush

    // set up attach pen brush
    public void PenBrush()
    {
        // activate the pen brush
        for (int i = 0; i < penBrush.Length; i++)
        {
            if (_pinchDetectors[i].IsHolding)
            {
                penhold[i]++;
                penTips[i].GetComponent<Renderer>().material.color = Color.white;
                Vector3 pinchRelative = voxelCanvas.transform.InverseTransformPoint(penTips[i].transform.position);
                if (CheckSel((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z) && drawState == DrawState.add_auto)
                {
                    voxelCanvas.GetComponent<VoxelCanvas>().SetBlock((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, new BlockFull());
                    voxelCanvas.GetComponent<VoxelCanvas>().GetBlock((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z).SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawWholeColor((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1]));

                } else if (CheckSel((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z) && drawState == DrawState.delete_auto)
                {
                    voxelCanvas.GetComponent<VoxelCanvas>().SetBlock((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, new BlockEmpty());
                    
                }
            }
            else
            {
                penTips[i].GetComponent<Renderer>().material.color = Color.gray;
            }

        }

        if (_pinchDetectors[0].IsHolding && _pinchDetectors[1].IsHolding)
        {
            Vector3 pinchRelative0 = voxelCanvas.transform.InverseTransformPoint(penTips[0].transform.position);
            Vector3 pinchRelative1 = voxelCanvas.transform.InverseTransformPoint(penTips[1].transform.position);

            Debug.Log("distance " + Vector3.Distance(pinchRelative0, pinchRelative1));
            if ((Vector3.Distance(pinchRelative0, pinchRelative1) < 0.5f) && CheckSel((int)pinchRelative0.x, (int)pinchRelative0.y, (int)pinchRelative0.z) && drawState == DrawState.add_release)
            {
                voxelCanvas.GetComponent<VoxelCanvas>().SetBlock((int)pinchRelative0.x, (int)pinchRelative0.y, (int)pinchRelative0.z, new BlockFull());
                voxelCanvas.GetComponent<VoxelCanvas>().GetBlock((int)pinchRelative0.x, (int)pinchRelative0.y, (int)pinchRelative0.z).SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawWholeColor((int)pinchRelative0.x, (int)pinchRelative0.y, (int)pinchRelative0.z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1]));

            }
            else if ((Vector3.Distance(pinchRelative0, pinchRelative1) < 0.3f) && CheckSel((int)pinchRelative1.x, (int)pinchRelative1.y, (int)pinchRelative1.z) && drawState == DrawState.delete_release)
            {
                voxelCanvas.GetComponent<VoxelCanvas>().SetBlock((int)pinchRelative0.x, (int)pinchRelative0.y, (int)pinchRelative0.z, new BlockEmpty());
            }
        }
    }

	// autodrawing with the pen
    public void AutoPenPaint()
    {
        // activate the pen brush
        for (int i = 0; i < penBrush.Length; i++)
        {
            if (_pinchDetectors[i].IsHolding)
            {
                penhold[i]++;
                Vector3 pinchRelative = voxelCanvas.transform.InverseTransformPoint(penTips[i].transform.position);
                if (CheckSel((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z) && drawState == DrawState.auto_block_paint)
                {
                    voxelCanvas.GetComponent<VoxelCanvas>().GetBlock((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z).SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawWholeColor((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1]));

                }

            }


        }
    }

    //auto paint
    public void AutoPenPaintFace()
    {
        for (int i = 0; i < penBrush.Length; i++)
        {
            float hitDistance = 0.0f;
            var detector = penTips[i];
            HitDirection h = ReturnDirection(detector.gameObject.transform.position, voxelCanvas, detector.gameObject.transform.forward, ref hitDistance, ref hitPoint[i]);

            if (_pinchDetectors[i].IsHolding && h != HitDirection.None)
            {

                switch (h)
                {
                    case HitDirection.Top:
                        bh[i] = Block.Direction.up;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Bottom:
                        bh[i] = Block.Direction.down;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Forward:
                        bh[i] = Block.Direction.north;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Right:
                        bh[i] = Block.Direction.east;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Back:
                        bh[i] = Block.Direction.south;
                        //Debug.Log(bh[i]);
                        break;
                    case HitDirection.Left:
                        bh[i] = Block.Direction.west;
                        //Debug.Log(bh[i]);
                        break;
                    default:
                        //bh[i] = Block.Direction.up;
                        break;
                }
            }

            if (_pinchDetectors[i].IsHolding)
            {

                ////Debug.Log(bh[i]);

                Vector3 pinchRelative = voxelCanvas.transform.InverseTransformPoint(detector.transform.position);
                Vector3 hitRelative = voxelCanvas.transform.InverseTransformPoint(hitPoint[i]);
                Block blockTouch = voxelCanvas.GetComponent<VoxelCanvas>().GetBlock((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z);
                Block blockHit = voxelCanvas.GetComponent<VoxelCanvas>().GetBlock((int)hitRelative.x, (int)hitRelative.y, (int)hitRelative.z);

                //Debug.Log(bh[i] + " " + (int)pinchRelative.x + " " + (int)pinchRelative.y + " " + (int)pinchRelative.z);
                if (CheckSel((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, bh[i]) && blockTouch.IsSolid(bh[i]))
                {

                    //Debug.Log("Drawing " + bh[i] + " " + (int)pinchRelative.x + " " + (int)pinchRelative.y + " " + (int)pinchRelative.z + " " + hitDistance);
                    blockTouch.SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawFaceColor((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1], bh[i]));

                }
                else if (blockHit.IsSolid(bh[i]))
                {

                    //Debug.Log("Drawing " + bh[i] + " " + (int)hitRelative.x + " " + (int)hitRelative.y + " " + (int)hitRelative.z + " hit distance: " + hitDistance + " " + (int)pinchRelative.x + " " + (int)pinchRelative.y + " " + (int)pinchRelative.z + " " + hitDistance);
                    if (CheckSel((int)hitRelative.x, (int)hitRelative.y, (int)hitRelative.z, bh[i]) && hitDistance <= 0.005 && hitDistance > 0)
                    {
                        Debug.Log("Drawing " + bh[i] + " " + (int)hitRelative.x + " " + (int)hitRelative.y + " " + (int)hitRelative.z + " hit distance: " + hitDistance + " " + (int)pinchRelative.x + " " + (int)pinchRelative.y + " " + (int)pinchRelative.z + " " + hitDistance);
                        blockHit.SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawFaceColor((int)hitRelative.x, (int)hitRelative.y, (int)hitRelative.z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1], bh[i]));
                    }
                    blockTouch.SetTiles(voxelCanvas.GetComponent<VoxelCanvas>().DrawFaceColor((int)pinchRelative.x, (int)pinchRelative.y, (int)pinchRelative.z, voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[0], voxelCanvas.GetComponent<VoxelCanvas>().DrawColors[1], bh[i]));

                }
            }

        }
    }


    #endregion;

}
