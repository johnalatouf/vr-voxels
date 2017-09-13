using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// this is the main voxel canvas
// portions of this code come from AlexStv, 
// ‘Unity voxel block tutorial pt. 1’, AlexStv, 2014. 
// http://alexstv.com/index.php/posts/unity-voxel-block-tutorial

public class VoxelCanvas : MonoBehaviour {

	// holds chunk information
    public Dictionary<VoxelCanvasPos, Chunk> chunks = new Dictionary<VoxelCanvasPos, Chunk>();
    public GameObject chunkPrefab;
    private int placement = 0;
    public float initialSize = 0.1f;                                         // Scale the voxelCanvas after the chunk is drawn for Oculus camera
    public int chunkDimension = 16;                                          // dimensions of chunk
    private Block.Tile[,,,] voxelCanvasTextures;                             // keeps track of voxel textures
    private int[] voxelCanvasDimensions = { 3, 3, 3 };                       // how many chunks
    private static int numberOfChunks = 0;
	// keeps track of chosen tile texture
    public int drawColorX = 0;
    public int drawColorY = 0;
    private int[,,] blockType;
    private Dictionary<string, int[]> selectionList;                          //saves selected voxels
    private int selectionListCount = 0;
    private SelectionHighlight highlight;

	// holds the grid textured objects
    [SerializeField]
    CanvasGrid[] gridWalls;

    [SerializeField]
    GameObject voxelBorders;


    public int ChunkDimension
    {
        get
        {
            return chunkDimension;
        }
    }

    public int[] VoxelCanvasDimensions
    {
        get
        {
            return voxelCanvasDimensions;
        }
        set
        {
            voxelCanvasDimensions = value;
        }
    }

    public int NumberOfChunks
    {
        get
        {
            return numberOfChunks;
        }
        set
        {
            numberOfChunks = value;
        }
    }

    public int[] DrawColors 
    {
           get
        {
            return new int[2] {drawColorX, drawColorY };
        }
           set
        {
            int[] cols = value;
            drawColorX = cols[0];
            drawColorY = cols[1];
            Debug.Log("You're drawing with " + drawColorX + ", " + drawColorY);
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

    public int[,,] BlockType
    {
        get
        {
            return blockType;
        }

        set
        {
            blockType = value;
        }
    }

    public Dictionary<string, int[]> SelectionList
    {
        get
        {
            return selectionList;
        }

        set
        {
            selectionList = value;
        }
    }

    public int SelectionListCount
    {
        get
        {
            return selectionListCount;
        }

        set
        {
            selectionListCount = value;
        }
    }

    void Start()
    {
        
    }

    void Awake()
    {
        // set the size from the programinfo object
        Debug.Log("STARTING CANVAS");
        highlight = this.GetComponent<SelectionHighlight>();
        ProgramInfo info = null;
        if (GameObject.FindWithTag("ProgramInfo") != null)
        {
            info = GameObject.FindWithTag("ProgramInfo").GetComponent<ProgramInfo>();
        }
        float initialX;
        numberOfChunks = 0;
        selectionList = new Dictionary<string, int[]>();
        
        if (GameObject.FindWithTag("ProgramInfo") != null && info != null)
        {
            voxelCanvasDimensions = new int[3] { info.VoxelCanvasDimensions[0], info.VoxelCanvasDimensions[1], info.VoxelCanvasDimensions[2] };
        }
        else
        {
            VoxelCanvasDimensions = new int[3] { 2, 2, 2 };
        }

        voxelCanvasTextures = new Block.Tile[chunkDimension * voxelCanvasDimensions[0], chunkDimension * voxelCanvasDimensions[1], chunkDimension * voxelCanvasDimensions[2], 6];
        blockType = new int[chunkDimension * voxelCanvasDimensions[0], chunkDimension * voxelCanvasDimensions[1], chunkDimension * voxelCanvasDimensions[2]];

        //set the voxel chunks properly
        if (numberOfChunks < voxelCanvasDimensions[0] * voxelCanvasDimensions[1] * voxelCanvasDimensions[2])
        {
            for (int x = 0; x < voxelCanvasDimensions[0]; x++)
            {
                for (int y = 0; y < voxelCanvasDimensions[1]; y++)
                {
                    for (int z = 0; z < voxelCanvasDimensions[2]; z++)
                    {
                        CreateChunk(x * chunkDimension, y * chunkDimension, z * chunkDimension);
                        numberOfChunks++;
                    }
                }
            }
        }

        voxelBorders.transform.localScale = new Vector3(voxelCanvasDimensions[0], voxelCanvasDimensions[1], voxelCanvasDimensions[2]) * chunkDimension;
        voxelBorders.transform.position = transform.position + voxelBorders.transform.localScale * 0.5f - Vector3.one * 0.5f;

        //set up the grid texture
        foreach (CanvasGrid wall in gridWalls)
        {
            wall.ChunkDimension = 1/initialSize;

            wall.SizeTexture();
        }

        voxelBorders.transform.SetParent(transform);
        //scale down the voxelCanvas

        initialX = transform.position.x - (voxelCanvasDimensions[1] * chunkDimension * initialSize) / 2.0f;
        transform.position = new Vector3(initialX, transform.position.y + 0.58f, transform.position.z - 2.3f);
        
        transform.localScale = new Vector3(initialSize, initialSize, initialSize);


        Debug.Log("The voxel canvas is " + chunkDimension * voxelCanvasDimensions[0] + " " + chunkDimension * voxelCanvasDimensions[1] + " " + chunkDimension * voxelCanvasDimensions[2] + " " + numberOfChunks);
    }

    // Update is called once per frame
    void Update () {
        
        if (Input.GetKey(KeyCode.A))
        {
            //sets an empty block at 0 0 0
            SetBlock(0, 0, 0, new BlockEmpty());
            //sets a full block at coords
            SetBlock(2 + placement, 3, 2 + placement, new BlockFull());
            //gets a block and sets all its tiles to one color
            GetBlock(2 + placement, 3, 2 + placement).SetTiles(DrawWholeColor(0 + placement, 3, 0 + placement, 1, 13));
            //gets a block and sets indicated direction to color
            GetBlock(placement, 3, placement).SetTiles(DrawFaceColor(placement, 3, placement, 3, 5, Block.Direction.north));
            placement++;
        }
        if (Input.GetKey(KeyCode.W))
        {
            SetBlock(1, 0, 0, new BlockEmpty());
            SetBlock(1 + placement, 2 + placement, 1 + placement, new BlockFull());
            GetBlock(1 + placement, 2 + placement, 1 + placement).SetTiles(DrawWholeColor(1 + placement, 2 + placement, 3 + placement, 4, 7));
            placement++;
        }
        if (Input.GetKey(KeyCode.D))
        {
            SetBlock(0, 1, 0, new BlockEmpty());
            SetBlock(1 + placement, placement-2, 1 + placement, new BlockFull());
            GetBlock(1 + placement, placement - 2, 1 + placement).SetTiles(DrawFaceColor(1 + placement, placement - 2, 1 + placement, 0, 9, Block.Direction.up));
            placement++;
        }
        if (Input.GetKey(KeyCode.S))
        {
            GetBlock(placement, 3, placement).SetTiles(DrawFaceColor(placement, 3, placement, 0, 5, Block.Direction.west));
        }

        if (placement > 40)
        {

            placement = 0;
        }
        
    }

    // functions for adding to the selction
    public void AddVoxelToSelection(int x, int y, int z)
    {
        Chunk chunk = GetChunk(x, y, z);

        if (chunk == null)
            return;

        string key;
        for (int i = 0; i < 6; i++)
        {
            key = x + "," + y + "," + z + "," + i;
            if (!selectionList.ContainsKey(key))
            {
                selectionList[key] = new int[] { x, y, z, i };
                selectionListCount++;
            }
            
            //selectionList.Add(key, new int[] { x, y, z, i });
        }
        highlight.HLVoxel(x, y, z);
    }

    public void DeselectVoxelToSelection(int x, int y, int z)
    {
        Chunk chunk = GetChunk(x, y, z);

        if (chunk == null)
            return;

        string key;
        for (int i = 0; i < 6; i++)
        {
            key = x + "," + y + "," + z + "," + i;
            
            if (selectionList.ContainsKey(key))
            {
                selectionList.Remove(key);
                selectionListCount--;
            }
        }
        highlight.UnHLVoxel(x, y, z);
    }

    public void AddFaceToSelection(int x, int y, int z, Block.Direction direction)
    {
        Chunk chunk = GetChunk(x, y, z);

        if (chunk == null)
            return;

        int face = 0;
        string key;
        switch (direction)
        {
            case Block.Direction.up:
                face = 0;
                break;
            case Block.Direction.down:
                face = 1;
                break;
            case Block.Direction.north:
                face = 2;
                break;
            case Block.Direction.east:
                face = 3;
                break;
            case Block.Direction.south:
                face = 4;
                break;
            case Block.Direction.west:
                face = 5;
                break;
        }
        key = x + "," + y + "," + z + "," + face;
        if (!selectionList.ContainsKey(key))
        {
            selectionList[key] = new int[] { x, y, z, face };
            selectionListCount++;
        }
        highlight.HLFace(x, y, z, direction);
    }

    public void DeselectFaceToSelection(int x, int y, int z, Block.Direction direction)
    {
        Chunk chunk = GetChunk(x, y, z);

        if (chunk == null)
            return;

        int face = 0;
        string key;
        switch (direction)
        {
            case Block.Direction.up:
                face = 0;
                break;
            case Block.Direction.down:
                face = 1;
                break;
            case Block.Direction.north:
                face = 2;
                break;
            case Block.Direction.east:
                face = 3;
                break;
            case Block.Direction.south:
                face = 4;
                break;
            case Block.Direction.west:
                face = 5;
                break;
        }
        key = x + "," + y + "," + z + "," + face;
        
        if (selectionList.ContainsKey(key))
        {
            selectionList.Remove(key);
            selectionListCount--;
        }
        highlight.UnHLFace(x, y, z, face);
    }

    public void ClearSelection()
    {
        selectionList.Clear();
        highlight.ClearHL();
        selectionListCount = 0;
    }

    public bool VoxelInSelection(int x, int y, int z)
    {
        Chunk chunk = GetChunk(x, y, z);

        if (chunk == null)
            return false;

        string key;
        int faces = 0;
        for (int i = 0; i < 6; i++)
        {
            key = x + "," + y + "," + z + "," + i;
            if (selectionList.ContainsKey(key))
            {
                faces++;
            }
        }
        return (faces > 0);
        
    }

    public bool FaceInSelection(int x, int y, int z, Block.Direction direction)
    {
        Chunk chunk = GetChunk(x, y, z);

        if (chunk == null)
            return false;

        int face = 0;
        string key;
        switch (direction)
        {
            case Block.Direction.up:
                face = 0;
                break;
            case Block.Direction.down:
                face = 1;
                break;
            case Block.Direction.north:
                face = 2;
                break;
            case Block.Direction.east:
                face = 3;
                break;
            case Block.Direction.south:
                face = 4;
                break;
            case Block.Direction.west:
                face = 5;
                break;
        }
        key = x + "," + y + "," + z + "," + face;
        return (selectionList.ContainsKey(key));

    }



    public void CreateChunk(int x, int y, int z)
    {
        VoxelCanvasPos voxelCanvasPos = new VoxelCanvasPos(x, y, z);

        //Instantiate the chunk at the coordinates using the chunk prefab
        GameObject newChunkObject = Instantiate(
                        chunkPrefab, new Vector3(x, y, z),
                        Quaternion.Euler(Vector3.zero)
                    ) as GameObject;
       

        Chunk newChunk = newChunkObject.GetComponent<Chunk>();
        newChunk.FillBlocks(ChunkDimension);

        newChunk.pos = voxelCanvasPos;
        newChunk.voxelCanvas = this;

        

        //Add it to the chunks dictionary with the position as the key
        chunks.Add(voxelCanvasPos, newChunk);

        //set blocks to 0
        Array.Clear(blockType, 0, blockType.Length);

        for (int xi = 0; xi < chunkDimension; xi++)
        {
            for (int yi = 0; yi < chunkDimension; yi++)
            {
                for (int zi = 0; zi < chunkDimension; zi++)
                {

                    //creates the block and sets the correct textures
                    SetBlock(x + xi, y + yi, z + zi, new BlockEmpty());
                    //GetBlock(x + xi, y + yi, z + zi).SetTiles(tiles);
                    GetBlock(x + xi, y + yi, z + zi).SetTiles(DrawWholeColor(x + xi, y + yi, z + zi, drawColorX, drawColorY));
                    //Debug.Log("create chunk " + (x + xi) + ", " + (y + yi) + ", " + (z + zi));

                }
            }
        }

        // parent the chunk to the voxelCanvas for scaling
        newChunkObject.transform.parent = transform;
    }

    public void DestroyChunk(int x, int y, int z)
    {
        Chunk chunk = null;
        if (chunks.TryGetValue(new VoxelCanvasPos(x, y, z), out chunk))
        {
            UnityEngine.Object.Destroy(chunk.gameObject);
            chunks.Remove(new VoxelCanvasPos(x, y, z));
        }
    }

    public Chunk GetChunk(int x, int y, int z)
    {
        VoxelCanvasPos pos = new VoxelCanvasPos();

        float multiple = ChunkDimension;
        pos.x = Mathf.FloorToInt(x / multiple) * ChunkDimension;
        pos.y = Mathf.FloorToInt(y / multiple) * ChunkDimension;
        pos.z = Mathf.FloorToInt(z / multiple) * ChunkDimension;

        Chunk containerChunk = null;

        chunks.TryGetValue(pos, out containerChunk);

        return containerChunk;
    }

    public Block GetBlock(int x, int y, int z)
    {
        Chunk containerChunk = GetChunk(x, y, z);

        if (containerChunk != null)
        {
            Block block = containerChunk.GetBlock(
                x - containerChunk.pos.x,
                y - containerChunk.pos.y,
                z - containerChunk.pos.z);

            return block;
        }
        else
        {
            return new BlockEmpty();
        }

    }

    public void SetBlock(int x, int y, int z, Block block)
    {
        Chunk chunk = GetChunk(x, y, z);

        if (chunk != null)
        {
            chunk.SetBlock(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, block);

            if (block.GetType().Name.Equals("BlockFull") ) {
                blockType[x, y, z] = 1;
            } else
            {
                blockType[x, y, z] = 0;
            }
            chunk.update = true;
        }
    }

    // when drawing a new block, apply all the color
    public Block.Tile[] DrawWholeColor(int x, int y, int z, int tilex, int tiley)
    {


        Chunk chunk = GetChunk(x, y, z);

        if (chunk != null)
        {


            voxelCanvasTextures[x, y, z, 0].x = tilex;
            voxelCanvasTextures[x, y, z, 0].y = tiley;
            voxelCanvasTextures[x, y, z, 1].x = tilex;
            voxelCanvasTextures[x, y, z, 1].y = tiley;
            voxelCanvasTextures[x, y, z, 2].x = tilex;
            voxelCanvasTextures[x, y, z, 2].y = tiley;
            voxelCanvasTextures[x, y, z, 3].x = tilex;
            voxelCanvasTextures[x, y, z, 3].y = tiley;
            voxelCanvasTextures[x, y, z, 4].x = tilex;
            voxelCanvasTextures[x, y, z, 4].y = tiley;
            voxelCanvasTextures[x, y, z, 5].x = tilex;
            voxelCanvasTextures[x, y, z, 5].y = tiley;


            Block.Tile[] tiles = {
                            voxelCanvasTextures[x, y, z, 0],
                            voxelCanvasTextures[x, y, z, 1],
                            voxelCanvasTextures[x, y, z, 2],
                            voxelCanvasTextures[x, y, z, 3],
                            voxelCanvasTextures[x, y, z, 4],
                            voxelCanvasTextures[x, y, z, 5]
            };
            chunk.update = true;
            return tiles;
        }
        else
        {
            //out of bounds?
            Block.Tile[] tiles = {
                            voxelCanvasTextures[0, 0, 0, 0],
                            voxelCanvasTextures[0, 0, 0, 1],
                            voxelCanvasTextures[0, 0, 0, 2],
                            voxelCanvasTextures[0, 0, 0, 3],
                            voxelCanvasTextures[0, 0, 0, 4],
                            voxelCanvasTextures[0, 0, 0, 5]
            };

            return tiles;
        }

    }

    // when drawing a new block, apply all the color
    public Block.Tile[] DrawEachColor(int x, int y, int z, int[,] tile)
    {


        Chunk chunk = GetChunk(x, y, z);

        if (chunk != null)
        {

            for (int i=0; i<6; i++)
            {
                voxelCanvasTextures[x, y, z, i].x = tile[i,0];
                voxelCanvasTextures[x, y, z, i].y = tile[i,1];
            }
            

            Block.Tile[] tiles = {
                            voxelCanvasTextures[x, y, z, 0],
                            voxelCanvasTextures[x, y, z, 1],
                            voxelCanvasTextures[x, y, z, 2],
                            voxelCanvasTextures[x, y, z, 3],
                            voxelCanvasTextures[x, y, z, 4],
                            voxelCanvasTextures[x, y, z, 5]
            };
            chunk.update = true;
            return tiles;
        }
        else
        {
            //out of bounds?
            Block.Tile[] tiles = {
                            voxelCanvasTextures[0, 0, 0, 0],
                            voxelCanvasTextures[0, 0, 0, 1],
                            voxelCanvasTextures[0, 0, 0, 2],
                            voxelCanvasTextures[0, 0, 0, 3],
                            voxelCanvasTextures[0, 0, 0, 4],
                            voxelCanvasTextures[0, 0, 0, 5]
            };

            return tiles;
        }

    }

    // apply tile to selection

    public void PaintSelection()
    {
        foreach (KeyValuePair<string, int[]> sel in selectionList)
        {
            GetBlock(sel.Value[0], sel.Value[1], sel.Value[2]).SetTiles(DrawFaceColor(sel.Value[0], sel.Value[1], sel.Value[2], drawColorX, drawColorY, sel.Value[3]));
        }
    }

    public void PaintSelection(int tilex, int tiley)
    {
        foreach (KeyValuePair<string, int[]> sel in selectionList)
        {
            GetBlock(sel.Value[0], sel.Value[1], sel.Value[2]).SetTiles(DrawFaceColor(sel.Value[0], sel.Value[1], sel.Value[2], tilex, tiley, sel.Value[3]));
        }
    }

    // delete whole selection
    public void DeleteSelection()
    {
        foreach (KeyValuePair<string, int[]> sel in selectionList)
        {
            SetBlock(sel.Value[0], sel.Value[1], sel.Value[2], new BlockEmpty());
        }

        ClearSelection();
    }

    // fill whole selection
    public void FillSelection()
    {
        foreach (KeyValuePair<string, int[]> sel in selectionList)
        {
            SetBlock(sel.Value[0], sel.Value[1], sel.Value[2], new BlockFull());
            GetBlock(sel.Value[0], sel.Value[1], sel.Value[2]).SetTiles(DrawWholeColor(sel.Value[0], sel.Value[1], sel.Value[2], drawColorX, drawColorY));
        }
    }

    public Block.Tile[] DrawFaceColor(int x, int y, int z, int tilex, int tiley, Block.Direction direction)
    {
        Chunk chunk = GetChunk(x, y, z);

        if (chunk != null)
        {

            switch (direction)
            {
                case Block.Direction.up:
                    voxelCanvasTextures[x, y, z, 0].x = tilex;
                    voxelCanvasTextures[x, y, z, 0].y = tiley;
                    break;
                case Block.Direction.down:
                    voxelCanvasTextures[x, y, z, 1].x = tilex;
                    voxelCanvasTextures[x, y, z, 1].y = tiley;
                    break;
                case Block.Direction.north:
                    voxelCanvasTextures[x, y, z, 2].x = tilex;
                    voxelCanvasTextures[x, y, z, 2].y = tiley;
                    break;
                case Block.Direction.east:
                    voxelCanvasTextures[x, y, z, 3].x = tilex;
                    voxelCanvasTextures[x, y, z, 3].y = tiley;
                    break;
                case Block.Direction.south:
                    voxelCanvasTextures[x, y, z, 4].x = tilex;
                    voxelCanvasTextures[x, y, z, 4].y = tiley;
                    break;
                case Block.Direction.west:
                    voxelCanvasTextures[x, y, z, 5].x = tilex;
                    voxelCanvasTextures[x, y, z, 5].y = tiley;
                    break;
            }

            Block.Tile[] tiles = {
                            voxelCanvasTextures[x, y, z, 0],
                            voxelCanvasTextures[x, y, z, 1],
                            voxelCanvasTextures[x, y, z, 2],
                            voxelCanvasTextures[x, y, z, 3],
                            voxelCanvasTextures[x, y, z, 4],
                            voxelCanvasTextures[x, y, z, 5],
                            voxelCanvasTextures[x, y, z, 2],
                            voxelCanvasTextures[x, y, z, 3]
            };
            chunk.update = true;
            return tiles;
        } else
        {
            Block.Tile t = new Block.Tile();
            Block.Tile[] tiles = {
                            t, t, t, t, t, t, t, t
            };
            return tiles;
        }

    }

    // a version for just setting direction by int
    public Block.Tile[] DrawFaceColor(int x, int y, int z, int tilex, int tiley, int direction)
    {
        Chunk chunk = GetChunk(x, y, z);

        if (chunk != null)
        {

            voxelCanvasTextures[x, y, z, direction].x = tilex;
            voxelCanvasTextures[x, y, z, direction].y = tiley;

            Block.Tile[] tiles = {
                            voxelCanvasTextures[x, y, z, 0],
                            voxelCanvasTextures[x, y, z, 1],
                            voxelCanvasTextures[x, y, z, 2],
                            voxelCanvasTextures[x, y, z, 3],
                            voxelCanvasTextures[x, y, z, 4],
                            voxelCanvasTextures[x, y, z, 5],
                            voxelCanvasTextures[x, y, z, 2],
                            voxelCanvasTextures[x, y, z, 3]
            };
            chunk.update = true;
            return tiles;
        }
        else
        {
            Block.Tile t = new Block.Tile();
            Block.Tile[] tiles = {
                            t, t, t, t, t, t, t, t
            };
            return tiles;
        }

    }

    // loading a save file
    public void LoadVoxelCanvas(int[] dim, int[,,] bType, Block.Tile[,,,] tiles, Vector3 scale, Vector3 pos, float rotY)
    {
        transform.localScale = Vector3.one;
        numberOfChunks = 0;

        //destroy all chunks
        for (int x = 0; x < voxelCanvasDimensions[0]; x++)
        {
            for (int y = 0; y < voxelCanvasDimensions[1]; y++)
            {
                for (int z = 0; z < voxelCanvasDimensions[2]; z++)
                {
                    DestroyChunk(x * chunkDimension, y * chunkDimension, z * chunkDimension);
                }
            }
        }

        blockType =  new int[bType.GetLength(0), bType.GetLength(1), bType.GetLength(2)];

        voxelCanvasDimensions = new int[3] { dim[0], dim[1], dim[2] };
        voxelCanvasTextures = new Block.Tile[chunkDimension * voxelCanvasDimensions[0], chunkDimension * voxelCanvasDimensions[1], chunkDimension * voxelCanvasDimensions[2], 6];

        //set the voxel chunks properly
        if (numberOfChunks < dim[0] * dim[1] * dim[2])
        {
            for (int x = 0; x < dim[0]; x++)
            {
                for (int y = 0; y < dim[1]; y++)
                {
                    for (int z = 0; z < dim[2]; z++)
                    {
                        CreateChunk(x * chunkDimension, y * chunkDimension, z * chunkDimension);
                        numberOfChunks++;
                    }
                }
            }
        }



        //set the voxels
        for (int x = 0; x < dim[0] * chunkDimension; x++)
        {
            for (int y = 0; y < dim[1] * chunkDimension; y++)
            {
                for (int z = 0; z < dim[2] * chunkDimension; z++)
                {
                    if (bType[x, y, z] == 1)
                    {
                        SetBlock(x, y, z, new BlockFull());
                        int[,] tile = new int[6,2];
                        for (int i=0; i<6; i++)
                        {
                            voxelCanvasTextures[x, y, z, i].x = tiles[x, y, z, i].x;
                            voxelCanvasTextures[x, y, z, i].y = tiles[x, y, z, i].y;
                            tile[i, 0] = tiles[x, y, z, i].x;
                            tile[i, 1] = tiles[x, y, z, i].y;
                        }
                        GetBlock(x, y, z).SetTiles(DrawEachColor(x, y, z, tile));
                    }

                }
            }
        }
        //voxelCanvasTextures = tiles;
        blockType = bType;

        //scale down the voxelCanvas
        transform.localScale = scale;
        transform.position = pos;

        Debug.Log("The voxel canvas is " + chunkDimension * voxelCanvasDimensions[0] + " " + chunkDimension * voxelCanvasDimensions[1] + " " + chunkDimension * voxelCanvasDimensions[2] + " " + numberOfChunks);

        //resize the grid
        voxelBorders.transform.localScale = Vector3.one;
        voxelBorders.transform.localScale = new Vector3(voxelCanvasDimensions[0], voxelCanvasDimensions[1], voxelCanvasDimensions[2]) * chunkDimension;
        voxelBorders.transform.position = transform.lossyScale.x*(transform.position + voxelBorders.transform.localScale * 0.5f - Vector3.one * 0.5f);

        //set up the grid texture
        foreach (CanvasGrid wall in gridWalls)
        {
            wall.ChunkDimension = 1 / initialSize;

            wall.SizeTexture();
        }
    }

    
}
