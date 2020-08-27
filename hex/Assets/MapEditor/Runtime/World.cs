using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class World : MonoBehaviour {
    public int gridX = 15;
    public int gridY = 15;
    public Dictionary<Vector2, Hex> Hexes { get; set; }

    public void Generate()
    {
        Hexes = new Dictionary<Vector2, Hex>();

        for (int j = 0; j < gridX; j++)
        {
            for (int i = 0; i < gridY; i++)
            {
                var position = new Vector2(i, j);
                var pos = ToPixel(position);

                var hex = new GameObject();
                hex.AddComponent<Hex>();
                var hm = hex.GetComponent<Hex>();
                hm.HexPosition = position;
                hex.transform.position = pos;
                //hex.transform.parent = transform.FindChild("HexHolder");
                hm.InitializeModel();
                this.Hexes.Add(position, hm);
            }
        }

        foreach(KeyValuePair<Vector2, Hex> kv in Hexes)
        {
            kv.Value.Link(this);
        }
    }
	
	void Update () 
    {
	
	}

    #region support
    public static Vector2 ToHex(Vector3 pos)
    {
        var px = pos.x + Globals.HalfWidth;
        var py = pos.z + Globals.Radius;

        int gridX = (int)Math.Floor(px / Globals.Width);
        int gridY = (int)Math.Floor(py / Globals.RowHeight);

        float gridModX = Math.Abs(px % Globals.Width);
        float gridModY = Math.Abs(py % Globals.RowHeight);

        bool gridTypeA = (gridY % 2) == 0;

        var resultY = gridY;
        var resultX = gridX;
        var m = Globals.ExtraHeight / Globals.HalfWidth;

        if (gridTypeA)
        {
            // middle
            resultY = gridY;
            resultX = gridX;
            // left
            if (gridModY < (Globals.ExtraHeight - gridModX * m))
            {
                resultY = gridY - 1;
                resultX = gridX - 1;
            }
            // right
            else if (gridModY < (-Globals.ExtraHeight + gridModX * m))
            {
                resultY = gridY - 1;
                resultX = gridX;
            }
        }
        else
        {
            if (gridModX >= Globals.HalfWidth)
            {
                if (gridModY < (2 * Globals.ExtraHeight - gridModX * m))
                {
                    // Top
                    resultY = gridY - 1;
                    resultX = gridX;
                }
                else
                {
                    // Right
                    resultY = gridY;
                    resultX = gridX;
                }
            }

            if (gridModX < Globals.HalfWidth)
            {
                if (gridModY < (gridModX * m))
                {
                    // Top
                    resultY = gridY - 1;
                    resultX = gridX;
                }
                else
                {
                    // Left
                    resultY = gridY;
                    resultX = gridX - 1;
                }
            }
        }

        return new Vector3(resultX, resultY);
    }

    public static Vector3 ToPixel(Vector2 hc)
    {
        var x = (hc.x * Globals.Width) + (((int)hc.y & 1) * Globals.Width / 2);
        return new Vector3(x, 0, (float)(hc.y * 1.5 * Globals.Radius));
    }

    public IEnumerable<Vector2> GetRing(Vector2 hcrd, int ring)
    {
        var left = new Vector2(hcrd.x - ring, hcrd.y);
        yield return left;

        var tx = left.x;
        var ty = left.y;
        for (var i = 1; i < ring + 1; i++)
        {
            tx = NextX(tx, ty);
            ty = ty + 1;
            yield return new Vector2(tx, ty);
        }

        var bx = left.x;
        var by = left.y;
        for (var i = 1; i < ring + 1; i++)
        {
            bx = NextX(bx, by);
            by = by - 1;
            yield return new Vector2(bx, by);
        }

        for (int i = 1; i <= ring; i++)
        {
            yield return new Vector2(tx + i, ty);
            yield return new Vector2(bx + i, by);
        }

        tx += ring;
        bx += ring;
        for (var i = 1; i < ring; i++)
        {
            tx = NextX(tx, ty);
            ty = ty - 1;
            yield return new Vector2(tx, ty);
        }
        for (var i = 1; i < ring; i++)
        {
            bx = NextX(bx, by);
            by = by + 1;
            yield return new Vector2(bx, by);
        }

        yield return new Vector2(hcrd.x + ring, hcrd.y);
    }

    private int NextX(float x, float y)
    {
        if (y % 2 == 0) return (int)x;
        else return (int)(x + 1);
    }
    #endregion









    public static List<Hex> searchRoute(Hex thisHexagon, Hex targetHexagon)
    {
        Hex nowHexagon = thisHexagon;
        //nowHexagon.reset();

        openList.Add(nowHexagon);
        bool finded = false;
        while (!finded)
        {
            openList.Remove(nowHexagon);//将当前节点从openList中移除  
            closeList.Add(nowHexagon);//将当前节点添加到关闭列表中  
            Hex[] neighbors = nowHexagon.neighbors;//获取当前六边形的相邻六边形  
            //print("当前相邻节点数----" + neighbors.size());  
            foreach (Hex neighbor in neighbors)
            {
                if (neighbor == null) continue;

                if (neighbor == targetHexagon)
                {//找到目标节点  
                    //System.out.println("找到目标点");  
                    finded = true;
                    neighbor.setFatherHexagon(nowHexagon);
                }
                if (closeList.Contains(neighbor) )//|| !neighbor.canPass())
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
                        openList.Remove(neighbor);//重新排序该节点在openList的位置  
                        neighbor.setgValue(assueGValue);//从新设置g估值  
                        openList.Add(neighbor);//从新排序openList。  
                    }
                }
                else
                {//没有在开启列表里  
                    //print("不在开启列表，添加");  
                    neighbor.sethValue(neighbor.computeHValue(targetHexagon));//计算好他的h估值  
                    neighbor.setgValue(neighbor.computeGValue(nowHexagon) + nowHexagon.getgValue());//计算该节点的g估值（到当前节点的g估值加上当前节点的g估值）  
                    openList.Add(neighbor);//添加到开启列表里  
                    neighbor.setFatherHexagon(nowHexagon);//将当前节点设置为该节点的父节点  
                }
            }

            if (openList.Count <= 0)
            {
                //print("无法到达该目标");  
                break;
            }
            else
            {
                nowHexagon = openList[0];//得到f估值最低的节点设置为当前节点  
            }
        }
        openList.Clear();
        closeList.Clear();

        List<Hex> route = new List<Hex>();
        if (finded)
        {//找到后将路线存入路线集合  
            Hex hex = targetHexagon;
            while (hex != thisHexagon)
            {
                route.Add(hex);//将节点添加到路径列表里  

                Hex fatherHex = hex.getFatherHexagon();//从目标节点开始搜寻父节点就是所要的路线  
                hex = fatherHex;
            }
            route.Add(hex);


        }
        route.Reverse();
        return route;
        //      resetMap();  
    }

    public static List<Hex> openList = new List<Hex>();
    public static List<Hex> closeList = new List<Hex>();

    //通过无阻挡寻路确定两个六边形的距离
    public static int GetRouteDis(Hex thisHexagon, Hex targetHexagon)
    {
        Hex nowHexagon = thisHexagon;
        //nowHexagon.reset();

        openList.Add(nowHexagon);
        bool finded = false;
        while (!finded)
        {
            openList.Remove(nowHexagon);//将当前节点从openList中移除  
            closeList.Add(nowHexagon);//将当前节点添加到关闭列表中  
            Hex[] neighbors = nowHexagon.neighbors;//获取当前六边形的相邻六边形  
            //print("当前相邻节点数----" + neighbors.size());  
            foreach (Hex neighbor in neighbors)
            {
                if (neighbor == null) continue;

                if (neighbor == targetHexagon)
                {//找到目标节点  
                    //System.out.println("找到目标点");  
                    finded = true;
                    neighbor.setFatherHexagon(nowHexagon);
                }
                if (closeList.Contains(neighbor))
                {//在关闭列表里  
                    //System.out.println("无法通过或者已在关闭列表");  
                    continue;
                }

                if (openList.Contains(neighbor))
                {//该节点已经在开启列表里  
                    //System.out.println("已在开启列表，判断是否更改父节点");  
                    float assueGValue = neighbor.computeGValue(nowHexagon) + nowHexagon.getgValue();//计算假设从当前节点进入，该节点的g估值  
                    if (assueGValue < neighbor.getgValue())
                    {//假设的g估值小于于原来的g估值  
                        openList.Remove(neighbor);//重新排序该节点在openList的位置  
                        neighbor.setgValue(assueGValue);//从新设置g估值  
                        openList.Add(neighbor);//从新排序openList。  
                    }
                }
                else
                {//没有在开启列表里  
                    //System.out.println("不在开启列表，添加");  
                    neighbor.sethValue(neighbor.computeHValue(targetHexagon));//计算好他的h估值  
                    neighbor.setgValue(neighbor.computeGValue(nowHexagon) + nowHexagon.getgValue());//计算该节点的g估值（到当前节点的g估值加上当前节点的g估值）  
                    openList.Add(neighbor);//添加到开启列表里  
                    neighbor.setFatherHexagon(nowHexagon);//将当前节点设置为该节点的父节点  
                }
            }

            if (openList.Count <= 0)
            {
                //System.out.println("无法到达该目标");  
                break;
            }
            else
            {
                nowHexagon = openList[0];//得到f估值最低的节点设置为当前节点  
            }
        }
        openList.Clear();
        closeList.Clear();

        List<Hex> route = new List<Hex>();
        if (finded)
        {//找到后将路线存入路线集合  
            Hex hex = targetHexagon;
            while (hex != thisHexagon)
            {
                route.Add(hex);//将节点添加到路径列表里  

                Hex fatherHex = hex.getFatherHexagon();//从目标节点开始搜寻父节点就是所要的路线  
                hex = fatherHex;
            }
            route.Add(hex);


        }
        return route.Count - 1;
    }
}
