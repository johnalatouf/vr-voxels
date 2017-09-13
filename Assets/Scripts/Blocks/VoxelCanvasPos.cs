using UnityEngine;
using System.Collections;

// keeps track of positions of chunks in canvas
// portions of this code come from AlexStv, 
// ‘Unity voxel block tutorial pt. 1’, AlexStv, 2014. 
// http://alexstv.com/index.php/posts/unity-voxel-block-tutorial

public struct VoxelCanvasPos
{
    public int x, y, z;

    public VoxelCanvasPos(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is VoxelCanvasPos))
            return false;

        VoxelCanvasPos pos = (VoxelCanvasPos)obj;
        if (pos.x != x || pos.y != y || pos.z != z)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}