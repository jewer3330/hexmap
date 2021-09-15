using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapCreateTool : EditorWindow
{
    private Medusa medusa;
    private HexBrush defaultBrush;
    private int selectBrushBase = 0;

    public static MapCreateTool Open(Medusa p)
    {
        MapCreateTool window = (MapCreateTool)EditorWindow.GetWindow(typeof(MapCreateTool));
        window.Show();
        window.medusa = p;
        return window;
    }

    private void OnGUI()
    {
        medusa.mapWidth = EditorGUILayout.IntField("mapWidth", medusa.mapWidth);
        medusa.mapHeight = EditorGUILayout.IntField("mapHeight", medusa.mapHeight);

        defaultBrush =(HexBrush) EditorGUILayout.ObjectField(defaultBrush, typeof(HexBrush), false);
        GameObject[] objs = null;
        var s = GUILayout.SelectionGrid(selectBrushBase, medusa.PreviewBases(MapCellData.HasEvent.None,out objs), 4);
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
            medusa.Clean();
            medusa.CreateMap();
            if(defaultBrush)
                medusa.ChangeAllHexToBrushType(defaultBrush);
        }
    }

    public void InitBrush(HexBrush defaultBrush)
    {
        medusa.defaultBrush = defaultBrush;
    }
}
