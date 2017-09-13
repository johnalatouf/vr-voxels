using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

// this class holds all of the filter function

public class Filters : MonoBehaviour {

	// the voxel canvas and info
    [SerializeField]
    private GameObject voxelCanvas;
    private VoxelCanvas vc;

	// the voxel editor
    [SerializeField]
    private VoxelEditor ve;

    [SerializeField]
    private Slider slider_fs, slider_rs, slider_n;

	// secret debugging things
    private bool p = false;
    private bool o = false;
    private bool i = false;


    // keeping track of sizes
    private int xend;
    private int yend;
    private int zend;

    // Use this for initialization
    void Start () {
        vc = voxelCanvas.GetComponent<VoxelCanvas>();
        SetSizes(vc.VoxelCanvasDimensions[0] * vc.chunkDimension, vc.VoxelCanvasDimensions[1] * vc.chunkDimension, vc.VoxelCanvasDimensions[2] * vc.chunkDimension);

    }
	
	// Update is called once per frame
	void Update () {
		//debugging wave functions
        if (!p && Input.GetKey(KeyCode.P))
        {
            SinWaves(5, 10);
            p = true;
        }
        if (!o && Input.GetKey(KeyCode.O))
        {
            SinWaves(20, 10);
            o = true;
        }
        if (!i && Input.GetKey(KeyCode.I))
        {
            Terrain(10, 10, new int[] { 10, 5 }, new int[] { 4, 2 });
            i = true;
        }
    }

	// set up sizes for the functions
    public void SetSizes(int x, int y, int z)
    {
        xend = x;
        yend = y;
        zend = z;
    }

	// these functions are called by the UI
    public void FlatSmoothButton()
    {
        Debug.Log(slider_fs.value);
        Smooth(slider_fs.value);
    }

    public void RoundSmoothButton()
    {
        Debug.Log(slider_rs.value);
        SofterSmooth(slider_rs.value);
    }

    public void NoiseButton()
    {
        Debug.Log(slider_n.value);
        Noise(slider_n.value);
    }

    public void WavesButton()
    {
        SinWaves(5, 10);
        SofterSmooth(0.5f);
        SofterSmooth(0.5f);
    }

    // rounded smothing smoothing
	// check neighbours and compare sum to threshold
    public void SofterSmooth(float level)
    {
        float surrounding = 0.0f;

        if (level > 1.0f)
        {
            level = 1.0f;
        }

        for (int x = 0; x < xend; x++)
        {
            for (int y = 0; y < yend; y++)
            {
                for (int z = 0; z < zend; z++)
                {


                    if (vc.GetBlock(x, y, z).GetType().Name.Equals("BlockFull") && ve.CheckSel(x, y, z)) // first check its full and select
                    {
                        surrounding = 0.0f;

                        for (int xi = x - 1; xi <= x + 1; xi++)
                        {
                            for (int yi = y - 1; yi <= y + 1; yi++)
                            {
                                for (int zi = z - 1; zi <= z + 1; zi++)
                                {
                                    if (vc.GetBlock(xi, yi, zi).GetType().Name.Equals("BlockFull"))
                                    {
                                        surrounding += 0.037f;
                                    }
                                }
                            }
                        }

                        if (surrounding < (level))
                        {
                            vc.SetBlock(x, y, z, new BlockEmpty());
                        }
                    }
                    else if (vc.GetBlock(x, y, z).GetType().Name.Equals("BlockEmpty") && ve.CheckSel(x, y, z)) // first check its full and select
                    {
                        surrounding = 0.0f;
                        Block.Tile b = vc.VoxelCanvasTextures[x, y, z, 3];
                        Dictionary<Block.Tile, int> btiles = new Dictionary<Block.Tile, int>();

                        for (int xi = x - 1; xi <= x + 1; xi++)
                        {
                            for (int yi = y - 1; yi <= y + 1; yi++)
                            {
                                for (int zi = z - 1; zi <= z + 1; zi++)
                                {
                                    if (vc.GetBlock(xi, yi, zi).GetType().Name.Equals("BlockFull"))
                                    {
                                        surrounding += 0.039f;
                                        // TODO - could be better
                                        b = vc.VoxelCanvasTextures[xi, yi, zi, 0];
                                        if (btiles.ContainsKey(b))
                                            btiles[b]++;
                                        else
                                            btiles.Add(b, 1);
                                    }
                                }
                            }
                        }


                        if (surrounding > (1 - level))
                        {
                            vc.SetBlock(x, y, z, new BlockFull());
                            // TODO - change the color
                            Block.Tile bl = btiles.OrderByDescending(m => m.Value).FirstOrDefault().Key;
                            vc.GetBlock(x, y, z).SetTiles(vc.DrawWholeColor(x, y, z, bl.x, bl.y));
                        }
                    }

                }
            }
        }
    }


    // flat surface-like smoothing
	// check neighbours and compare sum to threshold
    public void Smooth(float level)
    {
        float surrounding = 0.0f;

        if (level > 1.0f)
        {
            level = 1.0f;
        }

        for (int x = 0; x < xend; x++)
        {
            for (int y = 0; y < yend; y++)
            {
                for (int z = 0; z < zend; z++)
                {


                    if (vc.GetBlock(x, y, z).GetType().Name.Equals("BlockFull") && ve.CheckSel(x, y, z)) // first check its full and select
                    {
                        surrounding = 0.0f;
                        

                        if (vc.GetBlock(x + 1, y, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                        }
                        if (vc.GetBlock(x - 1, y, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                        }
                        if (vc.GetBlock(x, y + 1, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                        }
                        if (vc.GetBlock(x, y - 1, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                        }
                        if (vc.GetBlock(x, y, z + 1).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                        }
                        if (vc.GetBlock(x, y, z - 1).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                        }

                        if (surrounding < (level))
                        {
                            vc.SetBlock(x, y, z, new BlockEmpty());
                        }
                    }
                    else if (vc.GetBlock(x, y, z).GetType().Name.Equals("BlockEmpty") && ve.CheckSel(x, y, z)) // first check its full and select
                    {
                        surrounding = 0.0f;
                        
                        Block.Tile b = vc.VoxelCanvasTextures[x, y, z, 3];
                        if (vc.GetBlock(x + 1, y, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                            b = vc.VoxelCanvasTextures[x + 1, y, z, 3];
                        }
                        if (vc.GetBlock(x - 1, y, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                            b = vc.VoxelCanvasTextures[x - 1, y, z, 5];
                        }
                        if (vc.GetBlock(x, y + 1, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                            b = vc.VoxelCanvasTextures[x, y + 1, z, 1];
                        }
                        if (vc.GetBlock(x, y - 1, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                            b = vc.VoxelCanvasTextures[x, y - 1, z, 0];
                        }
                        if (vc.GetBlock(x, y, z + 1).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                            b = vc.VoxelCanvasTextures[x, y, z + 1, 4];
                        }
                        if (vc.GetBlock(x, y, z - 1).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                            b = vc.VoxelCanvasTextures[x, y, z - 1, 2];
                        }

                        if (surrounding > (1 - level))
                        {
                            vc.SetBlock(x, y, z, new BlockFull());
                            // TODO - change the color

                            vc.GetBlock(x, y, z).SetTiles(vc.DrawWholeColor(x, y, z, b.x, b.y));
                        }

                        
                    }

                }
            }
        }
    }

    // simple noise
	// check neighbours and apply most noise to those above threshold
	// also apply random voxels
    public void Noise(float level)
    {
        float surrounding = 0.0f;

        float rand;

        for (int x = 0; x < xend; x++)
        {
            for (int y = 0; y < yend; y++)
            {
                for (int z = 0; z < zend; z++)
                {


                    if (vc.GetBlock(x, y, z).GetType().Name.Equals("BlockFull") && ve.CheckSel(x, y, z)) // first check its full and select
                    {
                        surrounding = 0.0f;

                        if (vc.GetBlock(x + 1, y, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                        }
                        if (vc.GetBlock(x - 1, y, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                        }
                        if (vc.GetBlock(x, y + 1, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                        }
                        if (vc.GetBlock(x, y - 1, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                        }
                        if (vc.GetBlock(x, y, z + 1).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                        }
                        if (vc.GetBlock(x, y, z - 1).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                        }

                        //rand = Random.Range(0.2f, 1.2f);
                        rand = Random.Range(0.15f, 1.7f - level);
                        ////Debug.Log("surrounding " + surrounding + " rand " + rand);
                        if (surrounding > rand)
                        {
                            vc.SetBlock(x, y, z, new BlockEmpty());
                        }
                    }
                    else if (vc.GetBlock(x, y, z).GetType().Name.Equals("BlockEmpty") && ve.CheckSel(x, y, z)) // first check its full and select
                    {
                        surrounding = 0.0f;
                        Block.Tile b = vc.VoxelCanvasTextures[x, y, z, 3];
                        if (vc.GetBlock(x + 1, y, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                            b = vc.VoxelCanvasTextures[x + 1, y, z, 3];
                        }
                        if (vc.GetBlock(x - 1, y, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                            b = vc.VoxelCanvasTextures[x - 1, y, z, 5];
                        }
                        if (vc.GetBlock(x, y + 1, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                            b = vc.VoxelCanvasTextures[x, y + 1, z, 1];
                        }
                        if (vc.GetBlock(x, y - 1, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                            b = vc.VoxelCanvasTextures[x, y - 1, z, 0];
                        }
                        if (vc.GetBlock(x, y, z + 1).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                            b = vc.VoxelCanvasTextures[x, y, z + 1, 4];
                        }
                        if (vc.GetBlock(x, y, z - 1).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                            b = vc.VoxelCanvasTextures[x, y, z - 1, 2];
                        }

                        
                        rand = Random.Range(0.2f, level + 0.1f);
                        if (surrounding > rand)
                        {
                            vc.SetBlock(x, y, z, new BlockFull());
                            vc.GetBlock(x, y, z).SetTiles(vc.DrawWholeColor(x, y, z, b.x, b.y));
                        }

                        if (surrounding == 0)
                        {
                            rand = Random.Range(0f, 0.75f);
                            if (rand < level * 0.08f)
                            {
                                vc.SetBlock(x, y, z, new BlockFull());
                                vc.GetBlock(x, y, z).SetTiles(vc.DrawWholeColor(x, y, z, vc.drawColorX, vc.drawColorY));
                            }
                        }
                    }

                }
            }
        }
    }

    // random smoothing
	// not currently using this
    public void RandomSmooth()
    {
        float surrounding = 0.0f;
        float rand;

        for (int x = 0; x < xend; x++)
        {
            for (int y = 0; y < yend; y++)
            {
                for (int z = 0; z < zend; z++)
                {


                    if (vc.GetBlock(x, y, z).GetType().Name.Equals("BlockFull") && ve.CheckSel(x, y, z)) // first check its full and select
                    {
                        surrounding = 0.0f;

                        if (vc.GetBlock(x + 1, y, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                        }
                        if (vc.GetBlock(x - 1, y, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                        }
                        if (vc.GetBlock(x, y + 1, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                        }
                        if (vc.GetBlock(x, y - 1, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                        }
                        if (vc.GetBlock(x, y, z + 1).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                        }
                        if (vc.GetBlock(x, y, z - 1).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                        }

                        rand = Random.Range(0.1f, 0.8f);
                        //Debug.Log("surrounding " + surrounding + " rand " + rand);
                        if (surrounding < rand)
                        {
                            vc.SetBlock(x, y, z, new BlockEmpty());
                        }
                    }
                    else if (vc.GetBlock(x, y, z).GetType().Name.Equals("BlockEmpty") && ve.CheckSel(x, y, z)) // first check its full and select
                    {
                        surrounding = 0.0f;
                        
                        Block.Tile b = vc.VoxelCanvasTextures[x, y, z, 3];
                        if (vc.GetBlock(x + 1, y, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                            b = vc.VoxelCanvasTextures[x + 1, y, z, 3];
                        }
                        if (vc.GetBlock(x - 1, y, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                            b = vc.VoxelCanvasTextures[x - 1, y, z, 5];
                        }
                        if (vc.GetBlock(x, y + 1, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                            b = vc.VoxelCanvasTextures[x, y + 1, z, 1];
                        }
                        if (vc.GetBlock(x, y - 1, z).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                            b = vc.VoxelCanvasTextures[x, y - 1, z, 0];
                        }
                        if (vc.GetBlock(x, y, z + 1).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                            b = vc.VoxelCanvasTextures[x, y, z + 1, 4];
                        }
                        if (vc.GetBlock(x, y, z - 1).GetType().Name.Equals("BlockFull"))
                        {
                            surrounding += 0.17f;
                            b = vc.VoxelCanvasTextures[x, y, z - 1, 2];
                        }

                        rand = Random.Range(0f, 1.2f);
                        if (surrounding >= rand)
                        {
                            vc.SetBlock(x, y, z, new BlockFull());
                            // TODO - change the color

                            vc.GetBlock(x, y, z).SetTiles(vc.DrawWholeColor(x, y, z, b.x, b.y));
                        }
                    }

                }
            }
        }
    }

    // waves
	// use sin function to make uniform waves
    public void SinWaves(int height, int width)
    {
        int drawup = 0;
        Block.Tile b;
        //keep track of y's already dipped


        for (int x = 0; x < xend; x++)
        {
            for (int z = 0; z < zend; z++)
            {
                for  (int y = yend - 1; y >= 0; y--)
                {

                    //int[] coord = new int[] { x, z };
                    //Debug.Log("xs.Contains(coord)" + xs.Contains(coord));
                    if (vc.GetBlock(x, y, z).GetType().Name.Equals("BlockFull") && ve.CheckSel(x, y, z)) // first check its full and select
                    {
                        b = vc.VoxelCanvasTextures[x, y, z, 0];
                        if (vc.GetBlock(x, y + 1, z).GetType().Name.Equals("BlockEmpty"))
                        {
                            drawup = (int)(Mathf.Sin(((float)x / width) * 180 / Mathf.PI) * Mathf.Sin(((float)z / width) * 180 / Mathf.PI) * height);


                            //Debug.Log((Mathf.Sin(2 * Mathf.Sqrt(x * x + z * z) / 10) * 10) + " " + drawup);

                            //now build up on that
                            if (drawup > 0)
                            {
                                for (int dy = y; dy <= y + drawup; dy++)
                                {
                                    vc.SetBlock(x, dy, z, new BlockFull());
                                    Debug.Log(b.x + " " + b.y);
                                    vc.GetBlock(x, dy, z).SetTiles(vc.DrawWholeColor(x, dy, z, b.x, b.y));
                                }
                            }
                            else if (drawup <= 0)
                            {
                                for (int dy = y; dy >= (y + drawup); dy--)
                                {
                                    vc.SetBlock(x, dy, z, new BlockEmpty());
                                }
                                
                            }
                            break;
                        }

                    }


                }
            }
        }
    }


	// not currently used, but maybe in the future
	// makes one big ripple
    public void Ripple(int height, int width)
    {
        int drawup = 0;
        


        for (int x = 0; x < xend; x++)
        {
            for (int z = 0; z < zend; z++)
            {
                for (int y = yend - 1; y >= 0; y--)
                {

                    if (vc.GetBlock(x, y, z).GetType().Name.Equals("BlockFull") && ve.CheckSel(x, y, z)) // first check its full and select
                    {

                        if (vc.GetBlock(x, y + 1, z).GetType().Name.Equals("BlockEmpty"))
                        {
                            

                            drawup = (int)(Mathf.Sin(2 * Mathf.Sqrt((x - xend / 2) * (x - xend / 2) + (z - zend / 2) * (z - zend / 2)) / width) * 10);

                            //now build up on that
                            if (drawup > 0)
                            {
                                for (int dy = y; dy <= y + drawup; dy++)
                                {
                                    vc.SetBlock(x, dy, z, new BlockFull());
                                    Block.Tile b = vc.VoxelCanvasTextures[x, y, z, 0];
                                    vc.GetBlock(x, y, z).SetTiles(vc.DrawWholeColor(x, y, z, b.x, b.y));
                                }
                            }
                            else if (drawup <= 0)
                            {
                                for (int dy = y; dy >= (y + drawup); dy--)
                                {
                                    vc.SetBlock(x, dy, z, new BlockEmpty());
                                }

                            }
                            //xs.Add(coord);
                            break;
                        }

                    }


                }
            }
        }
    }

    // terrain
	// not currently in use
    public void Terrain(int height, int width, int[] topTexture, int[] bottomTexture)
    {
        int drawup = 0;
        int rand;
        


        for (int x = 0; x < xend; x++)
        {
            
            for (int z = 0; z < zend; z++)
            {
                rand = (int)Random.Range(height, height - 3);

                for (int y = yend - 1; y >= 0; y--)
                {

                    //int[] coord = new int[] { x, z };
                    //Debug.Log("xs.Contains(coord)" + xs.Contains(coord));
                    if (vc.GetBlock(x, y, z).GetType().Name.Equals("BlockFull") && ve.CheckSel(x, y, z)) // first check its full and select
                    {
                        
                        vc.GetBlock(x, y, z).SetTiles(vc.DrawWholeColor(x, y, z, bottomTexture[0], bottomTexture[1]));
                        //b = vc.VoxelCanvasTextures[x, y, z, 0];
                        if (vc.GetBlock(x, y + 1, z).GetType().Name.Equals("BlockEmpty"))
                        {
                            int newy = 0;

                            drawup = (int)(Mathf.Sin(((float)x / width) * 180 / Mathf.PI) * Mathf.Sin(((float)z / width) * 180 / Mathf.PI) * rand);

                            //now build up on that
                            if (drawup > 0)
                            {
                                for (int dy = y; dy <= y + drawup; dy++)
                                {
                                    vc.SetBlock(x, dy, z, new BlockFull());
                                    //Debug.Log(b.x + " " + b.y);
                                    vc.GetBlock(x, y, z).SetTiles(vc.DrawWholeColor(x, y, z, topTexture[0], topTexture[1]));
                                    vc.GetBlock(x, dy, z).SetTiles(vc.DrawWholeColor(x, dy, z, bottomTexture[0], bottomTexture[1]));
                                    newy = dy;
                                }
                                
                            }
                            else if (drawup <= 0)
                            {
                                for (int dy = y; dy >= (y + drawup); dy--)
                                {
                                    vc.SetBlock(x, dy, z, new BlockEmpty());
                                    newy = dy;
                                }

                            }
                            for (int dy = newy-1; dy >= 0; dy--)
                            {
                                vc.GetBlock(x, dy, z).SetTiles(vc.DrawWholeColor(x, dy, z, bottomTexture[0], bottomTexture[1]));
                            }
                            //xs.Add(coord);
                            break;
                        }

                    }


                }
            }
        }
    }
}
