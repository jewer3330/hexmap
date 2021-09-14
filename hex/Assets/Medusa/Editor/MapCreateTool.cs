using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapCreateTool : EditorWindow
{
    private MedusaToolWindow window;
    private HexBrush defaultBrush;
    private int selectBrushBase = 0;

    public static MapCreateTool Open(MedusaToolWindow p)
    {
        MapCreateTool window = (MapCreateTool)EditorWindow.GetWindow(typeof(MapCreateTool));
        window.Show();
        window.window = p;
        return window;
    }

    private void OnGUI()
    {
        window.mapWidth = EditorGUILayout.IntField("mapWidth", window.mapWidth);
        window.mapHeight = EditorGUILayout.IntField("mapHeight", window.mapHeight);

        defaultBrush =(HexBrush) EditorGUILayout.ObjectField(defaultBrush, typeof(HexBrush), false);
        GameObject[] objs = null;
        var s = GUILayout.SelectionGrid(selectBrushBase, window.previewBases(out objs, MapCellData.HasEvent.None), 4);
        if (defaultBrush == null)
        {
            defaultBrush = (objs[0] as GameObject).GetComponent<HexBrush>();
            InitBrush(defaultBrush);
        }
        if (s != selectBrushBase)
        {
            selectBrushBase = s;
            defaultBrush = (objs[s] as GameObject).GetComponent<HexBrush>();
            InitBrush(defaultBrush);
        }
        if (GUILayout.Button("Create"))
        {
            window.Clean();
            window.CreateMap();
            if(defaultBrush)
                window.ChangeAllHexToBrushType(defaultBrush);
        }
    }

    public static void InitBrush(HexBrush defaultBrush)
    {
        //var data = defaultBrush.data;

        //    int id = 0;
        //    if (!int.TryParse(defaultBrush.name, out id))
        //    {
        //        Debug.LogError("错误，名称不为数字");
        //    }
            
           
        
    }
}
