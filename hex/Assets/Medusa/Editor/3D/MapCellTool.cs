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

    public static void DrawBrush(MapCellData data)
    {
        data.walkType = (MapCellData.WalkType)EditorGUILayout.EnumPopup("是否可以通行", data.walkType);
        data.buildingType = (MapCellData.HasEvent)EditorGUILayout.EnumPopup("是否包含事件", data.buildingType);
        if (data.buildingType == MapCellData.HasEvent.Has)
            data.eventType = (MapCellData.EventType)EditorGUILayout.EnumPopup("事件类型", data.eventType);



    }

    public static void OnPropertyChange(MapCellData cell)
    {
        if (null == cell) 
            return;
        
        EditorGUILayout.LabelField("是否事件", cell.buildingType.ToString());
        EditorGUILayout.LabelField("是否可走", cell.walkType.ToString());
        EditorGUILayout.LabelField("地基资源", cell.res.ToString());
        EditorGUILayout.LabelField("事件资源", cell.buildingRes.ToString());
        if (cell.buildingType == MapCellData.HasEvent.Has)
            EditorGUILayout.LabelField("事件类型", cell.eventType.ToString());
       
    }

   

}
