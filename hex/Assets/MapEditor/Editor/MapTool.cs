using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Linq;
using System.Xml;
using System.IO;
//using SuperBoBo;

public class MapTool : EditorWindow
{
    static string mapSavePath = "Assets/MapEditor/Editor/Data";
    static string exportPath = "Assets/MapEditor/Editor/Export";
    [MenuItem("Tools/地图编辑器")]
    static void Init()
    {
        MapTool window = (MapTool)EditorWindow.GetWindow(typeof(MapTool));
        window.Show();
        SceneView.duringSceneGui += window.OnSceneFunc;

        window.minSize = new Vector2(300, 400);
        window.Start();
        
    }



    public void Start()
    {
       
    }

    private void OnSceneFunc(SceneView sceneView)
    {
        Repaint();
    }





    private string mapName = string.Empty;
    public int mapWidth = 10;
    public int mapHeight = 10;
    private List<GameObject> garbage = new List<GameObject>();
    private GameObject currentSelect;
    private Vector2 scrollPosition;
    private bool draw;

    private MapData map;
    private Rect topSize => new Rect(0, 0, this.position.width, 20);
    private Rect toolBarSize => new Rect(0, topSize.height, this.position.width, 40);

    private Rect brushSize => new Rect(0, toolBarSize.height + topSize.height, this.position.width, 240);

    private Rect brushInfoSize => new Rect(0, toolBarSize.height + topSize.height + brushSize.height, this.position.width, (this.position.height - topSize.height - toolBarSize.height - brushSize.height) * 0.5f);
    private Rect infoSize => new Rect(0, toolBarSize.height + topSize.height + brushSize.height + brushInfoSize.height, this.position.width, (this.position.height - topSize.height - toolBarSize.height - brushSize.height) * 0.5f);

    private string[] tabs = new string[] { "地基", "建筑" };
    private TabView tabview;
    void OnGUIBrush(Rect size)
    {
        GUILayout.BeginArea(size);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(string.Format("Name:{0},{1} x {2}", mapName, mapWidth, mapHeight));
        EditorGUILayout.ObjectField(map, typeof(MapData), false);
        GUILayout.EndHorizontal();
        draw = EditorGUILayout.ToggleLeft("绘制", draw);
        if (tabview == null)
        {
            tabview = new TabView(tabs, size);
            tabview.Reg(0, OnBaseCellBrush);
            tabview.Reg(1, OnBaseCellBrush);
            tabview.Reg(2, OnBaseCellBrush);
        }
        tabview.OnGUI();
        GUILayout.EndArea();
    }

    private int selectBrushBase = -1;

    private const string brashPath = "Assets/MapEditor/BrushPrefabs";
    public GUIContent[] previewBases(out UnityEngine.GameObject[] objs,int type)
    {
        var files = System.IO.Directory.GetFiles(Application.dataPath + "/MapEditor/BrushPrefabs","*.prefab",System.IO.SearchOption.TopDirectoryOnly);
        objs = files
            .Select((r) =>
                {
                    var p = r.Substring(r.IndexOf("Assets"));
                    var obj = AssetDatabase.LoadAssetAtPath(p, typeof(GameObject));
                    return (GameObject)obj; 
                })
            .Select(r => r.GetComponent<HexBrush>())
            .Where(r => r.data.buildingType == (MapCellData.BuildingType)type)
            .Select(r => r.gameObject)
            .ToArray();
        var ret = objs
            .Select(r => new GUIContent( AssetPreview.GetAssetPreview(r),r.name)).ToArray();
        return ret;
    }
    private int currentTag = -1;
    void OnBaseCellBrush(int type)
    {
        if (type != currentTag)
        {
            selectBrushBase = -1;
            currentTag = type;
        }
        UnityEngine.GameObject[] objs = null;
        var s =  GUILayout.SelectionGrid(selectBrushBase, previewBases(out objs, type), 4);
        if (s != selectBrushBase)
        {
            selectBrushBase = s;
            currentSelect = objs[s];
        }
    }


    private bool BrushInfo = true;
    void OnGUIBrushInfo(Rect size)
    {
        GUILayout.BeginArea(size);
        BrushInfo = EditorGUILayout.BeginFoldoutHeaderGroup(BrushInfo, "BrushInfo");
        if (BrushInfo)
        {
            if (currentSelect)
            {
                HexBrush hb = currentSelect.GetComponent<HexBrush>();
                if (hb)
                {
                    MapCellTool.DrawBrush(hb.data);
                }
            }
           
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        GUILayout.EndArea();
    }

    private bool Info = true;


    void OnGUIInfo(Rect size)
    {
        GUILayout.BeginArea(size);
        Info = EditorGUILayout.BeginFoldoutHeaderGroup(Info, "Info");
        if (Info)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            if (Selection.gameObjects != null)
            {
                foreach (var k in Selection.gameObjects)
                {
                    Hex cell = k.gameObject.GetComponent<Hex>();
                    if (cell)
                    {

                        MapCellTool.Draw(cell);
                    }

                    HexBuilding build = k.gameObject.GetComponent<HexBuilding>();
                    if (build)
                    {
                        if (build.hex)
                        {
                            EditorGUILayout.LabelField("选中了建筑");
                            MapCellTool.Draw(build.hex);
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        GUILayout.EndArea();
    }

    private void OnGUI()
    {
        OnGUIMenu(topSize);
        OnGUIToolbar(toolBarSize);
        OnGUIBrush(brushSize);
        OnGUIBrushInfo(brushInfoSize);
        OnGUIInfo(infoSize);
        //flag = EditorGUILayout.ObjectField("flagObject", flag, typeof(GameObject), true) as GameObject;
        //draw = EditorGUILayout.ToggleLeft("draw ?", draw);
        //currentSelect = EditorGUILayout.ObjectField("select go", currentSelect, typeof(GameObject), true) as GameObject;





    }

    Menu fileMenu;
    Menu editorMenu;

    private void OnGUIMenu(Rect size)
    {
        GUILayout.BeginArea(size);
        GUILayout.BeginHorizontal();
        int width = 80;
        if (fileMenu == null)
        {
            fileMenu = new Menu("File", new Rect(size.x, 0, width, size.height));
            fileMenu.AddItem("Save", Save);
            fileMenu.AddItem("Open", Load);
        }
        fileMenu.OnGUI();
        if (editorMenu == null)
        {
            editorMenu = new Menu("Edit", new Rect(size.x + width, 0, width, size.height));
            editorMenu.AddItem("Create", ()=> { MapCreateTool.Open(this); });
            editorMenu.AddItem("Delete", Clean);
            editorMenu.AddItem("NewBrush", NewBrush);
            editorMenu.AddItem("Export", Export);
        }
        editorMenu.OnGUI();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    public GUIContent[] toolbars => new GUIContent[]{
        new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(string.Format("Assets/MapEditor/Editor/Icons/{0}","folder-line.png")),"Open"),
        new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(string.Format("Assets/MapEditor/Editor/Icons/{0}","save-line.png")),"Save"),
        new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(string.Format("Assets/MapEditor/Editor/Icons/{0}","creative-commons-line.png")),"Create"),
        new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(string.Format("Assets/MapEditor/Editor/Icons/{0}","delete-bin-line.png")),"Delete"),
        new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(string.Format("Assets/MapEditor/Editor/Icons/{0}","brush-2-line.png")),"NewBrush"),
        new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(string.Format("Assets/MapEditor/Editor/Icons/{0}","external-link-line.png")),"Export"),

        
    };
    int select = -1;
    private void OnGUIToolbar(Rect size)
    {
        GUILayout.BeginArea(size);
        
        var s = GUILayout.Toolbar(select, toolbars,GUILayout.Width(30 * toolbars.Length) ,GUILayout.Height(30));
        if (s != select)
        {
            select = s;
            switch (select)
            {
                case 0:
                    Load();
                    break;
                case 1:
                    Save();
                    break;
                case 2:
                    MapCreateTool.Open(this);
                    break;
                case 3:
                    Clean();
                    break;
                case 4:
                    NewBrush();
                    break;
                case 5:
                    Export();
                    break;
            }
            select = -1;
        }
        GUILayout.EndArea();
    }

    private void Export()
    {
        string path = EditorUtility.SaveFilePanel("保存", exportPath,"1" ,"xml");
        var name = Path.GetFileNameWithoutExtension(path);
        Export(name);
    }

    public void Export(string fileName)
    {
        var xmldoc = new XmlDocument();
        //加入XML的声明段落,<?xml version="1.0" encoding="gb2312"?>
        XmlDeclaration xmldecl;
        xmldecl = xmldoc.CreateXmlDeclaration("1.0", "utf-8", null);
        xmldoc.AppendChild(xmldecl);
        var doc = xmldoc.CreateElement("Root");
        var root = xmldoc.AppendChild(doc);
        doc.SetAttribute("width", mapWidth.ToString());
        doc.SetAttribute("height", mapHeight.ToString());
        var walkableRoot = root.AppendChild(xmldoc.CreateElement("WalkableNodes"));
        var eventRoot = root.AppendChild(xmldoc.CreateElement("EventNodes"));


        foreach (var k in map.cells)
        {
            if (k.walkType == MapCellData.WalkType.Walkable)
            {
                XmlElement e = xmldoc.CreateElement("Node");
                SerializeWalkalbeNode(walkableRoot, k, e);
            }
            
            if(k.eventType != MapCellData.EventType.None)
            {
                XmlElement e = xmldoc.CreateElement("Node");
                SerializeEventNode(eventRoot, k, e);
            }
        }

        xmldoc.Save(Application.dataPath + $"/MapEditor/Editor/Export/{fileName}.xml");

        AssetDatabase.Refresh();
    }

 

    public void SerializeWalkalbeNode(XmlNode root,MapCellData data, XmlElement element)
    {
        element.SetAttribute("id", data.id.ToString());
        element.SetAttribute("x", data.x.ToString());
        element.SetAttribute("y", data.y.ToString());
       
        root.AppendChild(element);
    }



    public void SerializeEventNode(XmlNode root, MapCellData data, XmlElement element)
    {

        element.SetAttribute("id", data.id.ToString());
        element.SetAttribute("x", data.x.ToString());
        element.SetAttribute("y", data.y.ToString());
        if (data.eventType == MapCellData.EventType.Door)
        {
            element.SetAttribute("eventName", data.eventType.ToString());
        }
        else if (data.eventType == MapCellData.EventType.Key)
        {
            element.SetAttribute("eventName", data.eventType.ToString());
        }
        else if (data.eventType == MapCellData.EventType.Reward)
        {
            element.SetAttribute("eventName", data.eventType.ToString());
        }
        else if (data.eventType == MapCellData.EventType.Start)
        {
            element.SetAttribute("eventName", data.eventType.ToString());
        }
        else if (data.eventType == MapCellData.EventType.End)
        {
            element.SetAttribute("eventName", data.eventType.ToString());
        }

        root.AppendChild(element);
    }

    public void NewBrush()
    {
        BrushCreateTool window = (BrushCreateTool)EditorWindow.GetWindow(typeof(BrushCreateTool));
        window.Show();
    }

    public void Clean()
    {
        mapName = null;
        map = null;
        foreach (var k in garbage)
        {
            if(k)
                GameObject.DestroyImmediate(k);
        }
       
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }

    private void Load()
    {
        Clean();

        string path = EditorUtility.OpenFilePanel("打开", mapSavePath, "asset");
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("地图名称是空");
            return;
        }
        path = path.Substring(path.IndexOf("Assets/"));
        mapName = path;

        //Debug.Log(path);
        var map = AssetDatabase.LoadAssetAtPath<MapData>(path);

        if (map == null)
        {
            Debug.LogError("地图数据错误");
            return;
        }
        if (map)
            this.map = Instantiate(map);
        mapWidth = map.mapWidth;
        mapHeight = map.mapHeight;
        this.map.hexs = new Hex[mapWidth * mapHeight];
        ChangeDefaultHex(map.cells);



    }

    void ChangeDefaultHex(IList<MapCellData> data)
    {
        for (int i = 0; i < data.Count; i++)
        {
            var hex = InitHex(data[i]);
            if (!string.IsNullOrEmpty(data[i].res))
            {
                var o = AssetDatabase.LoadAssetAtPath<GameObject>(data[i].res);
                if (o)
                {
                    var brush = o.GetComponent<HexBrush>();
                    var newHex = ChangeGameObjectToBrushType(hex, brush);
                    GenBuildingRes(newHex);
                }
            }

        }
    }

    public void ChangeAllHexToBrushType(HexBrush brush)
    {
        var array = map.hexs;
        for(int i = 0; i < array.Length;i++)
        {
            var hex = array[i];
            ChangeGameObjectToBrushType(hex, brush);
        }
    }


    void Save()
    {

        string mapName = EditorUtility.SaveFilePanelInProject("保存", "new.asset", "asset", "保存地图配置", mapSavePath);
        if (string.IsNullOrEmpty(mapName))
        {
            Debug.LogError("地图名称是空");
            return;
        }
        MapData newMap = ScriptableObject.CreateInstance<MapData>();
        newMap.cells = this.map.cells.Select(r=>r.Clone()).ToArray();
        newMap.hexs = null;
        newMap.mapHeight = mapHeight;
        newMap.mapWidth = mapWidth;
       
        var path = mapName.Substring(mapName.IndexOf("Assets/"));
        mapName = path;
        var fileName = Path.GetFileNameWithoutExtension(mapName);
        Export(fileName);
        AssetDatabase.DeleteAsset(mapName);
        AssetDatabase.CreateAsset(newMap, mapName);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        
    }

    Hex InitHex(MapCellData data)
    {
        var position = new Vector2(data.x, data.y);
        var pos = World.ToPixel(position);
        var hex = new GameObject();
        var hm = hex.AddComponent<Hex>();
        hm.data = data;
        hm.HexPosition = position;
        hex.transform.position = pos;
        hm.InitializeModel();
        garbage.Add(hex);
        hm.HexModel.meshRenderer.sharedMaterial = new Material(Shader.Find("Diffuse"));
        
       
        MapCellTool.OnPropertyChange(hm);
        return hm;
    }

    public void CreateMap(int height = 0)
    {
        map = ScriptableObject.CreateInstance<MapData>();
        map.cells = new MapCellData[mapHeight * mapWidth];
        map.hexs = new Hex[mapHeight * mapWidth];
        map.mapWidth = mapWidth;
        map.mapHeight = mapHeight;
        for (int j = 0; j < mapHeight; j++)
        {
            for (int i = 0; i < mapWidth; i++)
            {
                var data = new MapCellData();
                data.id = i + j * mapWidth;
                data.x = i;
                data.y = j;
                map.cells[data.id] = data;
                var hex = InitHex(data);
                map.hexs[data.id] = hex;
            }
        }
    }

    private void OnDestroy()
    {
        Clean();
    }

    void Update()
    {
        if (Selection.gameObjects != null && draw)
        {
            ChangeGameObjectType(Selection.gameObjects);
        }

        Repaint();
    }

    void ChangeGameObjectType(GameObject [] gos)
    {
       
        if (currentSelect != null)
        {
            foreach (var k in gos)
            {
                Hex cell = k.GetComponent<Hex>();
                HexBrush hb = currentSelect.GetComponent<HexBrush>();
                if (cell != null && cell.data.res != AssetDatabase.GetAssetPath(currentSelect))//选中是地表
                {
                    if (hb.data.buildingType == MapCellData.BuildingType.Floor) //画刷是地面
                    {
                        ChangeGameObjectToBrushTypeWithUndo(cell, hb);
                    }
                    else
                    {
                        AddBuildToHexWithUndo(cell, hb);//画刷是建筑
                    }
                }
                if (k)
                {
                    HexBuilding building = k.GetComponent<HexBuilding>();
                    if (building && building.hex) //当前选中的是建筑
                    {
                        if (hb.data.buildingType == MapCellData.BuildingType.Building)//画刷是建筑
                        {
                            AddBuildToHexWithUndo(building.hex, hb);
                        }
                        else if (hb.data.buildingType == MapCellData.BuildingType.Floor)//画刷是Floor，那就替换地面了
                        {
                            ChangeGameObjectToBrushTypeWithUndo(building.hex, hb);
                        }
                    }
                }
            }
        }
    }

    void AddBuildToHexWithUndo(Hex hex, HexBrush brush)
    {
        var res = AssetDatabase.GetAssetPath(brush.gameObject);
        if (res != hex.data.buildingRes)
        {
            Undo.RecordObject(hex, hex.name);
            hex.data.buildingRes = res;
            hex.data.buildingType = brush.data.buildingType;
            hex.data.eventType = brush.data.eventType;
            hex.data.walkType = brush.data.walkType;

            if (hex.transform.childCount == 1)
            {
                var trans = hex.transform.GetChild(0);
                Undo.DestroyObjectImmediate(trans.gameObject);
            }

            GenBuildingResWithUndo(hex);
        }
    }

    void GenBuildingResWithUndo(Hex hex)
    {
        if (!string.IsNullOrEmpty(hex.data.buildingRes))
        {
            var go = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(hex.data.buildingRes));
            go.transform.SetParent(hex.transform, false);
            go.transform.localPosition = Vector3.up;
            Undo.RegisterCreatedObjectUndo(go, hex.data.buildingRes);
            Undo.DestroyObjectImmediate(go.GetComponent<HexBrush>());
            var building = Undo.AddComponent<HexBuilding>(go);
            Undo.RecordObject(building, building.name);
            building.hex = hex;
        }
    }

    void AddBuildToHex(Hex hex,HexBrush brush)
    {
        var res = AssetDatabase.GetAssetPath(brush.gameObject);
        if (res != hex.data.buildingRes)
        {
            hex.data.buildingRes = res;
            hex.data.buildingType = brush.data.buildingType;
            hex.data.eventType = brush.data.eventType;
            hex.data.walkType = brush.data.walkType;

            if (hex.transform.childCount == 1)
            {
                var trans = hex.transform.GetChild(0);
                GameObject.DestroyImmediate(trans.gameObject);
            }

            GenBuildingRes(hex);
        }
    }

    void GenBuildingRes(Hex hex)
    {
        if (!string.IsNullOrEmpty(hex.data.buildingRes) && hex.data.buildingType == MapCellData.BuildingType.Building)
        {
            var go = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(hex.data.buildingRes));
            go.transform.SetParent(hex.transform, false);
            go.transform.localPosition = Vector3.up;

            GameObject.DestroyImmediate(go.GetComponent<HexBrush>());
            var building = go.AddComponent<HexBuilding>();
            building.hex = hex;
        }
    }

    Hex ChangeGameObjectToBrushType(Hex hex,HexBrush brush)
    {
        
        GameObject go = GameObject.Instantiate(brush.gameObject) as GameObject;
        HexBrush hb = go.GetComponent<HexBrush>();
        go.transform.position = hex.transform.position;
        Hex newHex = go.AddComponent<Hex>();

        
        newHex.data = hex.data.Clone();
        newHex.data.walkType = hb.data.walkType;
        newHex.data.res = AssetDatabase.GetAssetPath(brush);

        newHex.HexPosition = new Vector2(newHex.data.x, newHex.data.y);

        garbage.Add(go);

        var index = map.HexPositionToIndex(hex.data.x, hex.data.y);
        map.hexs[index]  = newHex;
        map.cells[index] = newHex.data;
        GameObject.DestroyImmediate(hex.gameObject);
        GameObject.DestroyImmediate(hb);
        return newHex;
    }

    void ChangeGameObjectToBrushTypeWithUndo(Hex hex, HexBrush brush)
    {
       
        GameObject go = GameObject.Instantiate(brush.gameObject) as GameObject;
        HexBrush hb = go.GetComponent<HexBrush>();
        go.transform.position = hex.transform.position;
        Hex newHex = go.AddComponent<Hex>();

        newHex.data = hex.data.Clone();
        newHex.data.walkType = hb.data.walkType;
        newHex.data.buildingType = hb.data.buildingType;
        newHex.data.eventType = hb.data.eventType;
        newHex.data.res = AssetDatabase.GetAssetPath(brush);

        newHex.HexPosition = new Vector2(newHex.data.x, newHex.data.y);

        Undo.RegisterCreatedObjectUndo(go, "HexBrushObject");


        this.garbage.Add(go);
        Undo.RecordObject(map,"mapData");
        var index = hex.data.id;
        map.hexs[index] = newHex;
        map.cells[index] = newHex.data;

        Undo.DestroyObjectImmediate(hex.gameObject);
        Undo.DestroyObjectImmediate(hb);
       


    }

}
