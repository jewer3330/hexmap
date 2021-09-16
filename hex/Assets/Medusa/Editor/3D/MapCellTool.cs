using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MapCellTool : Editor {

    public static void Draw(MapCellData cell)
    {
        EditorGUILayout.LabelField("id", cell.id.ToString());
        EditorGUILayout.LabelField(string.Format("pos ({0},{1})", cell.x.ToString(), cell.y.ToString()));

        OnPropertyChange(cell);

        
    }
    public static float minValue = 1f;
    public static float maxValue = 10f;
    public static void DrawBrush(MapCellData data)
    {
        data.walkType = (MapCellData.WalkType)EditorGUILayout.EnumPopup("是否可以通行", data.walkType);
        data.buildingType = (MapCellData.Catalogue)EditorGUILayout.EnumPopup("分类", data.buildingType);
        if (data.buildingType == MapCellData.Catalogue.Event)
            data.eventType = (MapCellData.EventType)EditorGUILayout.EnumPopup("事件类型", data.eventType);
        if (data.walkType == MapCellData.WalkType.Walkable)
        {
            EditorGUILayout.LabelField("阻挡值越大，代表越难通过");
            data.cost = EditorGUILayout.Slider("阻挡值", data.cost, minValue, maxValue);
        }
    }

    public static void OnPropertyChange(MapCellData cell)
    {
        if (null == cell) 
            return;
        
        EditorGUILayout.LabelField("分类", cell.buildingType.ToString());
        EditorGUILayout.LabelField("是否可走", cell.walkType.ToString());
        EditorGUILayout.LabelField("地基资源", cell.res.ToString());
        EditorGUILayout.LabelField("事件资源", cell.buildingRes.ToString());
        if (cell.buildingType == MapCellData.Catalogue.Event)
            EditorGUILayout.LabelField("事件类型", cell.eventType.ToString());
        if (cell.walkType == MapCellData.WalkType.Walkable)
        { 
            EditorGUILayout.LabelField("阻挡值", cell.cost.ToString());
        }
    }

   

}
