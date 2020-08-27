using UnityEngine;
using System.Collections;

/// <summary>
/// Class Hold Map Cell Data
/// </summary>
public class Hex : MonoBehaviour 
{
    float gValue = 999f;
    float hValue = 999f;
    Hex father;
    public int _dir;
    public float yRotation = -1;
    public Vector2 HexPosition;
    public HexModel HexModel { get; set; }
 
    public Hex[] neighbors = new Hex[6];
    public Hex linkedHex;
    
	public MapCellData data;
    public int dir
    {
        set
        {
            _dir = value;
            yRotation = -30f + (_dir+1) * 60f;
            if (neighbors[_dir] != null)
            {
                linkedHex = neighbors[_dir];
                linkedHex.HexModel.GetComponent<MeshRenderer>().sharedMaterial.color = Color.grey;
            }
        }
    }
    public void InitializeModel()
    {
        
        HexModel = gameObject.AddComponent<HexModel>();
       
        HexModel.GenMesh();
        //hex.renderer.material.mainTexture = Resources.Load("textures/hex") as Texture2D;
    }


    void Link(int idx, World world, Vector2 offset)
    {
        Vector2 index = HexPosition;
        index += offset;
        if ((index.y < world.gridY && index.y>=0) && (index.x < world.gridX && index.x >= 0))
            neighbors[idx] = world.Hexes[index];
        else
            neighbors[idx] = null;
    }
    public void Link(World world)
    {
        if(HexPosition.y%2 == 0)
        {
            Link(0, world, new Vector2(0, 1));
            Link(1, world, new Vector2(1, 0));
            Link(2, world, new Vector2(0, -1));
            Link(3, world, new Vector2(-1, -1));
            Link(4, world, new Vector2(-1, 0));
            Link(5, world, new Vector2(-1, 1));
        }
        else
        {
            Link(0, world, new Vector2(1, 1));
            Link(1, world, new Vector2(1, 0));
            Link(2, world, new Vector2(1, -1));
            Link(3, world, new Vector2(0, -1));
            Link(4, world, new Vector2(-1, 0));
            Link(5, world, new Vector2(0, 1));
        }
       
    }

	void Start () 
    {
	
	}

	void Update () 
    {
	
	}

    private void OnDrawGizmos()
    {
        if (yRotation > 0)
            Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0f, yRotation, 0f) * transform.forward * 2f);
    }
    public float cost = 1f;

    public float computeGValue(Hex hex)
    {
        return cost;
    }

    public void setgValue(float v)
    {
        gValue = v * cost;
    }

    public float getgValue()
    {
        return gValue;
    }

    public void sethValue(float v)
    {
        hValue = v;
    }

    public float gethValue()
    {
        return hValue;
    }

    public float computeHValue(Hex hex)
    {
        return Vector3.Distance(transform.position, hex.transform.position)*cost;
    }

    public void setFatherHexagon(Hex f)
    {
        father = f;
    }

    public Hex getFatherHexagon()
    {
        return father;
    }
}
