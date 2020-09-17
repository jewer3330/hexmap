using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Class Hold Map Cell Data
/// </summary>
public class Hex : MonoBehaviour 
{
    public float gValue = 999f;
    public float hValue = 999f;
    Hex father;
    public int _dir;
    public float yRotation = -1;
    public Vector2 HexPosition
    {
        set
        {
            data.x = (int)value.x;
            data.y = (int)value.y;
        }
        get
        {
            return  new Vector2(data.x, data.y);
        }
    }
    public HexModel HexModel { get; set; }
 
    public Hex[] neighbors = new Hex[6];
    public Hex linkedHex;
    
	public MapCellData data = new MapCellData();
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

    public void Link(int idx, Hex hex)
    {
        neighbors[idx] = hex;
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

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.BeginGUI();
        UnityEditor.Handles.Label(transform.position, " g -> " + gValue);
        UnityEditor.Handles.Label(transform.position + Vector3.up, " h -> " + hValue);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2, " f -> " + getFValue());
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3, " c -> " + count);
        UnityEditor.Handles.EndGUI();
        if (father)
        {
            UnityEditor.Handles.DrawLine(transform.position, father.transform.position);
            UnityEditor.Handles.Label(father.transform.position, "head");
        }
#endif
    }

    private void OnDrawGizmos()
    {
        if (yRotation > 0)
            Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0f, yRotation, 0f) * transform.forward * 2f);

    }
    /// <summary>
    /// 移动到邻近格子产生的消耗，越大表示阻挡
    /// </summary>
    public float cost = 1f;

    public float computeGValue(Hex hex)
    {
        return cost;
    }

    public void setgValue(float v)
    {
        gValue = v;
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
        return Vector3.Distance(transform.position, hex.transform.position);
    }

    public void setFatherHexagon(Hex f)
    {
        father = f;
    }

    public Hex getFatherHexagon()
    {
        return father;
    }

    public float getFValue()
    {
        return gValue + hValue;
    }

    public int count;

    public bool canPass()
    {
        return data.walkType == MapCellData.WalkType.Walkable;
    }
}
