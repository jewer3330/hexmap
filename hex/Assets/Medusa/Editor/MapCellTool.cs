using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MapCellTool : Editor {

    public static void Draw(Hex cell)
    {
        EditorGUILayout.LabelField("id", cell.data.id.ToString());
        EditorGUILayout.LabelField(string.Format("pos ({0},{1})", cell.data.x.ToString(), cell.data.y.ToString()));

        OnPropertyChange(cell);

        
    }

    public static void DrawBrush(MapCellData data)
    {
        data.walkType = (MapCellData.WalkType)EditorGUILayout.EnumPopup("是否可以通行", data.walkType);
        data.buildingType = (MapCellData.HasEvent)EditorGUILayout.EnumPopup("是否包含事件", data.buildingType);
        if (data.buildingType == MapCellData.HasEvent.Has)
            data.eventType = (MapCellData.EventType)EditorGUILayout.EnumPopup("事件类型", data.eventType);



    }

    public static void OnPropertyChange(Hex cell)
    {
        if (!cell) 
            return;
        if (null == cell.data)
            return;
        EditorGUILayout.LabelField("是否事件", cell.data.buildingType.ToString());
        EditorGUILayout.LabelField("是否可走", cell.data.walkType.ToString());
        EditorGUILayout.LabelField("地基资源", cell.data.res.ToString());
        EditorGUILayout.LabelField("事件资源", cell.data.buildingRes.ToString());
        if (cell.data.buildingType == MapCellData.HasEvent.Has)
            EditorGUILayout.LabelField("事件类型", cell.data.eventType.ToString());
       
    }

   

}
