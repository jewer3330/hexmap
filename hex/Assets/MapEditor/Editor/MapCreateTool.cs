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
        var s = GUILayout.SelectionGrid(selectBrushBase, window.previewBases(out objs, MapCellData.BuildingType.Floor), 4);
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
        var data = defaultBrush.data;

            int id = 0;
            if (!int.TryParse(defaultBrush.name, out id))
            {
                Debug.LogError("错误，名称不为数字");
            }
            data.luaTableID = id;
            if (data.buildingType == MapCellData.BuildingType.Building)
            {
                var path = "Table/duplicate/DuplicateFacilities";
                if (Main.LuaLoader == null)
                    Main.Init(Main.Mode.Editor);
                var t = Main.luaEnv.DoString(Main.LuaLoader(ref path));
                data.LoadFromLua((XLua.LuaTable)t[0]);
            }
            if (data.buildingType == MapCellData.BuildingType.Floor)
            {
                var path = "Table/duplicate/DuplicateFloor";
                if (Main.LuaLoader == null)
                    Main.Init(Main.Mode.Editor);
                var t = Main.luaEnv.DoString(Main.LuaLoader(ref path));
                data.LoadFromLua((XLua.LuaTable)t[0]);
            }
        
    }
}
