using UnityEngine;
using System.Collections.Generic;




public class MapData : ScriptableObject
{
    public Hex[] hexs;
    public MapCellData[] cells;
    public int mapWidth;
    public int mapHeight;
    public int layerCount;
    public int HexPositionToIndex(int x, int y,int z)
    {
        int size = mapWidth * mapHeight;
        return z * size + y * mapWidth + x;
    }
}
