using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// set up highlight quads for selections

public class SelectionHighlight : MonoBehaviour {

    [SerializeField]
    private GameObject highlight;

    [SerializeField]
    private GameObject platform;

    private Dictionary<string, GameObject> highlightList;                                      //saves selected voxels

    // Use this for initialization
    void Start () {
        highlightList = new Dictionary<string, GameObject>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

	// place the quad in the position of the voxel, then rotate, then move forward slighly past voxel face
    public GameObject InstantiateHighlight(Vector3 position, Vector3 normal)

    {
        var spawned = GameObject.Instantiate(highlight);
		spawned.transform.localScale = transform.localScale;

        spawned.transform.position = transform.TransformPoint(position);

        spawned.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, normal);
        spawned.transform.position = spawned.transform.position - spawned.transform.forward * (transform.localScale.z / 1.9f);

        if (normal == transform.up || normal == -transform.up)
        {
            spawned.transform.Rotate(0, transform.rotation.eulerAngles.y, 0, Space.World);
            //Debug.Log("top, bottom");
        }

        spawned.SetActive(true);
        spawned.transform.SetParent(this.transform);

        return spawned;

    }

    // functions for adding to the selction
    public void HLVoxel(int x, int y, int z)
    {
        string key;
        Vector3 normal = transform.up;
        Vector3 pos = new Vector3(x, y, z);
        float offset = 0;
        for (int i = 0; i < 6; i++)
        {
            key = x + "," + y + "," + z + "," + i;
            switch (i)
            {
                case 0:
                    normal = transform.up;
                    pos = new Vector3(pos.x, pos.y + offset, pos.z);
                    break;
                case 1:
                    normal = -transform.up;
                    pos = new Vector3(pos.x, pos.y - 2*offset, pos.z);
                    break;
                case 2:
                    normal = transform.forward;
                    pos = new Vector3(pos.x + offset, pos.y, pos.z);
                    break;
                case 3:
                    normal = transform.right;

                    pos = new Vector3(pos.x - offset, pos.y, pos.z);
                    break;
                case 4:
                    normal = -transform.forward;

                    pos = new Vector3(pos.x - offset, pos.y, pos.z - offset);
                    break;
                case 5:
                    normal = -transform.right;
                    pos = new Vector3(pos.x - offset, pos.y, pos.z - offset);
                    break;
                default:
                    break;
            }
            if (!highlightList.ContainsKey(key))
            {
                GameObject hl = InstantiateHighlight(pos, normal);
                //highlightList.Add(key, hl);
                highlightList[key] = hl;
            }
        }
    }

    // functions for adding to the selction
    public void HLFace(int x, int y, int z, Block.Direction direction)
    {
        int face = 0;
        Vector3 normal = transform.up;
        Vector3 pos = new Vector3(x, y, z);
        Vector3 offset = transform.lossyScale / 2;
        string key;
        switch (direction)
        {
            case Block.Direction.up:
                face = 0;
                normal = transform.up;
                pos = new Vector3(pos.x, pos.y + offset.y, pos.z);
                break;
            case Block.Direction.down:
                normal = -transform.up;
                pos = new Vector3(pos.x, pos.y - offset.y, pos.z);
                face = 1;
                break;
            case Block.Direction.north:
                normal = transform.forward;
                pos = new Vector3(pos.x, pos.y, pos.z + offset.z);
                face = 2;
                break;
            case Block.Direction.east:
                face = 3;
                normal = -transform.forward;
                pos = new Vector3(pos.x, pos.y, pos.z - offset.z);
                break;
            case Block.Direction.south:
                face = 4;
                normal = -transform.forward;
                pos = new Vector3(pos.x, pos.y, pos.z - offset.z);
                break;
            case Block.Direction.west:
                face = 5;
                normal = -transform.right;
                pos = new Vector3(pos.x - offset.x, pos.y, pos.z);
                break;
        }
        key = x + "," + y + "," + z + "," + face;
        if (!highlightList.ContainsKey(key))
        {
            GameObject hl = InstantiateHighlight(pos, normal);
            //highlightList.Add(key, hl);
            highlightList[key] = hl;
        }
    }

	// unhilight a voxel
    public void UnHLVoxel(int x, int y, int z)
    {
        string key;
        for (int i = 0; i < 6; i++)
        {
            key = x + "," + y + "," + z + "," + i;
            if (highlightList.ContainsKey(key))
            {
                Destroy(highlightList[key]);
                highlightList.Remove(key);
            }
        }
    }

	// unhighlight a face
    public void UnHLFace(int x, int y, int z, int i)
    {
        string key;
        key = x + "," + y + "," + z + "," + i;
        if (highlightList.ContainsKey(key))
        {
            Destroy(highlightList[key]);
            highlightList.Remove(key);
        }
    }

	// empty selection
    public void ClearHL()
    {
        foreach (KeyValuePair<string, GameObject> hl in highlightList)
        {
            Destroy(hl.Value);
        }

        highlightList.Clear();
    }
}
