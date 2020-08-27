using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Hex))]
public class MapCellTool : Editor {

    private GUIStyle style;

    //void OnSceneGUI()
    //{
    //    if (null == style)
    //    {
    //        style = new GUIStyle();
    //        style.fontSize = 40;
    //    }

    //    Hex cell = (Hex)target;
    //    if (cell == null)
    //    {
    //        return;
    //    }

    //    Handles.color = Color.red;
        
    //    Handles.BeginGUI();
    //    var rect = new Rect(10, 10, 200, 400);
    //    EditorGUI.DrawRect(rect, Color.black * 0.1f);
    //    GUILayout.BeginArea(rect);
    //    Draw(cell);
    //    GUILayout.EndArea();
    //    Handles.EndGUI();


    //}

    public static void Draw(Hex cell)
    {
        EditorGUILayout.LabelField("id", cell.data.id.ToString());
        EditorGUILayout.LabelField(string.Format("pos ({0},{1})", cell.data.x.ToString(), cell.data.y.ToString()));

        OnPropertyChange(cell);

        
    }

    public static void Draw(MapCellData data)
    {
        EditorGUILayout.LabelField("id", data.id.ToString());
        EditorGUILayout.LabelField(string.Format("pos ({0},{1})", data.x.ToString(), data.y.ToString()));
        
        data.walkType = (MapCellData.WalkType)EditorGUILayout.EnumPopup("类型", data.walkType);
        if (data.buildingType == MapCellData.BuildingType.Building)
            data.eventType = (MapCellData.EventType)EditorGUILayout.EnumPopup("事件类型", data.eventType);

    }

    public static void OnPropertyChange(Hex cell)
    {
        if (!cell) 
            return;
        if (null == cell.data)
            return;
        cell.data.walkType = (MapCellData.WalkType)EditorGUILayout.EnumPopup("类型", cell.data.walkType);
        if(cell.data.buildingType == MapCellData.BuildingType.Building)
            cell.data.eventType = (MapCellData.EventType)EditorGUILayout.EnumPopup("事件类型", cell.data.eventType);
        //if (!cell.HexModel)
        //    return;
        //if (!cell.HexModel.meshRenderer)
        //    return;
        //switch (cell.data.type)
        //{
        //    case MapCellData.Type.UnWalkable:
        //        cell.HexModel.meshRenderer.sharedMaterial.color = Color.black;
        //        break;
        //    case MapCellData.Type.Walkable:
        //        cell.HexModel.meshRenderer.sharedMaterial.color = Color.white;
        //        break;
        //    case MapCellData.Type.Start:
        //        cell.HexModel.meshRenderer.sharedMaterial.color = Color.green;
        //        break;
        //    case MapCellData.Type.End:
        //        cell.HexModel.meshRenderer.sharedMaterial.color = Color.red;
        //        break;
        //}
    }

   

}
