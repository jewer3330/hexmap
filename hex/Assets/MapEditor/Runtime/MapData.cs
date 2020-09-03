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
    public WalkType walkType = WalkType.UnWalkable;
    public EventType eventType = EventType.None;
    public BuildingType buildingType = BuildingType.Floor;
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
 

    
    public MapCellData()
    {
        
    }

    public MapCellData Clone()
    {
        return (MapCellData)this.MemberwiseClone();
    }

    //public MapCellData(XLua.LuaTable mo)
    //{
    //    this.x = mo.Get<string,int>("x");
    //    this.y = mo.Get<string, int>("y");
    //    this.h = mo.Get<string, int>("h");
    //    this.id = mo.Get<string, int>("id");
    //    this.res = mo.Get<string, string>("res");
    //    this.walkType = (WalkType)System.Enum.Parse(typeof(WalkType), mo.Get<string, string>("walkType"));
    //    this.eventType = (EventType)System.Enum.Parse(typeof(EventType), mo.Get<string, string>("eventType"));
    //    this.buildingType = (BuildingType)System.Enum.Parse(typeof(BuildingType), mo.Get<string, string>("buildingType"));
    //    this.buildingRes = mo.Get<string, string>("buildingRes");
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
