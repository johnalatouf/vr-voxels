using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this builds the grid by scaling the grid texture

public class CanvasGrid : MonoBehaviour {

	// need to know which direction this wall points to scale grid
    private Renderer rend;
    private float scaleX = 1;
    private float scaleY = 1;
    [SerializeField]
    private Vector3 scaleDirections;

    private float chunkDimension;


    public float ScaleX
    {
        get
        {
            return scaleX;
        }

        set
        {
            scaleX = value;
        }
    }

    public float ScaleY
    {
        get
        {
            return scaleY;
        }

        set
        {
            scaleY = value;
        }
    }

    public float ChunkDimension
    {
        get
        {
            return chunkDimension;
        }

        set
        {
            chunkDimension = value;
        }
    }

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (scaleDirections.x > 0 && scaleDirections.y > 0)
        {
            ScaleX = transform.parent.lossyScale.x * ChunkDimension;
            ScaleY = transform.parent.lossyScale.y * ChunkDimension;
        }
        else if (scaleDirections.x > 0 && scaleDirections.z > 0)
        {
            ScaleX = transform.parent.lossyScale.x * ChunkDimension;
            ScaleY = transform.parent.lossyScale.z * ChunkDimension;
        }
        else if (scaleDirections.z > 0 && scaleDirections.y > 0)
        {
            ScaleY = transform.parent.lossyScale.y * ChunkDimension;
            ScaleX = transform.parent.lossyScale.z * ChunkDimension;
        }

        rend.material.mainTextureScale = new Vector2(ScaleX, ScaleY);
    }

    // Update is called once per frame
    void Update () {
		
    }

	// depending on orientation, scale texture to fit canvas
    public void SizeTexture()
    {
        rend = GetComponent<Renderer>();
        if (scaleDirections.x > 0 && scaleDirections.y > 0)
        {
            ScaleX = transform.parent.lossyScale.x * ChunkDimension;
            ScaleY = transform.parent.lossyScale.y * ChunkDimension;
        }
        else if (scaleDirections.x > 0 && scaleDirections.z > 0)
        {
            ScaleX = transform.parent.lossyScale.x * ChunkDimension;
            ScaleY = transform.parent.lossyScale.z * ChunkDimension;
        }
        else if (scaleDirections.z > 0 && scaleDirections.y > 0)
        {
            ScaleY = transform.parent.lossyScale.y * ChunkDimension;
            ScaleX = transform.parent.lossyScale.z * ChunkDimension;
        }
        Debug.Log("texture scales: " + ScaleX + " " + ScaleY);
        rend.material.mainTextureScale = new Vector2(ScaleX, ScaleY);
    }
}
