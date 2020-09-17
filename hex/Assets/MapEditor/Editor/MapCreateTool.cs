using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapCreateTool : EditorWindow
{
    private MapTool window;
    private HexBrush defaultBrush;
    private int selectBrushBase = 0;

    public static MapCreateTool Open(MapTool p)
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
        var s = GUILayout.SelectionGrid(selectBrushBase, window.previewBases(out objs,0), 3);
        if (defaultBrush == null)
        {
            defaultBrush = (objs[0] as GameObject).GetComponent<HexBrush>();
        }
        if (s != selectBrushBase)
        {
            selectBrushBase = s;
            defaultBrush = (objs[s] as GameObject).GetComponent<HexBrush>();
        }
        if (GUILayout.Button("Create"))
        {
            window.Clean();
            window.CreateMap();
            if(defaultBrush)
                window.ChangeAllHexToBrushType(defaultBrush);
        }
    }
}
