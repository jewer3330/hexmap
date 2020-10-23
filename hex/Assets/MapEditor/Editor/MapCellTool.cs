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

    public static void DrawBrush(MapCellData data)
    {
        EditorGUILayout.LabelField("是否可走", data.walkType.ToString());
        //data.buildingType = (MapCellData.BuildingType)EditorGUILayout.EnumPopup("建筑?", data.buildingType);
        EditorGUILayout.LabelField("建筑类型",data.buildingType.ToString());
        //EditorGUILayout.LabelField("地基资源",data.res.ToString());
        //EditorGUILayout.LabelField("建筑资源",data.buildingRes.ToString());
        //if (data.buildingType == MapCellData.BuildingType.设施)
        //    EditorGUILayout.LabelField("事件类型", data.eventType.ToString());

        if (data.luaTableID != 0)
        {
            EditorGUILayout.LabelField("表ID", data.luaTableID.ToString());
            if (data.buildingType == MapCellData.BuildingType.Building)
            {

                EditorGUILayout.LabelField("表名称", data.TableName);
                EditorGUILayout.LabelField("表简介", data.TableInfomation);
                EditorGUILayout.LabelField("表事件", data.TableEffect);

            }
            if (data.buildingType == MapCellData.BuildingType.Floor)
            {
                EditorGUILayout.LabelField("表名称", data.TableName);
            }

           
        }
    }

    public static void OnPropertyChange(Hex cell)
    {
        if (!cell) 
            return;
        if (null == cell.data)
            return;
        EditorGUILayout.LabelField("建筑类型", cell.data.buildingType.ToString());
        EditorGUILayout.LabelField("是否可走", cell.data.walkType.ToString());
        EditorGUILayout.LabelField("地基资源", cell.data.res.ToString());
        EditorGUILayout.LabelField("建筑资源", cell.data.buildingRes.ToString());
        //if (cell.data.buildingType == MapCellData.BuildingType.设施)
        //    EditorGUILayout.LabelField("事件类型", cell.data.eventType.ToString());

        var data = cell.data;
       
        if (data.luaTableID != 0)
        {
            EditorGUILayout.LabelField("表ID", data.luaTableID.ToString());
            if (data.buildingType == MapCellData.BuildingType.Building)
            {
                EditorGUILayout.LabelField("表名称", data.TableName);
                EditorGUILayout.LabelField("表简介", data.TableInfomation);
                EditorGUILayout.LabelField("表事件", data.TableEffect);

            }
            if (data.buildingType == MapCellData.BuildingType.Floor)
            {
                EditorGUILayout.LabelField("表名称", data.TableName);
            }


        }

    }

   

}
