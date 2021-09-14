using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Linq;
using System.Xml;
using System.IO;
public class MedusaToolWindow : SceneView
{
    static string mapSavePath = "Assets/Medusa/Editor/Data";
    static string exportPath = "Assets/Medusa/Editor/Export";
    [MenuItem("Tools/Medusa Editor &m")]
    static void Init()
    {
        MedusaToolWindow window = (MedusaToolWindow)GetWindow(typeof(MedusaToolWindow));
        window.Show();
        
        //window.titleContent = new GUIContent("MapTool");
        duringSceneGui += window.OnScene;

        window.minSize = new Vector2(1440, 900);
        window.Start();
        
    }



    public void Start()
    {
       
    }

    private void OnScene(SceneView sceneView)
    {
        Repaint();
        if (Event.current != null)
        {
            if (Event.current.keyCode == KeyCode.Delete)
            {

                var hexBuilding = Selection.GetFiltered<HexBuilding>(SelectionMode.TopLevel);
                foreach (var k in hexBuilding)
                {
                    OnDeleteBuilding(k);
                }
                var hex = Selection.GetFiltered<Hex>(SelectionMode.TopLevel);
                foreach(var k in hex)
                {
                    OnDeleteHex(k);
                }
            }
        }
    }

    private void OnDeleteBuilding(HexBuilding building)
    {
        if (building.hex)
        {
            building.hex.data.buildingType = MapCellData.HasEvent.None;
            building.hex.data.buildingRes = string.Empty;
        }
    }

    private void OnDeleteHex(Hex hex)
    {
        if (map.cells[hex.data.id] != null)
        {
            map.cells[hex.data.id] = null;
        }
        if (map.hexs[hex.data.id])
        {
            map.hexs[hex.data.id] = null;
        }
    }



    private string mapName = string.Empty;
    public int mapWidth = 10;
    public int mapHeight = 10;
    private List<GameObject> garbage = new List<GameObject>();
    private GameObject currentSelect;
    private Vector2 scrollPosition;
    private bool draw;

    private MapData map;
    private int start = 0;
    private int width => 300;
    private int startY = 0;
    //private Rect view => new Rect(start, 20, width, 300);

    //private Rect viewRight => new Rect(view.width, 20, this.position.width - view.width, this.position.height);
    private Rect topSize => new Rect(start, startY, width, 20);
    private Rect toolBarSize => new Rect(330, 0, width, 20);

    private Rect brushSize => new Rect((this.position.width - width * 2) * 0.5f, 20 , width * 2, 80);

    private Rect brushInfoSize => BrushInfo ? new Rect(start, startY + 20, width, 300) : new Rect(start, startY + 20, 150, 20);
    private Rect infoSize => Info? new Rect(this.position.width - width , startY  + topSize.height  + 150 , width, 300) : new Rect(this.position.width - 80, startY + topSize.height + 150, width, 20);
    private Rect layerSize => GUILayer ? new Rect(this.position.width - width,infoSize.y + infoSize.height + 10, 300, 360) : new Rect(this.position.width - 80, infoSize.y + infoSize.height + 10, 300, 20);

    private string[] tabs = new string[] { "地基", "事件" };
    private TabView tabview;
    void OnGUIBrush(Rect size)
    {


        EditorGUI.DrawRect(size, Color.gray * 0.7f);
        GUILayout.BeginArea(size);
        bool ret = EditorGUILayout.ToggleLeft("绘制", draw);
        if (ret != draw)
        {
            draw = ret;
            Selection.objects = null;
        }

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

    private const string brashPath = "/Medusa/Facilities";
    private const string floorPath = "/Medusa/Floor";
    public GUIContent[] previewBases(out UnityEngine.GameObject[] objs, MapCellData.HasEvent type)
    {
        var path = floorPath;
        if (type == MapCellData.HasEvent.Has)
            path = brashPath;

        var files = System.IO.Directory.GetFiles(Application.dataPath + path, "*.prefab",System.IO.SearchOption.TopDirectoryOnly);
        objs = files
            .Select((r) =>
                {
                    var p = r.Substring(r.IndexOf("Assets"));
                    var obj = AssetDatabase.LoadAssetAtPath(p, typeof(GameObject));
                    return (GameObject)obj; 
                })
            .Select(r => r.GetComponent<HexBrush>())
            .Where(r => r.data.buildingType == type)
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
        var content = previewBases(out objs, (MapCellData.HasEvent)type);
        var s =  GUILayout.SelectionGrid(selectBrushBase, content, 15, GUILayout.Width(600), GUILayout.Height(30));
        if (s != selectBrushBase)
        {
            Selection.objects = null;
            selectBrushBase = s;
            currentSelect = objs[s];
        }
        //if (type == 0)
        //{
        //    defaultBrush = objs[1].GetComponent<HexBrush>();
        //}
    }


    private bool BrushInfo = true;
    void OnGUIBrushInfo(Rect size)
    {
        EditorGUI.DrawRect(size, Color.gray * 0.7f);
        GUILayout.BeginArea(size);
        BrushInfo = EditorGUILayout.BeginFoldoutHeaderGroup(BrushInfo, "地图信息");
        if (BrushInfo)
        {
            EditorGUILayout.LabelField(string.Format("Name:{0},{1} x {2}", mapName, mapWidth, mapHeight));
            EditorGUILayout.ObjectField(map, typeof(MapData), false);
            EditorGUILayout.LabelField("笔刷信息");
            if (currentSelect)
            {
                HexBrush hb = currentSelect.GetComponent<HexBrush>();
                MapCreateTool.InitBrush(hb);
                if (hb)
                {
                    EditorGUILayout.LabelField(hb.name);
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
        EditorGUI.DrawRect(size, Color.gray * 0.7f);
        GUILayout.BeginArea(size);
       
        Info = EditorGUILayout.BeginFoldoutHeaderGroup(Info, "详细信息");
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
                            GUI.color = Color.red;
                            EditorGUILayout.LabelField("选中了事件");
                            GUI.color = Color.white;
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

    //private GUIStyle fontStyle;

    protected override void OnGUI()
    {
        //if (Event.current != null)
        //    draw = Event.current.shift;
        //if (null == fontStyle)
        //{
        //    fontStyle = new GUIStyle() { fontSize = 40 };
        //    fontStyle.normal.textColor = Color.red;
        //}
        base.OnGUI();
        //EditorGUI.DrawRect(view, Color.gray *0.9f);
        OnGUIMenu(topSize);
        OnGUIToolbar(toolBarSize);
        OnGUIBrush(brushSize);
        OnGUIBrushInfo(brushInfoSize);
        OnGUIInfo(infoSize);
        OnGUILayer(layerSize);
        //flag = EditorGUILayout.ObjectField("flagObject", flag, typeof(GameObject), true) as GameObject;
        //draw = EditorGUILayout.ToggleLeft("draw ?", draw);
        //currentSelect = EditorGUILayout.ObjectField("select go", currentSelect, typeof(GameObject), true) as GameObject;

        //if (draw)
        //{
        //    GUILayout.BeginArea(viewRight);
        //    GUILayout.BeginHorizontal();
        //    GUILayout.Space(400);
        //    //GUILayout.Label(new GUIContent("绘制中"), fontStyle);
        //    GUILayout.EndHorizontal();
        //    GUILayout.EndArea();
        //}

        
    }

   

    //  %表示ctrl     #表示shift    &表示alt  
    public override void AddItemsToMenu(GenericMenu menu)
    {
       
        menu.AddItem(new GUIContent("File/Save"), false, Save);
        menu.AddItem(new GUIContent("File/Open"), false, Load);

        menu.AddItem(new GUIContent("Edit/New"), false, () => { MapCreateTool.Open(this); });
        menu.AddItem(new GUIContent("Edit/Delete"), false, Clean);
        menu.AddItem(new GUIContent("Edit/NewBrush"), false, NewBrush);
        menu.AddItem(new GUIContent("Edit/Export"), false, Export);
        base.AddItemsToMenu(menu);
    }

    private void OnGUIMenu(Rect size)
    {
        GUILayout.BeginArea(size);
        GUILayout.BeginHorizontal();
        
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void ActiveLayer(IGrouping<int, MapCellData> layer,int l, bool active)
    {
        var cells = layer.ToList();
        for (int i = 0; i < cells.Count; i++)
        {
            var id = map.HexPositionToIndex(cells[i].x, cells[i].y, cells[i].z);
            if(map.hexs[id])
                map.hexs[id].gameObject.SetActive(active);
        }
    }

    private void OnLayer(IGrouping<int,MapCellData> layer,int l)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("当前层级" + l.ToString());
        ActiveLayer(layer,l,true);
        if (GUILayout.Button("删除"))
        {
            if (map.layerCount == 0)
            {
                Debug.LogError("最起码有一层吧？？");
                return;
            }
            int size = map.hexs.Length;
            for (int i = size - 1; i >= 0; i--)
            {
                if (map.cells[i].z == l)
                {
                    map.cells[i] = null;
                    GameObject.DestroyImmediate(map.hexs[i].gameObject);
                    map.hexs[i] = null;


                }
                else if (map.cells[i].z > l)
                {
                    map.cells[i].z--;
                    map.cells[i].id = map.HexPositionToIndex(map.cells[i].x, map.cells[i].y, map.cells[i].z);
                }
                
            }
            map.layerCount--;


            var left_cells = map.cells.Where(r => r != null).ToArray();
            var left_hexs = map.hexs.Where(r => r != null).ToArray();

            map.hexs = left_hexs;
            map.cells = left_cells;

            //if (layerSelect > map.layerCount)
            //{
                layerSelect = 0;
            //}
        }

        GUILayout.EndHorizontal();
    }

    private Vector2 layerScroll;
    private int layerSelect = 0;
    private int defaultBrushIndex;
    private bool GUILayer;
    private void OnGUILayer(Rect size)
    {
        
        EditorGUI.DrawRect(size, Color.gray * 0.7f);
        GUILayout.BeginArea(size);

        GUILayer = EditorGUILayout.BeginFoldoutHeaderGroup(GUILayer, "新增层级");
        //EditorGUILayout.LabelField("层级");

        if (GUILayer)
        {
            if (map)
            {
                layerScroll = GUILayout.BeginScrollView(layerScroll);
                var layers = map.cells.GroupBy(r => r.z).ToArray();
                var contents = layers.Select(r => new GUIContent("第" + r.Key.ToString() + "层")).ToArray();
                var s = GUILayout.SelectionGrid(layerSelect, contents, 1);
                if (s != layerSelect)
                {
                    ActiveLayer(layers[layerSelect], layerSelect, false);
                    layerSelect = s;
                }
                GUILayout.EndScrollView();
                if (layers.Length > layerSelect && layerSelect >= 0)
                    OnLayer(layers[layerSelect], layerSelect);
            }
            GUILayout.Space(10);
            EditorGUILayout.LabelField("新增层级");
            EditorGUILayout.LabelField("选择默认地基");
            //defaultBrush = (HexBrush)EditorGUILayout.ObjectField(defaultBrush, typeof(HexBrush), false);
            GameObject[] objs = null;
            var sel = GUILayout.SelectionGrid(defaultBrushIndex, previewBases(out objs, MapCellData.HasEvent.None), 4);
            if (defaultBrush == null)
            {
                defaultBrush = (objs[0] as GameObject).GetComponent<HexBrush>();
                MapCreateTool.InitBrush(defaultBrush);
            }
            if (sel != defaultBrushIndex)
            {
                defaultBrushIndex = sel;
                defaultBrush = (objs[sel] as GameObject).GetComponent<HexBrush>();
                MapCreateTool.InitBrush(defaultBrush);
            }

            if (GUILayout.Button("+"))
            {
                AddMapLayer();

            }
        }
        GUILayout.EndArea();
    }


    public void AddMapLayer()
    {
        if (!map)
        {
            Debug.LogError("请先创建");
            return;
        }
        map.layerCount++;
        int count = map.layerCount + 1;
        int size = map.mapWidth * map.mapHeight;
        var cells = new MapCellData[size * count];
        var hexs = new Hex[size * count];

        Array.Copy(map.hexs, hexs, map.hexs.Length);
        Array.Copy(map.cells, cells, map.cells.Length);
        var t_hexs = new List<Hex>();
        for (int j = 0; j < mapHeight; j++)
        {
            for (int i = 0; i < mapWidth; i++)
            {
                var data = new MapCellData();
                data.id = map.HexPositionToIndex(i, j, map.layerCount);
                data.x = i;
                data.y = j;
                data.z = map.layerCount;
                cells[data.id] = data;
                var hex = InitHex(data);
                hexs[data.id] = hex;
                t_hexs.Add(hex);
            }
        }
        
        map.hexs = hexs;
        map.cells = cells;
        if (defaultBrush)
            ChangeHexsToBrushType(t_hexs, defaultBrush);
    }

    public GUIContent[] toolbars => new GUIContent[]{
        new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(string.Format("Assets/Medusa/Editor/Icons/{0}","folder-line.png")),"Open"),
        new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(string.Format("Assets/Medusa/Editor/Icons/{0}","save-line.png")),"Save"),
        new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(string.Format("Assets/Medusa/Editor/Icons/{0}","creative-commons-line.png")),"Create"),
        new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(string.Format("Assets/Medusa/Editor/Icons/{0}","delete-bin-line.png")),"Delete"),
        new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(string.Format("Assets/Medusa/Editor/Icons/{0}","brush-2-line.png")),"NewBrush"),
        new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(string.Format("Assets/Medusa/Editor/Icons/{0}","external-link-line.png")),"Export"),

        
    };
    int select = -1;
    private void OnGUIToolbar(Rect size)
    {
        GUILayout.BeginArea(size);
        
        var s = GUILayout.Toolbar(select, toolbars,GUILayout.Width(30 * toolbars.Length) ,GUILayout.Height(20));
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
        doc.SetAttribute("layers", map.layerCount.ToString());
        var walkableRoot = root.AppendChild(xmldoc.CreateElement("WalkableNodes"));
        var eventRoot = root.AppendChild(xmldoc.CreateElement("EventNodes"));


        foreach (var k in map.cells)
        {
            if (k.walkType == MapCellData.WalkType.Walkable)
            {
                XmlElement e = xmldoc.CreateElement("Node");
                SerializeWalkalbeNode(walkableRoot, k, e);
            }
            
            if(k.buildingType == MapCellData.HasEvent.Has)
            {
                XmlElement e = xmldoc.CreateElement("Node");
                SerializeEventNode(eventRoot, k, e);
            }
        }

        xmldoc.Save(Application.dataPath + $"/Medusa/Editor/Export/{fileName}.xml");
        //ExportLua(fileName);
        AssetDatabase.Refresh();
    }

    public void ExportLua(string fileName)
    {
        string path = Application.dataPath + $"/LuaScripts/src/Dungeon/Dungeon{fileName}.lua";
        using (FileStream fs = new FileStream(path,FileMode.Create))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine("-- auto created by Medusa editor DO NOT motify");
                sw.WriteLine($"-- created time {DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}");
                sw.WriteLine("local data = {");

                sw.WriteLine($"width = {mapWidth},");
                sw.WriteLine($"height = {mapHeight},");
                sw.WriteLine($"layers = {map.layerCount},");
                sw.WriteLine("walkableNodes = {");

                foreach (var k in map.cells)
                {
                    sw.Write($"[{k.id}] = {{");
                    sw.Write($"id = {k.id},");
                    sw.Write($"x = {k.x},");
                    sw.Write($"y = {k.y},");
                    sw.Write($"z = {k.z},");
                    sw.Write($"walkType = {(int)k.walkType},");
                    sw.Write($"buildingType = {(int)k.buildingType},");
                    sw.Write($"res = '{k.res}',");
                    sw.Write($"buildingRes = '{k.buildingRes}',");
                    sw.Write($"eventId = {(k.buildingType == MapCellData.HasEvent.Has ? k.eventType : 0)},");
                    sw.WriteLine("},");
                }

                sw.WriteLine("}");
                sw.WriteLine("}");
                sw.WriteLine("return data");
            }
        }
        AssetDatabase.Refresh();
    }
 

    public void SerializeWalkalbeNode(XmlNode root,MapCellData data, XmlElement element)
    {
        element.SetAttribute("id", data.id.ToString());
        element.SetAttribute("x", data.x.ToString());
        element.SetAttribute("y", data.y.ToString());
        element.SetAttribute("z", data.z.ToString());

        root.AppendChild(element);
    }



    public void SerializeEventNode(XmlNode root, MapCellData data, XmlElement element)
    {

        element.SetAttribute("id", data.id.ToString());
        element.SetAttribute("x", data.x.ToString());
        element.SetAttribute("y", data.y.ToString());
        element.SetAttribute("z", data.z.ToString());
        element.SetAttribute("eventId", ((int)data.eventType).ToString());
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
        

        string path = EditorUtility.OpenFilePanel("打开", mapSavePath, "asset");
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("地图名称是空");
            return;
        }
        Clean();
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
        this.map.hexs = new Hex[mapWidth * mapHeight * (map.layerCount + 1)];
        ChangeDefaultHex(map.cells);

        layerSelect = 0;
        var layers = map.cells.GroupBy(r => r.z).ToArray();
        if (layers != null && layers.Length != 0)
        {
            foreach (var k in layers)
            {
                ActiveLayer(k, k.Key, false);
            }
            ActiveLayer(layers[layerSelect], layerSelect, false);
        }
    }

    void ChangeHex(Hex hex, MapCellData data)
    {
        if (!string.IsNullOrEmpty(data.res))
        {
            if (data.buildingType == MapCellData.HasEvent.None)
            {
                var o = AssetDatabase.LoadAssetAtPath<GameObject>(data.res);
                if (o)
                {
                    var brush = o.GetComponent<HexBrush>();
                    //MapCreateTool.InitBrush(brush);
                    var newHex = ChangeGameObjectToBrushType(hex, brush);
                    //GenBuildingRes(newHex);
                }
            }
            if (data.buildingType == MapCellData.HasEvent.Has)
            {
                var o = AssetDatabase.LoadAssetAtPath<GameObject>(data.res);
                if (o)
                {
                    var brush = o.GetComponent<HexBrush>();
                    //MapCreateTool.InitBrush(brush);
                    var newHex = ChangeGameObjectToBrushType(hex, brush);
                    GenBuildingRes(newHex);
                }
            }
        }
    }

    void ChangeDefaultHex(IList<MapCellData> data)
    {
        for (int i = 0; i < data.Count; i++)
        {
            var hex = InitHex(data[i]);

            ChangeHex(hex, data[i]);
        }
    }
    protected HexBrush defaultBrush;
    public void ChangeAllHexToBrushType(HexBrush brush)
    {
        defaultBrush = brush;
        var array = map.hexs;
        ChangeHexsToBrushType(array, defaultBrush);
    }

    public void ChangeHexsToBrushType(IList<Hex> hexs, HexBrush brush)
    {
        var array = hexs;
        for (int i = 0; i < array.Count; i++)
        {
            var hex = array[i];
            hex.data.res = AssetDatabase.GetAssetPath(brush);
            hex.data.buildingRes = string.Empty;
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
        MapData newMap = GameObject.Instantiate(map);
        newMap.cells = this.map.cells
            .Where(r=> r != null)
            .Select(r=>r.Clone()).ToArray();
        newMap.hexs = null;
        //newMap.mapHeight = mapHeight;
        //newMap.mapWidth = mapWidth;
        //newMap.layerCount = map.layerCount;
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
        
       
        //MapCellTool.OnPropertyChange(hm);
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
                data.id = map.HexPositionToIndex(i, j, 0);
                data.x = i;
                data.y = j;
                data.z = height;
                //data.TableID =  int.Parse(name);
                map.cells[data.id] = data;
                var hex = InitHex(data);
                map.hexs[data.id] = hex;
            }
        }
    }

    private new void OnDestroy()
    {
        Clean();
        base.OnDestroy();
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
                    if (hb.data.buildingType == MapCellData.HasEvent.None) //画刷是地面
                    {
                        ChangeGameObjectToBrushTypeWithUndo(cell, hb);
                    }
                    else
                    {
                        AddBuildToHexWithUndo(cell, hb);//画刷是事件
                    }
                }
                if (k)
                {
                    HexBuilding building = k.GetComponent<HexBuilding>();
                    if (building && building.hex) //当前选中的是事件
                    {
                        if (hb.data.buildingType == MapCellData.HasEvent.Has)//画刷是事件
                        {
                            AddBuildToHexWithUndo(building.hex, hb);
                        }
                        else if (hb.data.buildingType == MapCellData.HasEvent.None)//画刷是Floor，那就替换地面了
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
            //hex.data.eventType = brush.data.eventType;
            hex.data.walkType = brush.data.walkType;
            ParseData(hex, brush);
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
            go.transform.localPosition = Vector3.up * Globals.OffsetScale;
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
            //hex.data.eventType = brush.data.eventType;
            hex.data.walkType = brush.data.walkType;
            ParseData(hex, brush);

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
        if (!string.IsNullOrEmpty(hex.data.buildingRes) && hex.data.buildingType == MapCellData.HasEvent.Has)
        {
            var go = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(hex.data.buildingRes));
            go.transform.SetParent(hex.transform, false);
            go.transform.localPosition = Vector3.up * Globals.OffsetScale;

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
        go.name = newHex.data.id.ToString();
        //newHex.data.res = AssetDatabase.GetAssetPath(brush);

        newHex.HexPosition = new Vector2(newHex.data.x, newHex.data.y);
        ParseData(newHex, brush);
        garbage.Add(go);

        var index = map.HexPositionToIndex(hex.data.x, hex.data.y,hex.data.z);
        map.hexs[index]  = newHex;
        map.cells[index] = newHex.data;
        GameObject.DestroyImmediate(hex.gameObject);
        GameObject.DestroyImmediate(hb);
        return newHex;
    }

    void ParseData(Hex hex, HexBrush brush)
    {
        //hex.data.luaTableID = brush.data.luaTableID;
        //hex.data.TableEffect = brush.data.TableEffect;
        //hex.data.TableInfomation = brush.data.TableInfomation;
        //hex.data.TableName = brush.data.TableName;
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
        //newHex.data.eventType = hb.data.eventType;
        newHex.data.res = AssetDatabase.GetAssetPath(brush);
        newHex.data.buildingRes = string.Empty;
        newHex.HexPosition = new Vector2(newHex.data.x, newHex.data.y);
        ParseData(newHex, brush);
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
