using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 帮助创建地图，清理垃圾，保姆类
/// </summary>
public class Medusa 
{
    /// <summary>
    /// 资源保存路径
    /// </summary>
    private static string mapSavePath = "Assets/Medusa/Editor/Data";
    /// <summary>
    /// 资源导出路径
    /// </summary>
    private static string exportPath = "Assets/Medusa/Editor/Export";

    /// <summary>
    /// 导出Lua配置
    /// </summary>
    private static string exportLuaPath = "Assets/Medusa/Editor/ExportLua";

    /// <summary>
    /// 事件预制体路径
    /// </summary>
    private static string brashPath = "/Medusa/Facilities";
    
    /// <summary>
    /// 地皮预制体路径
    /// </summary>
    private static string floorPath = "/Medusa/Floor";

    /// <summary>
    /// 名称
    /// </summary>
    public string mapName = string.Empty;

    /// <summary>
    /// 地图宽度
    /// </summary>
    public int mapWidth = 10;

    /// <summary>
    /// 地图高度
    /// </summary>
    public int mapHeight = 10;

    /// <summary>
    /// 地图信息
    /// </summary>
    public MapData map;

   

    /// <summary>
    /// 垃圾回收
    /// </summary>
    private List<GameObject> garbage = new List<GameObject>();


    /// <summary>
    /// 默认笔刷
    /// </summary>
    public HexBrush defaultBrush;

    private bool genRes;

    /// <summary>
    /// 加载地图
    /// </summary>
    public void Load(bool genRes = true)
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

        var map = AssetDatabase.LoadAssetAtPath<MapData>(path);

        if (map == null)
        {
            Debug.LogError("地图数据错误");
            return;
        }
        if (map)
            this.map = Object.Instantiate(map);
        mapWidth = map.mapWidth;
        mapHeight = map.mapHeight;
        this.genRes = genRes;
        Link();
        if (genRes)
        {
            this.map.hexs = new Hex[mapWidth * mapHeight * (map.layerCount + 1)];
            ChangeDefaultHex(map.cells);

            var layerSelect = 0;
            var layers = GetLayers();
            if (layers != null && layers.Length != 0)
            {
                foreach (var k in layers)
                {
                    ActiveLayer(k, false);
                }
                ActiveLayer(layers[layerSelect], true);
            }
        }
    }


    public IGrouping<int, MapCellData>[] GetLayers()
    {
        return map?.cells.GroupBy(r => r.z).ToArray();
    }

    /// <summary>
    /// 激活层级
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="l"></param>
    /// <param name="active"></param>
    public void ActiveLayer(IGrouping<int, MapCellData> layer, bool active)
    {
        var cells = layer.ToList();
        for (int i = 0; i < cells.Count; i++)
        {
            var id = map.HexPositionToIndex(cells[i].x, cells[i].y, cells[i].z);
            map.cells[id].active = active;
            if (genRes)
            {
                if (map.hexs[id])
                    map.hexs[id].gameObject.SetActive(active);
            }
        }
    }

    /// <summary>
    /// 添加一个层级
    /// </summary>
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
       

       
        Array.Copy(map.cells, cells, map.cells.Length);
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
            }
        }

       
        map.cells = cells;
        if (genRes)
        {
            var hexs = new Hex[size * count];
            Array.Copy(map.hexs, hexs, map.hexs.Length);


            var t_hexs = new List<Hex>();
            for (int j = 0; j < mapHeight; j++)
            {
                for (int i = 0; i < mapWidth; i++)
                {
                    var id = map.HexPositionToIndex(i, j, map.layerCount);
                    var data = cells[id];
                    var hex = InitHex(data);
                    hexs[id] = hex;
                    t_hexs.Add(hex);
                }
            }


            map.hexs = hexs;
            if (defaultBrush)
                ChangeHexsToBrushType(t_hexs, defaultBrush);
        }
    }

    /// <summary>
    /// 删除一个层级
    /// </summary>
    public void RemoveMapLayer(int l)
    {
        if (map.layerCount == 0)
        {
            Debug.LogError("最起码有一层吧？？");
            return;
        }
        int size = map.cells.Length;
        for (int i = size - 1; i >= 0; i--)
        {
            if (map.cells[i].z == l)
            {
                map.cells[i] = null;
                if (genRes)
                {
                    GameObject.DestroyImmediate(map.hexs[i].gameObject);
                    map.hexs[i] = null;
                }


            }
            else if (map.cells[i].z > l)
            {
                map.cells[i].z--;
                map.cells[i].id = map.HexPositionToIndex(map.cells[i].x, map.cells[i].y, map.cells[i].z);
            }

        }
        map.layerCount--;


        var left_cells = map.cells.Where(r => r != null).ToArray();
        if (genRes)
        {
            var left_hexs = map.hexs.Where(r => r != null).ToArray();
            map.hexs = left_hexs;
        }
        map.cells = left_cells;

        
    }


    /// <summary>
    /// 替换资源
    /// </summary>
    /// <param name="data"></param>
    public void ChangeDefaultHex(IList<MapCellData> data)
    {
        for (int i = 0; i < data.Count; i++)
        {
            var hex = InitHex(data[i]);

            ChangeHex(hex, data[i]);
        }
    }

    public Hex InitHex(MapCellData data)
    {
        var position = new Vector2(data.x, data.y);
        var pos = World.ToWorldPos(position);
        var hex = new GameObject();
        var hm = hex.AddComponent<Hex>();
        hm.data = data;
        hm.HexPosition = position;
        hex.transform.position = pos;
        hm.InitializeModel();
        garbage.Add(hex);
        hm.HexModel.meshRenderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

        return hm;
    }

    /// <summary>
    /// 生成资源
    /// </summary>
    /// <param name="hex"></param>
    /// <param name="data"></param>
    public void ChangeHex(Hex hex, MapCellData data)
    {
        if (!string.IsNullOrEmpty(data.res))
        {
            var o = AssetDatabase.LoadAssetAtPath<GameObject>(data.res);
            if (o)
            {
                var brush = o.GetComponent<HexBrush>();
                var newHex = ChangeGameObjectToBrushType(hex, brush);
                if (data.buildingType == MapCellData.Catalogue.Event)
                {
                    GenBuildingRes(newHex);
                }
            }
        }
    }

    /// <summary>
    /// 生成事件资源
    /// </summary>
    /// <param name="hex"></param>
    public void GenBuildingRes(Hex hex)
    {
        if (!string.IsNullOrEmpty(hex.data.buildingRes) && hex.data.buildingType == MapCellData.Catalogue.Event)
        {
            var go = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(hex.data.buildingRes));
            go.transform.SetParent(hex.transform, false);
            go.transform.localPosition = Vector3.up * Globals.OffsetScale;

            GameObject.DestroyImmediate(go.GetComponent<HexBrush>());
            var building = go.AddComponent<HexBuilding>();
            building.hex = hex;
        }
    }

    /// <summary>
    /// 将hex替换为笔刷类型
    /// </summary>
    /// <param name="hex"></param>
    /// <param name="brush"></param>
    /// <returns></returns>
    public Hex ChangeGameObjectToBrushType(Hex hex, HexBrush brush)
    {

        GameObject go = GameObject.Instantiate(brush.gameObject) as GameObject;
        HexBrush hb = go.GetComponent<HexBrush>();
        go.transform.position = hex.transform.position;
        Hex newHex = go.AddComponent<Hex>();


        newHex.data = hex.data.Clone();
        newHex.data.walkType = hb.data.walkType;
        go.name = newHex.data.id.ToString();

        newHex.HexPosition = new Vector2(newHex.data.x, newHex.data.y);
        garbage.Add(go);

        var index = map.HexPositionToIndex(hex.data.x, hex.data.y, hex.data.z);
        map.hexs[index] = newHex;
        map.cells[index] = newHex.data;
        GameObject.DestroyImmediate(hex.gameObject);
        GameObject.DestroyImmediate(hb);
        return newHex;
    }

    /// <summary>
    /// 保存地图信息
    /// </summary>
    public void Save()
    {

        string mapName = EditorUtility.SaveFilePanelInProject("保存", "new.asset", "asset", "保存地图配置", mapSavePath);
        if (string.IsNullOrEmpty(mapName))
        {
            Debug.LogError("地图名称是空");
            return;
        }
        MapData newMap = GameObject.Instantiate(map);
        newMap.cells = this.map.cells
            .Where(r => r != null)
            .Select(r => r.Clone()).ToArray();
        newMap.hexs = null;
        var path = mapName.Substring(mapName.IndexOf("Assets/"));
        mapName = path;
        var fileName = Path.GetFileNameWithoutExtension(mapName);
        AssetDatabase.DeleteAsset(mapName);
        AssetDatabase.CreateAsset(newMap, mapName);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

    }

    /// <summary>
    /// 清理垃圾
    /// </summary>
    public void Clean()
    {
        mapName = null;
        map = null;
        foreach (var k in garbage)
        {
            if (k)
                GameObject.DestroyImmediate(k);
        }

        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 新建画刷
    /// </summary>
    public void NewBrush()
    {
        BrushCreateTool window = (BrushCreateTool)EditorWindow.GetWindow(typeof(BrushCreateTool));
        window.medusa = this;
        window.Show();
    }

    /// <summary>
    /// 导出
    /// </summary>

    public void Export()
    {
        string path = EditorUtility.SaveFilePanel("保存", exportPath, "1", "xml");
        var name = Path.GetFileNameWithoutExtension(path);
        Export(name);
        ExportLua(name);
    }
    /// <summary>
    /// 导出到文件
    /// </summary>
    /// <param name="fileName"></param>
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

            if (k.buildingType == MapCellData.Catalogue.Event)
            {
                XmlElement e = xmldoc.CreateElement("Node");
                SerializeEventNode(eventRoot, k, e);
            }
        }

        xmldoc.Save(Application.dataPath + $"/{exportPath.Substring(exportPath.IndexOf("Medusa"))}/{fileName}.xml");
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 导出lua配置
    /// </summary>
    /// <param name="fileName"></param>
    public void ExportLua(string fileName)
    {
        string path = Application.dataPath + $"/{exportLuaPath.Substring(exportLuaPath.IndexOf("Medusa"))}/{fileName}.lua";
        using (FileStream fs = new FileStream(path, FileMode.Create))
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
                    sw.Write($"eventId = {(k.buildingType == MapCellData.Catalogue.Event ? (int)k.eventType : 0)},");
                    sw.WriteLine("},");
                }

                sw.WriteLine("}");
                sw.WriteLine("}");
                sw.WriteLine("return data");
            }
        }
        AssetDatabase.Refresh();
    }


    private void SerializeWalkalbeNode(XmlNode root, MapCellData data, XmlElement element)
    {
        element.SetAttribute("id", data.id.ToString());
        element.SetAttribute("x", data.x.ToString());
        element.SetAttribute("y", data.y.ToString());
        element.SetAttribute("z", data.z.ToString());

        root.AppendChild(element);
    }



    private void SerializeEventNode(XmlNode root, MapCellData data, XmlElement element)
    {

        element.SetAttribute("id", data.id.ToString());
        element.SetAttribute("x", data.x.ToString());
        element.SetAttribute("y", data.y.ToString());
        element.SetAttribute("z", data.z.ToString());
        element.SetAttribute("eventId", ((int)data.eventType).ToString());
        root.AppendChild(element);
    }


    /// <summary>
    /// 预览笔刷
    /// </summary>
    /// <param name="type">笔刷类型</param>
    /// <param name="objs">笔刷预制体</param>
    /// <returns></returns>
    public GUIContent[] PreviewBases(MapCellData.Catalogue type,out UnityEngine.GameObject[] objs)
    {
        var path = floorPath;
        if (type == MapCellData.Catalogue.Event)
            path = brashPath;

        var files = System.IO.Directory.GetFiles(Application.dataPath + path, "*.prefab", System.IO.SearchOption.TopDirectoryOnly);
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
            .Select(r => new GUIContent(AssetPreview.GetAssetPreview(r), r.name)).ToArray();
        return ret;
    }

    /// <summary>
    /// 创建地图
    /// </summary>
    public void CreateMap(bool genRes = true)
    {
        map = ScriptableObject.CreateInstance<MapData>();
        this.genRes = genRes;
        map.cells = new MapCellData[mapHeight * mapWidth];
        if (genRes)
        {
            map.hexs = new Hex[mapHeight * mapWidth];
        }
        map.mapWidth = mapWidth;
        map.mapHeight = mapHeight;
        map.layerCount = 1;
        for (int j = 0; j < mapHeight; j++)
        {
            for (int i = 0; i < mapWidth; i++)
            {
                var data = new MapCellData();
                data.id = map.HexPositionToIndex(i, j, 0);
                data.x = i;
                data.y = j;
                data.z = 0;
                map.cells[data.id] = data;
                if (genRes)
                {
                    var hex = InitHex(data);
                    map.hexs[data.id] = hex;
                }
            }
        }
        Link();
    }

    /// <summary>
    /// 连接节点
    /// </summary>
    public void Link()
    {
        if (map)
        {
            foreach (var k in map.cells)
            {
                k.Link(map);
            }
        }
    }


    public void DeleteBuilding(HexBuilding building)
    {
        if (building.hex)
        {
            building.hex.data.buildingType = MapCellData.Catalogue.Floor;
            building.hex.data.buildingRes = string.Empty;
        }
    }

    public void DeleteHex(Hex hex)
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

    /// <summary>
    /// 替换成默认笔刷
    /// </summary>
    /// <param name="brush"></param>
    public void ChangeAllHexToBrushType(HexBrush brush)
    {
        defaultBrush = brush;
        var array = map.hexs;
        ChangeHexsToBrushType(array, defaultBrush);
    }

    /// <summary>
    /// 替换成默认笔刷
    /// </summary>
    /// <param name="brush"></param>
    public void ChangeAllCellToBrushType(HexBrush brush)
    {
        defaultBrush = brush;
        var array = map.cells;
        foreach (var k in array)
        {
            ChangeCell(k, defaultBrush);
        }
    }


    void ChangeHexsToBrushType(IList<Hex> hexs, HexBrush brush)
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


    void ParseData(Hex hex, HexBrush brush)
    {

    }

    public void AddBuildToHexWithUndo(Hex hex, HexBrush brush)
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

    /// <summary>
    /// 替换各自数据，目前只有2种属性需要替换
    /// </summary>
    /// <param name="data"></param>
    /// <param name="brush"></param>
    public void ChangeCell(MapCellData data, HexBrush brush)
    {
        data.eventType = brush.data.eventType;
        data.walkType = brush.data.walkType;
        data.buildingType = brush.data.buildingType;
        data.cost = brush.data.cost;
    }

    public void GenBuildingResWithUndo(Hex hex)
    {
        if (!string.IsNullOrEmpty(hex.data.buildingRes))
        {
            var go = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(hex.data.buildingRes));
            go.transform.SetParent(hex.transform, false);
            go.transform.localPosition = Vector3.up * Globals.OffsetScale;
            Undo.RegisterCreatedObjectUndo(go, hex.data.buildingRes);
            Undo.DestroyObjectImmediate(go.GetComponent<HexBrush>());
            var building = Undo.AddComponent<HexBuilding>(go);
            Undo.RecordObject(building, building.name);
            building.hex = hex;
        }
    }


    public void ChangeGameObjectToBrushTypeWithUndo(Hex hex, HexBrush brush)
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
        Undo.RecordObject(map, "mapData");
        var index = hex.data.id;
        map.hexs[index] = newHex;
        map.cells[index] = newHex.data;

        Undo.DestroyObjectImmediate(hex.gameObject);
        Undo.DestroyObjectImmediate(hb);
    }
}
