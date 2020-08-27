using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class MapCellData
{
    public int x;
    public int y;
    public int h;
    public int id;
    public string res;
    public string buildingRes;

    public enum WalkType
    {
        UnWalkable,
        Walkable,
        
        
    }

    public enum EventType
    { 
        None,
        Start,
        End,
        Door,
        Key,
        Reward,
    }

    public enum BuildingType
    { 
        Floor,
        Building,
    }
 

    public WalkType walkType = WalkType.UnWalkable;
    public EventType eventType = EventType.None;
    public BuildingType buildingType = BuildingType.Floor;
    public MapCellData()
    {

    }

    public MapCellData Clone()
    {
        return (MapCellData)this.MemberwiseClone();
    }

    //public MapCellData(MapCell mo)
    //{
    //    this.x = mo.data.x;
    //    this.y = mo.data.y;
    //    this.h = mo.data.h;
    //    this.id = mo.data.id;
    //    this.res = mo.data.res;
    //    this.type = mo.data.type;
    //    this.eventType = mo.data.eventType;
    //}

  

    public MapCellData(Hex mo)
    {
        this.x = mo.data.x;
        this.y = mo.data.y;
        this.h = mo.data.h;
        this.id = mo.data.id;
        this.res = mo.data.res;
        this.walkType = mo.data.walkType;
        this.eventType = mo.data.eventType;
        this.buildingType = mo.data.buildingType;
        this.buildingRes = mo.data.buildingRes;
    }
}



public class MapData : ScriptableObject
{
    public Hex[] hexs;
    public MapCellData[] cells;
    public int mapWidth;
    public int mapHeight;

    public int HexPositionToIndex(int x, int y)
    {
        return y * mapWidth + x;
    }
}
