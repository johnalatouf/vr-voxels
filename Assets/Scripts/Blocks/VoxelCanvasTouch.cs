using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

public class VoxelCanvasTouch : MonoBehaviour {

    LeapProvider provider;
    public GameObject redCube;

    void Start()
    {
        provider = FindObjectOfType<LeapProvider>() as LeapProvider;
    }

    void Update()
    {
        Frame frame = provider.CurrentFrame;
        foreach (Hand hand in frame.Hands)
        {
            
            if (hand.IsLeft)
            {
                // getting the index finger position
                float distance = Vector3.Distance(hand.Fingers[1].TipPosition.ToVector3(), redCube.transform.position);
                if (distance < 0.01)
                {
                    //Debug.Log("Close to red cube! " + distance);
                }
            }
        }
    }

    void FixedUpdate()
    {

        Frame frame = provider.CurrentFrame;
        foreach (Hand hand in frame.Hands)
        {

            if (hand.IsLeft)
            {
                // getting the point position
                Vector3 fwd = transform.TransformDirection(hand.Fingers[1].Direction.ToVector3());

                RaycastHit hit;

                if (Physics.Raycast(transform.position, fwd, out hit))
                {
                    Debug.DrawLine(hand.Fingers[1].TipPosition.ToVector3(), hit.point);
                }
            }
        }
        
    }
}
