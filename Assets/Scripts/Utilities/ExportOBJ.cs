using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
// unity editor only works in unity, otherwise just save a hardcoded file
using UnityEditor;
#endif
using ObjParser;
using System.IO;
using System;

public class ExportOBJ : MonoBehaviour {

    private Obj obj;

    private Mtl mtl;

    [SerializeField]
    private VoxelCanvas voxelCanvas;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.B))
        {
            ExportObj();
        }
    }

    public void ExportObj()
    {
        VoxelCanvasPos vcp = new VoxelCanvasPos(0,0,0);
        // Arrange
        string path;
#if UNITY_EDITOR
        // Editor specific code here

        path = EditorUtility.SaveFilePanel(
               "Save Model",
               "",
               "save",
               "obj");
#else
        if (!Directory.Exists("./VR_Voxels/Saves"))
            Directory.CreateDirectory("./VR_Voxels/Saves");
        path = "./VR_Voxels/Saves/Voxels.obj";

#endif


        if (path.Length != 0)
        {

            obj = new Obj();

            mtl = new Mtl();
            string[] headers = new string[] { "ObjParser" };

            List<string> objStrings = new List<string>();
            //foreach (Chunk c in voxelCanvas.chunks)
            int tri = 1;
            foreach (KeyValuePair<VoxelCanvasPos, Chunk> c in voxelCanvas.chunks)
            {
                vcp = c.Key;
                string name = "g chunk-" + c.Value.pos.x + "-" + c.Value.pos.y + "-" + c.Value.pos.z;
                objStrings.Add(name);
                MeshFilter filter = c.Value.gameObject.GetComponent<MeshFilter>();
                //int tri = 1;
                for (int i=0; i< filter.mesh.vertices.Length; i++)
                {

                    string v = "v " + (c.Value.pos.x + filter.mesh.vertices[i].x) + " " + (c.Value.pos.y + filter.mesh.vertices[i].y) + " " + (c.Value.pos.z + filter.mesh.vertices[i].z);
                    objStrings.Add(v);

                    string vt = "vt " + (filter.mesh.uv[i].x) + " " + (filter.mesh.uv[i].y);
                    objStrings.Add(vt);
                    
                    if (tri%4 != 0)
                    {
                        tri++;
                    } else
                    {
                        int b = tri - 2;
                        int d = tri - 1;
                        int a = tri - 3;
                        string s = "f " + a + "/" + a + "/" + a + " " + b + "/" + b + "/" + a + " " + d + "/" + d + "/" + a;
                        Debug.Log(s);
                        objStrings.Add(s);

                        s = "f " + a + "/" + a + "/" + a + " " + d + "/" + d + "/" + a + " " + tri + "/" + tri + "/" + a;
                        Debug.Log(s);
                        objStrings.Add(s);

                        tri++;
                    }
                    
                }
                
            }

            IEnumerable<string> objFile = objStrings;
            Debug.Log(objStrings.Count);
            Debug.Log(objFile.ToString());
            

            // Act

            obj.LoadObj(objFile);

            // need to change this, write individually with g
            obj.WriteObjFile(path, headers);

            //SaveTexture(vcp, "texture.png", path);
        }
    }


    //not working at the moment
    public void SaveTexture(VoxelCanvasPos vcp, string imageName, string saveAs)
    {
        
        Texture2D tex = (Texture2D)voxelCanvas.chunks[vcp].GetComponent<Renderer>().material.mainTexture;
        byte[] byteArray;
        FileStream saveFile;
        byteArray = tex.EncodeToPNG();

        string temp = Convert.ToBase64String(byteArray);

        PlayerPrefs.SetString(saveAs, temp);      /// save it to file if u want.
        PlayerPrefs.SetInt(saveAs + "_w", tex.width);
        PlayerPrefs.SetInt(saveAs + "_h", tex.height);

        saveFile = File.Create(saveAs + "/" + imageName);
        //formatter.Serialize(saveFile, saveInfo);

        saveFile.Close();
    }
}
