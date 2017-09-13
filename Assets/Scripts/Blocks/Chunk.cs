using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

// builds the mesh for voxel chunks
// portions of this code come from AlexStv, 
// ‘Unity voxel block tutorial pt. 1’, AlexStv, 2014. 
// http://alexstv.com/index.php/posts/unity-voxel-block-tutorial

public class Chunk : MonoBehaviour
{
	// chunk info
    private int chunkSize = 16;
    private Block[,,] blocks = new Block[1, 1, 1];
    public bool update = true;

	// mesh holder
    MeshFilter filter;
    MeshCollider coll;

    public VoxelCanvas voxelCanvas;
    public VoxelCanvasPos pos;

    public void FillBlocks(int chunkDimension)
    {
        chunkSize = chunkDimension;
        blocks = new Block[chunkSize, chunkSize, chunkSize];
    }

    void Start()
    {
        chunkSize = voxelCanvas.ChunkDimension;
        if (blocks[0,0,0] == null) {
            blocks = new Block[chunkSize, chunkSize, chunkSize];
        }
        
        filter = gameObject.GetComponent<MeshFilter>();
        coll = gameObject.GetComponent<MeshCollider>();
    }

    //Update is called once per frame
    void Update()
    {
        if (update)
        {
            update = false;
            UpdateChunk();
        }
    }

    public Block GetBlock(int x, int y, int z)
    {
        if (InRange(x) && InRange(y) && InRange(z))
            return blocks[x, y, z];
        return voxelCanvas.GetBlock(pos.x + x, pos.y + y, pos.z + z);
    }

    public bool InRange(int index)
    {
        if (index < 0 || index >= chunkSize)
            return false;

        return true;
    }

    public void SetBlock(int x, int y, int z, Block block)
    {
        if (InRange(x) && InRange(y) && InRange(z))
        {
            blocks[x, y, z] = block;
        }
        else
        {
            voxelCanvas.SetBlock(pos.x + x, pos.y + y, pos.z + z, block);
        }
    }

    // Updates the chunk based on its contents
    void UpdateChunk()
    {
        MeshData meshData = new MeshData();

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    meshData = blocks[x, y, z].Blockdata(this, x, y, z, meshData);
                }
            }
        }

        RenderMesh(meshData);
    }

    // Sends the calculated mesh information
    // to the mesh and collision components
    void RenderMesh(MeshData meshData)
    {
        filter.mesh.Clear();
        filter.mesh.vertices = meshData.vertices.ToArray();
        filter.mesh.triangles = meshData.triangles.ToArray();

        filter.mesh.uv = meshData.uv.ToArray();
        filter.mesh.RecalculateNormals();

        coll.sharedMesh = null;
        Mesh mesh = new Mesh();
        mesh.vertices = meshData.colVertices.ToArray();
        mesh.triangles = meshData.colTriangles.ToArray();
        mesh.RecalculateNormals();

        coll.sharedMesh = mesh;
    }

}