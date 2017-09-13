using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
// Editor specific code here
using UnityEditor;
#endif
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class SaveLoadSession : MonoBehaviour {

	// serialize all of this info and save to binary
    private CanvasInfo canvasInfo;
    private SaveInfo saveInfo;
    private PlatformInfo platformInfo;
    [SerializeField]
    private Transform platform;
    [SerializeField]
    private VoxelCanvas voxelCanvas;
    public bool IsSceneBeingLoaded = false;
    [SerializeField]
    private Filters filters;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SaveData()
    {
        FileStream saveFile;
        BinaryFormatter formatter = new BinaryFormatter();
        string path;

        canvasInfo = new CanvasInfo(voxelCanvas.transform.position.x, voxelCanvas.transform.position.y, voxelCanvas.transform.position.z, voxelCanvas.transform.lossyScale.x, voxelCanvas.transform.lossyScale.y, voxelCanvas.transform.lossyScale.z, voxelCanvas.transform.rotation.eulerAngles.y, voxelCanvas.VoxelCanvasDimensions, voxelCanvas.VoxelCanvasTextures, voxelCanvas.BlockType);
        platformInfo = new PlatformInfo(platform.position.x, platform.position.y, platform.position.z, platform.lossyScale.x, platform.lossyScale.y, platform.lossyScale.z, platform.rotation.eulerAngles.y);
        saveInfo = new SaveInfo(platformInfo, canvasInfo);


        
        

#if UNITY_EDITOR
        // Editor specific code here

        path = EditorUtility.SaveFilePanel(
               "Save Model",
               "",
               "save",
               "binary");
#else
        if (!Directory.Exists("./VR_Voxels/Saves"))
            Directory.CreateDirectory("./VR_Voxels/Saves");
        path = "./VR_Voxels/Saves/Voxels.binary";

#endif



        if (path.Length != 0)
        {
            saveFile = File.Create(path);
            formatter.Serialize(saveFile, saveInfo);

            saveFile.Close();
        }



        
    }

    public void LoadData()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveFile;
        string path;

#if UNITY_EDITOR
        path = EditorUtility.OpenFilePanel("Open File", "", "binary");
#else   
        path = "./VR_Voxels/Saves/Voxels.binary";

#endif


        if (path.Length != 0)
        {
            Debug.Log(path);
            saveFile = File.Open(path, FileMode.Open);
            saveInfo = (SaveInfo)formatter.Deserialize(saveFile);
            saveFile.Close();
            Debug.Log("save info: " + saveInfo.Canvas.Dimensions);
            platform.GetComponent<Platform>().ResetRotation();
            voxelCanvas.LoadVoxelCanvas(saveInfo.Canvas.Dimensions, saveInfo.Canvas.BlockTypes, saveInfo.Canvas.VoxelCanvasTextures, new Vector3(saveInfo.Canvas.ScaleX, saveInfo.Canvas.ScaleY, saveInfo.Canvas.ScaleZ), new Vector3(saveInfo.Canvas.PositionX, saveInfo.Canvas.PositionY, saveInfo.Canvas.PositionZ), saveInfo.Canvas.RotationY);

            platform.GetComponent<Platform>().LoadPlatform(new Vector3(saveInfo.Platform.ScaleX, saveInfo.Platform.ScaleY, saveInfo.Platform.ScaleZ), new Vector3(saveInfo.Platform.PositionX, saveInfo.Platform.PositionY, saveInfo.Platform.PositionZ), saveInfo.Platform.RotationY);

            string printString = "";
            foreach (int i in saveInfo.Canvas.BlockTypes)
            {
                printString += i + " ";
            }
            Debug.Log(printString);
        }

        // for filters
        filters.SetSizes(voxelCanvas.VoxelCanvasDimensions[0] * voxelCanvas.chunkDimension, voxelCanvas.VoxelCanvasDimensions[1] * voxelCanvas.chunkDimension, voxelCanvas.VoxelCanvasDimensions[2] * voxelCanvas.chunkDimension);
    }

    

}

// For clenliness, and assuming this doesn't slow things too much, first copy all of this info to serializable classes, then serialize

[Serializable]
public class CanvasInfo
{
    private float positionX, positionY, positionZ;
    private float scaleX, scaleY, scaleZ;

    private float rotationY;
    private int[] dimensions;
    private Block.Tile[,,,] voxelCanvasTextures;
    private int[,,] blockTypes;

    public CanvasInfo(float positionX, float positionY, float positionZ, float scaleX, float scaleY, float scaleZ, float rotationY, int[] dimensions, Block.Tile[,,,] voxelCanvasTextures, int[,,] blockTypes)
    {
        this.positionX = positionX;
        this.positionY = positionY;
        this.positionZ = positionZ;
        this.scaleX = scaleX;
        this.scaleY = scaleY;
        this.scaleZ = scaleZ;
        this.rotationY = rotationY;
        this.dimensions = dimensions;
        this.voxelCanvasTextures = voxelCanvasTextures;
        this.blockTypes = blockTypes;
    }

    public float PositionX
    {
        get
        {
            return positionX;
        }

        set
        {
            positionX = value;
        }
    }

    public float PositionY
    {
        get
        {
            return positionY;
        }

        set
        {
            positionY = value;
        }
    }

    public float PositionZ
    {
        get
        {
            return positionZ;
        }

        set
        {
            positionZ = value;
        }
    }

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

    public float ScaleZ
    {
        get
        {
            return scaleZ;
        }

        set
        {
            scaleZ = value;
        }
    }

    public float RotationY
    {
        get
        {
            return rotationY;
        }

        set
        {
            rotationY = value;
        }
    }

    public int[] Dimensions
    {
        get
        {
            return dimensions;
        }

        set
        {
            dimensions = value;
        }
    }

    public Block.Tile[,,,] VoxelCanvasTextures
    {
        get
        {
            return voxelCanvasTextures;
        }

        set
        {
            voxelCanvasTextures = value;
        }
    }

    public int[,,] BlockTypes
    {
        get
        {
            return blockTypes;
        }

        set
        {
            blockTypes = value;
        }
    }
}

// save everything for the platform

[Serializable]
public class PlatformInfo
{
    private float positionX, positionY, positionZ;
    private float scaleX, scaleY, scaleZ;

    private float rotationY;

    public PlatformInfo(float positionX, float positionY, float positionZ, float scaleX, float scaleY, float scaleZ, float rotationY)
    {
        this.positionX = positionX;
        this.positionY = positionY;
        this.positionZ = positionZ;
        this.scaleX = scaleX;
        this.scaleY = scaleY;
        this.scaleZ = scaleZ;
        this.rotationY = rotationY;
    }

    public float PositionX
    {
        get
        {
            return positionX;
        }

        set
        {
            positionX = value;
        }
    }

    public float PositionY
    {
        get
        {
            return positionY;
        }

        set
        {
            positionY = value;
        }
    }

    public float PositionZ
    {
        get
        {
            return positionZ;
        }

        set
        {
            positionZ = value;
        }
    }

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

    public float ScaleZ
    {
        get
        {
            return scaleZ;
        }

        set
        {
            scaleZ = value;
        }
    }

    public float RotationY
    {
        get
        {
            return rotationY;
        }

        set
        {
            rotationY = value;
        }
    }
}

[Serializable]
public class SaveInfo
{
    private PlatformInfo platform;
    private CanvasInfo canvas;

    public SaveInfo(PlatformInfo platform, CanvasInfo canvas)
    {
        this.platform = platform;
        this.canvas = canvas;
    }

    public PlatformInfo Platform
    {
        get
        {
            return platform;
        }

        set
        {
            platform = value;
        }
    }

    public CanvasInfo Canvas
    {
        get
        {
            return canvas;
        }

        set
        {
            canvas = value;
        }
    }
}
