using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

public class Platform : MonoBehaviour
{
	// voxel canvas and starting scale
    [SerializeField]
    private VoxelCanvas voxelCanvas;
    private Vector3 initScale;
	
	// handles and handle info
    [SerializeField]
    private GameObject[] handles;
    private float handleRadius;
    [SerializeField]
    private PinchDetector[] _pinchDetectors;
    [SerializeField]
    private PalmDirectionDetector[] _palmDetectors;
    LeapProvider provider;

    [SerializeField]
    private GameObject target;

    private bool palmTouch = false;

	// original info for resetting position
    private Vector3 canvasSize;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Vector3 originalCanvasPosition;
    private Vector3 originalCanvasScale;
    private Quaternion originalRotation;
    private Quaternion originalCanvasRotation;

    // keeping track of positions, distances, and angles
    private Vector3[] lastHandPos = new Vector3[] { Vector3.zero, Vector3.zero };
    private float[] oldAngle = { 0.0f, 0.0f };
    private float lastDistance;
    private bool ongoing = false;
    private int scaleFactor = 1;
    private float scaleDivisor = 27.0f;

    // bools for palms touching handles
    bool[] palmCollision = new bool[] { false, false };
    bool[] pinchCollision = new bool[] { false, false };
    bool[] handleCollision = new bool[] { false, false, false, false };
    
	// state machine for the platform
	private enum PlatformState
    {
        stationary,
        rotation,
        translation,
        scale
    }

    private PlatformState platformState = PlatformState.stationary;


    // Use this for initialization
    void Start()
    {

        initScale = transform.lossyScale;
        handleRadius = handles[0].transform.lossyScale.y * 1.3f / 2;
        canvasSize = new Vector3(voxelCanvas.VoxelCanvasDimensions[0] * voxelCanvas.chunkDimension * voxelCanvas.transform.lossyScale.x, voxelCanvas.VoxelCanvasDimensions[1] * voxelCanvas.chunkDimension * voxelCanvas.transform.lossyScale.y, voxelCanvas.VoxelCanvasDimensions[2] * voxelCanvas.chunkDimension * voxelCanvas.transform.lossyScale.z);
        transform.position = voxelCanvas.transform.position + new Vector3(canvasSize.x / 2, -0.025f, canvasSize.z / 2);
        //target.transform.position = voxelCanvas.transform.position;

        // go by the larger one
        float scale = canvasSize.x;
        if (canvasSize.z > canvasSize.x)
            scale = canvasSize.z;
        transform.localScale = new Vector3(scale * 1.7f, initScale.y / (1.4f - scale), scale * 1.7f);

        Debug.Log("scale is " + scale);

        target.transform.position = voxelCanvas.transform.position;

        //set the scale factor
        for (int i = 0; i < voxelCanvas.VoxelCanvasDimensions.Length; i += 2)
        {
            if (voxelCanvas.VoxelCanvasDimensions[i] > scaleFactor)
            {
                scaleFactor = voxelCanvas.VoxelCanvasDimensions[i];
            }
        }

        //scale the handles better
        for (int i = 0; i < handles.Length; i++)
        {
            handles[i].transform.localScale = new Vector3(handles[i].transform.localScale.x, handles[i].transform.localScale.x * 35f, handles[i].transform.localScale.z);
        }

        // for reset
        originalPosition = transform.position;
        originalScale = transform.lossyScale;
        originalCanvasScale = voxelCanvas.transform.lossyScale;
        originalCanvasPosition = voxelCanvas.transform.position;
        originalRotation = transform.rotation;
        originalCanvasRotation = voxelCanvas.transform.rotation;

        //when the canvas is very big, resize it to be manageable
        if (voxelCanvas.VoxelCanvasDimensions[0] > 3 || voxelCanvas.VoxelCanvasDimensions[2] > 3)
        {
            originalPosition = originalPosition - new Vector3(0, 0, 0.1f);
            originalScale = transform.lossyScale * 0.5f;
            originalCanvasScale = voxelCanvas.transform.lossyScale * 0.5f;
            originalRotation = transform.rotation;
            originalCanvasRotation = voxelCanvas.transform.rotation;

            transform.position = originalPosition;
            transform.localScale = originalScale;
            voxelCanvas.transform.position = originalCanvasPosition;
            voxelCanvas.transform.localScale = originalCanvasScale;
            voxelCanvas.transform.position = Vector3.MoveTowards(voxelCanvas.transform.position, target.transform.position, 100);
        }

    }

    // Update is called once per frame
    void Update()
    {
		// if both pinching, scale, otherwise transform for one pinch, rotate for palm
        bool both = pinchCollision[0] && pinchCollision[1];

        if (pinchCollision[0] || pinchCollision[1] || platformState == PlatformState.translation)
        {
            if (both)
            {
                platformState = PlatformState.scale;
                if (_pinchDetectors[0].DidEndPinch || _pinchDetectors[0].DidRelease || !_pinchDetectors[0].IsPinching)
                {
                    pinchCollision[0] = false;
                }
                if (_pinchDetectors[1].DidEndPinch || _pinchDetectors[1].DidRelease || !_pinchDetectors[1].IsPinching)
                {
                    pinchCollision[1] = false;
                }
            }
            else if (pinchCollision[0])
            {
                if (_pinchDetectors[0].DidStartHold || _pinchDetectors[0].DidStartPinch)
                {
                    platformState = PlatformState.translation;
                }

                if (_pinchDetectors[0].DidEndPinch || _pinchDetectors[0].DidRelease)
                {
                    pinchCollision[0] = false;
                    platformState = PlatformState.stationary;
                }
            }
            else if (pinchCollision[1])
            {
                if (_pinchDetectors[1].DidStartHold || _pinchDetectors[1].DidStartPinch)
                {
                    platformState = PlatformState.translation;
                }

                if (_pinchDetectors[1].DidEndPinch || _pinchDetectors[1].DidRelease)
                {
                    pinchCollision[1] = false;
                    platformState = PlatformState.stationary;
                }
            }
        }

		// fix weird scaling issues
        if (platformState == PlatformState.scale)
        {
            if ((_pinchDetectors[0].DidEndPinch || _pinchDetectors[0].DidRelease || !_pinchDetectors[0].IsPinching) && _pinchDetectors[1].DidStartPinch)
            {
                pinchCollision[0] = false;
                platformState = PlatformState.translation;
            }
            if ((_pinchDetectors[1].DidEndPinch || _pinchDetectors[1].DidRelease || !_pinchDetectors[1].IsPinching) && _pinchDetectors[0].DidStartPinch)
            {
                pinchCollision[1] = false;
                platformState = PlatformState.translation;
            }
        }
    }

    private void FixedUpdate()
    {

        if (platformState == PlatformState.rotation)
        {
            for (int i = 0; i < palmCollision.Length; i++)
            {
                var pd = _palmDetectors[i];
                if (palmCollision[i])
                {
                    RotatePlatform(pd.transform.position, i);
                }
            }
        }

        if (platformState == PlatformState.translation)
        {
            for (int i = 0; i < pinchCollision.Length; i++)
            {
                var detector = _pinchDetectors[i];
                if (pinchCollision[i])
                {
                    TranslatePlatform(detector.Position, i);
                }
            }
        }

        if (platformState == PlatformState.scale)
        {
            
            ScalePlatform(_pinchDetectors[0].Position, _pinchDetectors[1].Position);

        }

        lastDistance = Vector3.Distance(_pinchDetectors[0].Position, _pinchDetectors[1].Position);

        if (_pinchDetectors[0].DidEndPinch || _pinchDetectors[0].DidRelease || !_pinchDetectors[0].IsPinching)
                {
                    pinchCollision[0] = false;
                }
                if (_pinchDetectors[1].DidEndPinch || _pinchDetectors[1].DidRelease || !_pinchDetectors[1].IsPinching)
                {
                    pinchCollision[1] = false;
                }
    }

    // follow hand movement, pass in detector position
    public void TranslatePlatform(Vector3 pos, int i)
    {
        //keep track of travel
        Vector3 travel = pos - lastHandPos[i];

        transform.position += travel;
        voxelCanvas.transform.position += travel;
        lastHandPos[i] = pos;
    }

    // rotate platform based on hand angle
    public void RotatePlatform(Vector3 pos, int i)
    {
        Vector3 targetDir = pos - transform.position;
        float angle = Vector3.Angle(targetDir, transform.forward);
        float angle_from_right = Vector3.Angle(targetDir, transform.right);


        if (angle_from_right > 160)
        {
            transform.Rotate(0, (oldAngle[i] - angle), 0, Space.Self);
            voxelCanvas.transform.RotateAround(transform.position, Vector3.up, (oldAngle[i] - angle));
            oldAngle[i] = Vector3.Angle(targetDir, transform.forward);
        } else {
            transform.Rotate(0, (angle - oldAngle[i]), 0, Space.Self);
            voxelCanvas.transform.RotateAround(transform.position, Vector3.up, angle - oldAngle[i]);
            oldAngle[i] = Vector3.Angle(targetDir, transform.forward);
        }

        Debug.Log(i + " rotation: " + transform.rotation.eulerAngles.ToString() + " " + (angle - oldAngle[i]) + " " + angle + " " + angle_from_right);
    }
    

    //scale platform
    public void ScalePlatform(Vector3 handL, Vector3 handR)
    {

        if ((_pinchDetectors[0].DidEndPinch || _pinchDetectors[0].DidRelease || !_pinchDetectors[0].IsPinching))
        {
            pinchCollision[0] = false;
            return;
        }
        if ((_pinchDetectors[1].DidEndPinch || _pinchDetectors[1].DidRelease || !_pinchDetectors[1].IsPinching))
        {
            pinchCollision[1] = false;
            return;
        }

        float scale = Vector3.Distance(handL, handR);
        float change = scale - lastDistance;
        float step = 25.0f * Time.fixedDeltaTime;
        transform.localScale += new Vector3(change, 0, change);
        voxelCanvas.transform.localScale += new Vector3(change, change, change) / (scaleDivisor * scaleFactor);
        
        voxelCanvas.transform.position = Vector3.MoveTowards(voxelCanvas.transform.position, target.transform.position, step);

        // scale the handles better
        for (int i = 0; i < handles.Length; i++)
        {
            handles[i].transform.localScale = new Vector3(handles[i].transform.localScale.x, handles[i].transform.localScale.x * 35f, handles[i].transform.localScale.z);
        }

    }

    // a hand has entered a handle
    public void HandlePalmEnter(GameObject handle, Collider palm)
    {
        RigidHand h = palm.transform.parent.GetComponent<RigidHand>();

        handle.GetComponent<Renderer>().material.color = Color.yellow;
        float palmRot = h.GetPalmRotation().eulerAngles.z;
        if (h.Handedness == Chirality.Left &&
            palmRot > 340.0f && palmRot < 360.0f &&
            h.GetLeapHand().GetIndex().IsExtended &&
            h.GetLeapHand().GetMiddle().IsExtended)
        {
            ////Debug.Log("touching left palm to " + handle.name + " palm is active: " + h.GetPalmRotation().eulerAngles);
            palmCollision[0] = true;
            oldAngle[0] = Vector3.Angle(_palmDetectors[0].transform.position - transform.position, transform.forward);
            oldAngle[1] = Vector3.Angle(_palmDetectors[1].transform.position - transform.position, transform.forward);
            platformState = PlatformState.rotation;
        }

        if (h.Handedness == Chirality.Right &&
            palmRot > -10.0f && palmRot < 20.0f &&
            h.GetLeapHand().GetIndex().IsExtended &&
            h.GetLeapHand().GetMiddle().IsExtended)
        {
            ////Debug.Log("touching right palm to " + handle.name + " palm is active: " + h.GetPalmRotation().eulerAngles);
            palmCollision[1] = true;
            oldAngle[0] = Vector3.Angle(_palmDetectors[0].transform.position - transform.position, transform.forward);
            oldAngle[1] = Vector3.Angle(_palmDetectors[1].transform.position - transform.position, transform.forward);
            platformState = PlatformState.rotation;
        }

    }

    // a hand has exited a handle
    public void HandlePalmExit(GameObject handle, Collider palm)
    {
        RigidHand h = palm.transform.parent.GetComponent<RigidHand>();
        handle.GetComponent<Renderer>().material.color = new Color(53 / 255, 146 / 255, 255 / 255);
        //check position

        if (h.Handedness == Chirality.Left)
        {
            ////Debug.Log("exiting left palm to " + handle.name + " palm is active: " + h.GetPalmRotation().eulerAngles);
            palmCollision[0] = false;
            platformState = PlatformState.stationary;
        }

        if (h.Handedness == Chirality.Right)
        {
            ////Debug.Log("exiting right palm to " + handle.name + " palm is active: " + h.GetPalmRotation().eulerAngles);
            palmCollision[1] = false;
            platformState = PlatformState.stationary;
        }

    }

    // a hand has exited a handle
    public void HandlePinchEnter(GameObject handle, GameObject finger)
    {
        RigidHand h = finger.transform.parent.GetComponent<RigidHand>();
        handle.GetComponent<Renderer>().material.color = Color.red;
        if (h.Handedness == Chirality.Left && platformState != PlatformState.rotation)
        {
            ////Debug.Log("enter left pinch to " + handle.name);
            pinchCollision[0] = true;
            lastHandPos[0] = _pinchDetectors[0].Position;
        }

        if (h.Handedness == Chirality.Right && platformState != PlatformState.rotation)
        {
            ////Debug.Log("enter right pinch to " + handle.name);
            pinchCollision[1] = true;
            lastHandPos[1] = _pinchDetectors[1].Position;
        }


    }

    // a hand has exited a handle
    public void HandlePinchExit(GameObject handle, GameObject finger)
    {
        handle.GetComponent<Renderer>().material.color = new Color(53 / 255, 146 / 255, 255 / 255);
        RigidHand h = finger.transform.parent.GetComponent<RigidHand>();
        if (h.Handedness == Chirality.Left)
        {
            //Debug.Log("exit left pinch to " + handle.name);
            pinchCollision[0] = false;
        }

        if (h.Handedness == Chirality.Right)
        {
            //Debug.Log("exit right pinch to " + handle.name);
            pinchCollision[1] = false;
        }


    }

    //reset the position and scale
    public void ResetPositionScale()
    {
        transform.position = originalPosition;
        transform.localScale = originalScale;
        voxelCanvas.transform.position = originalCanvasPosition;
        voxelCanvas.transform.localScale = originalCanvasScale;
        voxelCanvas.transform.position = Vector3.MoveTowards(voxelCanvas.transform.position, target.transform.position, 100);
    }

    //reset rotations
    public void ResetRotation()
    {
        ResetPositionScale();
        transform.rotation = originalRotation;
        voxelCanvas.transform.rotation = originalCanvasRotation;
    }

    // loading a save file
    public void LoadPlatform(Vector3 pScale, Vector3 pos, float rotY)
    {
        initScale = pScale;
        handleRadius = handles[0].transform.lossyScale.y * 1.3f / 2;
        canvasSize = new Vector3(voxelCanvas.VoxelCanvasDimensions[0] * voxelCanvas.chunkDimension * voxelCanvas.transform.lossyScale.x, voxelCanvas.VoxelCanvasDimensions[1] * voxelCanvas.chunkDimension * voxelCanvas.transform.lossyScale.y, voxelCanvas.VoxelCanvasDimensions[2] * voxelCanvas.chunkDimension * voxelCanvas.transform.lossyScale.z);
        transform.position = pos;

        // go by the larger one
        float scale = canvasSize.x;
        if (canvasSize.z > canvasSize.x)
            scale = canvasSize.z;
        transform.localScale = pScale;
        transform.position = voxelCanvas.transform.position + new Vector3(canvasSize.x / 2, -0.025f, (canvasSize.z + voxelCanvas.transform.lossyScale.z) / 2);
        target.transform.position = voxelCanvas.transform.position;

        //set the scale factor
        // TODO - needs to update on load
        scaleFactor = 1;
        for (int i = 0; i < voxelCanvas.VoxelCanvasDimensions.Length; i += 2)
        {
            if (voxelCanvas.VoxelCanvasDimensions[i] > scaleFactor)
            {
                scaleFactor = voxelCanvas.VoxelCanvasDimensions[i];
            }
        }

        //scale the handles better
        for (int i = 0; i < handles.Length; i++)
        {
            handles[i].transform.localScale = new Vector3(handles[i].transform.localScale.x, handles[i].transform.localScale.x * 35f, handles[i].transform.localScale.z);
        }

        // for reset
        originalScale = transform.lossyScale;

    }
}
