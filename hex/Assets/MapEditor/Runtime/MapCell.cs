//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//public class MapCell : MonoBehaviour
//{
//    public const int MaxStep = 100;
//    public MapCellData data = new MapCellData();
//    public int steps = MaxStep;
//    public MapCell prev;

//    //public MapCellIndicator indicator;

//    //public Player player;

//    public void SetData(MapCellData data)
//    {
//        this.data = data;
//    }

//    public int Cost
//    {
//        get
//        {
//            //if (!player)
//            //    return data.cost;
//            //if (player == LevelManager.selectPlayer)
//            //{
//            //    return data.cost;
//            //}
//            //else
//            //{
//            return MaxStep;
//            //}
//        }
//    }


//    public int ID
//    {
//        get
//        {
//            //if (data == null || !LevelManager)
//                return 0;
//            //return data.x + data.y * (LevelManager.mapWidth);
//        }
//    }

//    public void SetData(MapCell data)
//    {
//        SetData(data.data);
//    }

//    public bool IsSame(MapCell cell)
//    {
//        return data.x == cell.data.x && data.y == cell.data.y && data.h == cell.data.h;
//    }

//    public static float Distance(MapCell left, MapCell right)
//    {
//        if (left && right)
//        {
//            return Vector3.Distance(left.transform.position, right.transform.position);
//        }
//        return 0;
//    }

//    public float Distance(MapCell cell)
//    {
//        return Distance(this, cell);
//    }


//    //public LevelManager LevelManager
//    //{
//    //    get;set;
//    //}

//    //public MapCell Left
//    //{
//    //    get
//    //    {
//    //        if (_left)
//    //            return _left;
//    //        if (LevelManager == null || data.x == 0)
//    //            return null;
//    //        _left = LevelManager.GetData(data.x - 1, data.y);
//    //        return _left;
//    //    }
//    //}

//    //public MapCell Right
//    //{
//    //    get
//    //    {
//    //        if (_right)
//    //            return _right;
//    //        if (LevelManager == null || data.x == LevelManager.mapWidth - 1)
//    //            return null;
//    //        _right = LevelManager.GetData(data.x + 1, data.y);
//    //        return _right;
//    //    }
//    //}


//    //public MapCell Top
//    //{
//    //    get
//    //    {
//    //        if (_top)
//    //            return _top;
//    //        if (LevelManager == null || data.y == LevelManager.mapHeight - 1)
//    //            return null;
//    //         _top =LevelManager.GetData(data.x , data.y + 1);
//    //        return _top;
//    //    }
//    //}

//    //public MapCell Bottom
//    //{
//    //    get
//    //    {
//    //        if (_bottom)
//    //            return _bottom;
//    //        if (LevelManager == null || data.y == 0)
//    //            return null;
//    //        _bottom = LevelManager.GetData(data.x, data.y - 1);
//    //        return  _bottom;
//    //    }
//    //}

//    private MapCell _bottom;
//    private MapCell _top;
//    private MapCell _left;
//    private MapCell _right;

//    private readonly List<MapCell> cells = new List<MapCell>();

//    public List<MapCell> GetCloseCells()
//    {
//        if(cells.Count == 0)
//        {
//            //if (Left != null)
//            //{
//            //    cells.Add(Left);
//            //}
//            //if (Right != null)
//            //{
//            //    cells.Add(Right);
//            //}
//            //if (Top != null)
//            //{
//            //    cells.Add(Top);
//            //}
//            //if (Bottom != null)
//            //{
//            //    cells.Add(Bottom);
//            //}
//        }

//        return cells;
//    }

//    private void OnDrawGizmos()
//    {
//        Gizmos.color = Color.red;
//        if (prev)
//        {
//            var end = prev.transform.position + 2 * Vector3.up;
//            var start = transform.position + 2 * Vector3.up;
//            var fwd = (end - start).normalized;
//            Gizmos.DrawLine(start, end);
//            Gizmos.DrawLine(start, start + Quaternion.Euler(0,45,0)  * fwd * 0.2f);
//            Gizmos.DrawLine(start, start + Quaternion.Euler(0,-45,0) * fwd * 0.2f);
//        }
//    }

//}
