using UnityEngine;
using System.Collections;

// empty block
// portions of this code come from AlexStv, 
// ‘Unity voxel block tutorial pt. 1’, AlexStv, 2014. 
// http://alexstv.com/index.php/posts/unity-voxel-block-tutorial

public class BlockEmpty : Block
{
    public BlockEmpty()
        : base()
    {

    }

    public override MeshData Blockdata
        (Chunk chunk, int x, int y, int z, MeshData meshData)
    {
        return meshData;
    }

    public override bool IsSolid(Block.Direction direction)
    {
        return false;
    }
}