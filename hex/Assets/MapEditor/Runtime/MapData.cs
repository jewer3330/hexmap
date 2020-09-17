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

    [System.NonSerialized]
    public float gValue = 999f;
    [System.NonSerialized]
    public float hValue = 999f;
    [System.NonSerialized]
    public MapCellData father;
    [System.NonSerialized]
    public Vector3 position;
    [System.NonSerialized]
    public MapCellData[] neighbors = new MapCellData[6];
    [System.NonSerialized]
    /// <summary>
    /// 移动到邻近格子产生的消耗，越大表示阻挡
    /// </summary>
    public float cost = 1f;
    [System.NonSerialized]
    public int count;

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

    public void Link(int idx, MapCellData hex)
    {
        neighbors[idx] = hex;
    }

    //public MapCellData(XLua.LuaTable mo)
    //{
    //    this.x = mo.Get<string,int>("x");
    //    this.y = mo.Get<string, int>("y");
    //    this.id = mo.Get<string, int>("id");
    //    this.res = mo.Get<string, string>("res");
    //    this.walkType = (WalkType)System.Enum.Parse(typeof(WalkType), mo.Get<string, string>("walkType"));
    //    this.eventType = (EventType)System.Enum.Parse(typeof(EventType), mo.Get<string, string>("eventId"));
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


    
    public float computeGValue(MapCellData hex)
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

    public float computeHValue(MapCellData hex)
    {
        return Vector3.Distance(position, hex.position);
    }

    public void setFatherHexagon(MapCellData f)
    {
        father = f;
    }

    public MapCellData getFatherHexagon()
    {
        return father;
    }

    public float getFValue()
    {
        return gValue + hValue;
    }

   

    public bool canPass()
    {
        return walkType == WalkType.Walkable;
    }

   

    //1.把起始格添加到开启列表。
    //2.重复如下的工作：
    //   a) 寻找开启列表中F值最低的格子。我们称它为当前格。
    //   b) 把它切换到关闭列表。
    //   c) 对相邻的6格中的每一个？
    //       * 如果它不可通过或者已经在关闭列表中，略过它。反之如下。
    //       * 如果它不在开启列表中，把它添加进去。把当前格作为这一格的父节点。记录这一格的F,G,和H值。
    //       * 如果它已经在开启列表中，用G值为参考检查新的路径是否更好。更低的G值意味着更好的路径。如果是这样，就把这一格的父节点改成当前格，
    //           并且重新计算这一格的G和F值。如果你保持你的开启列表按F值排序，改变之后你可能需要重新对开启列表排序。

    //   d) 停止，当你
    //       * 把目标格添加进了关闭列表(注解)，这时候路径被找到，或者
    //       * 没有找到目标格，开启列表已经空了。这时候，路径不存在。
    //3.保存路径。从目标格开始，沿着每一格的父节点移动直到回到起始格。这就是你的路径。

    public static int counting;

    public static List<MapCellData> searchRoute(MapCellData thisHexagon, MapCellData targetHexagon)
    {
        counting = 0;
        var nowHexagon = thisHexagon;
        //nowHexagon.reset();
        nowHexagon.setgValue(0);//fix
        nowHexagon.sethValue(0);
        nowHexagon.setFatherHexagon(null);
        openList.Add(nowHexagon);
        bool finded = false;
        while (!finded)
        {
            openList.Remove(nowHexagon);//将当前节点从openList中移除  
            closeList.Add(nowHexagon);//将当前节点添加到关闭列表中  
            if (closeList.Contains(targetHexagon))
            {
                finded = true;
            }
            var neighbors = nowHexagon.neighbors;//获取当前六边形的相邻六边形  
            //print("当前相邻节点数----" + neighbors.size());  
            foreach (var neighbor in neighbors)
            {
                if (neighbor == null) continue;
                if (closeList.Contains(neighbor) || !neighbor.canPass())
                {//在关闭列表里  
                    //print("无法通过或者已在关闭列表");  
                    continue;
                }

                if (openList.Contains(neighbor))
                {//该节点已经在开启列表里  
                    //print("已在开启列表，判断是否更改父节点");  
                    float assueGValue = neighbor.computeGValue(nowHexagon) + nowHexagon.getgValue();//计算假设从当前节点进入，该节点的g估值  
                    if (assueGValue < neighbor.getgValue())
                    {//假设的g估值小于于原来的g估值  
                        //openList.Remove(neighbor);//重新排序该节点在openList的位置  
                        neighbor.setgValue(assueGValue);//从新设置g估值  
                        neighbor.setFatherHexagon(nowHexagon);
                        neighbor.count = counting++;
                        openList.Sort((l, r) => l.getFValue().CompareTo(r.getFValue()));
                        //openList.Add(neighbor);//从新排序openList。  
                    }
                }
                else
                {//没有在开启列表里  
                    //print("不在开启列表，添加");  
                    neighbor.sethValue(neighbor.computeHValue(targetHexagon));//计算好他的h估值  
                    neighbor.setgValue(neighbor.computeGValue(nowHexagon) + nowHexagon.getgValue());//计算该节点的g估值（到当前节点的g估值加上当前节点的g估值）  
                    openList.Add(neighbor);//添加到开启列表里  
                    neighbor.setFatherHexagon(nowHexagon);//将当前节点设置为该节点的父节点  
                    neighbor.count = counting++;
                }
            }

            if (openList.Count <= 0)
            {
                //print("无法到达该目标");  
                break;
            }
            else
            {
                openList.Sort((l, r) => l.getFValue().CompareTo(r.getFValue()));
                nowHexagon = openList[0];//得到f估值最低的节点设置为当前节点  
            }
        }
        openList.Clear();
        closeList.Clear();

        var route = new List<MapCellData>();
        if (finded)
        {//找到后将路线存入路线集合  
            MapCellData hex = targetHexagon;
            while (hex != thisHexagon)
            {
                route.Add(hex);//将节点添加到路径列表里  

                var fatherHex = hex.getFatherHexagon();//从目标节点开始搜寻父节点就是所要的路线  
                hex = fatherHex;
            }
            route.Add(hex);


        }
        route.Reverse();
        return route;
    }

    public static List<MapCellData> openList = new List<MapCellData>();
    public static List<MapCellData> closeList = new List<MapCellData>();
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
