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

    public Medusa medusa;

    [MenuItem("Tools/Medusa Editor &m")]
    static void Init()
    {
        MedusaToolWindow window = (MedusaToolWindow)GetWindow(typeof(MedusaToolWindow));
        if (window.medusa == null)
            window.medusa = new Medusa();
        window.Show();


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
                    medusa.DeleteBuilding(k);
                }
                var hex = Selection.GetFiltered<Hex>(SelectionMode.TopLevel);
                foreach (var k in hex)
                {
                    medusa.DeleteHex(k);
                }
            }
        }
    }






    private GameObject currentSelect;
    private bool draw;
    private Vector2 scrollPosition;


    private int start = 0;
    private int width => 300;
    private int startY = 0;

    private Rect topSize => new Rect(start, startY, width, 20);
    private Rect toolBarSize => new Rect(330, 0, width, 20);

    private Rect brushSize => new Rect((this.position.width - width * 2) * 0.5f, 20, width * 2, 80);

    private Rect brushInfoSize => BrushInfo ? new Rect(start, startY + 20, width, 300) : new Rect(start, startY + 20, 150, 20);
    private Rect infoSize => Info ? new Rect(this.position.width - width, startY + topSize.height + 150, width, 300) : new Rect(this.position.width - 80, startY + topSize.height + 150, width, 20);
    private Rect layerSize => GUILayer ? new Rect(this.position.width - width, infoSize.y + infoSize.height + 10, 300, 360) : new Rect(this.position.width - 80, infoSize.y + infoSize.height + 10, 300, 20);

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

    private int currentTag = -1;
    void OnBaseCellBrush(int type)
    {
        if (type != currentTag)
        {
            selectBrushBase = -1;
            currentTag = type;
        }
        UnityEngine.GameObject[] objs = null;
        var content = medusa.PreviewBases((MapCellData.HasEvent)type, out objs);
        var s = GUILayout.SelectionGrid(selectBrushBase, content, 15, GUILayout.Width(600), GUILayout.Height(30));
        if (s != selectBrushBase)
        {
            Selection.objects = null;
            selectBrushBase = s;
            currentSelect = objs[s];
        }

    }


    private bool BrushInfo = true;
    void OnGUIBrushInfo(Rect size)
    {
        EditorGUI.DrawRect(size, Color.gray * 0.7f);
        GUILayout.BeginArea(size);
        BrushInfo = EditorGUILayout.BeginFoldoutHeaderGroup(BrushInfo, "地图信息");
        if (BrushInfo)
        {
            EditorGUILayout.LabelField(string.Format("Name:{0},{1} x {2}", medusa.mapName, medusa.mapWidth, medusa.mapHeight));
            EditorGUILayout.ObjectField(medusa.map, typeof(MapData), false);
            EditorGUILayout.LabelField("笔刷信息");
            if (currentSelect)
            {
                HexBrush hb = currentSelect.GetComponent<HexBrush>();
                medusa.defaultBrush = hb;
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

                        MapCellTool.Draw(cell.data);
                    }

                    HexBuilding build = k.gameObject.GetComponent<HexBuilding>();
                    if (build)
                    {
                        if (build.hex)
                        {
                            GUI.color = Color.red;
                            EditorGUILayout.LabelField("选中了事件");
                            GUI.color = Color.white;
                            MapCellTool.Draw(build.hex.data);
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        GUILayout.EndArea();
    }


    protected override void OnGUI()
    {

        base.OnGUI();

        OnGUIToolbar(toolBarSize);
        OnGUIBrush(brushSize);
        OnGUIBrushInfo(brushInfoSize);
        OnGUIInfo(infoSize);
        OnGUILayer(layerSize);



    }


    private void OnLayer(IGrouping<int, MapCellData> layer, int l)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("当前层级" + l.ToString());
        medusa.ActiveLayer(layer, true);
        if (GUILayout.Button("删除"))
        {
            medusa.RemoveMapLayer(l);
            layerSelect = 0;
        }

        GUILayout.EndHorizontal();
    }

    private Vector2 layerScroll;
    private int layerSelect = 0;
    private int defaultBrushIndex;
    private bool GUILayer = true;
    private void OnGUILayer(Rect size)
    {

        EditorGUI.DrawRect(size, Color.gray * 0.7f);
        GUILayout.BeginArea(size);

        GUILayer = EditorGUILayout.BeginFoldoutHeaderGroup(GUILayer, "新增层级");
        //EditorGUILayout.LabelField("层级");

        if (GUILayer)
        {

            layerScroll = GUILayout.BeginScrollView(layerScroll);
            var layers = medusa.GetLayers();
            if (layers != null)
            {
                var contents = layers.Select(r => new GUIContent("第" + r.Key.ToString() + "层")).ToArray();
                var s = GUILayout.SelectionGrid(layerSelect, contents, 1);
                if (s != layerSelect)
                {
                    medusa.ActiveLayer(layers[layerSelect], false);
                    layerSelect = s;
                }
            }
            GUILayout.EndScrollView();
            if (layers != null && layers.Length > layerSelect && layerSelect >= 0)
                OnLayer(layers[layerSelect], layerSelect);
            GUILayout.Space(10);
            EditorGUILayout.LabelField("新增层级");
            EditorGUILayout.LabelField("选择默认地基");
            //defaultBrush = (HexBrush)EditorGUILayout.ObjectField(defaultBrush, typeof(HexBrush), false);
            GameObject[] objs = null;
            var sel = GUILayout.SelectionGrid(defaultBrushIndex, medusa.PreviewBases(MapCellData.HasEvent.None, out objs), 4);
            var defaultBrush = medusa.defaultBrush;
            if (defaultBrush == null)
            {
                defaultBrush = objs[0].GetComponent<HexBrush>();
                medusa.defaultBrush = defaultBrush;

            }
            if (sel != defaultBrushIndex)
            {
                defaultBrushIndex = sel;
                defaultBrush = objs[sel].GetComponent<HexBrush>();
                medusa.defaultBrush = defaultBrush;
            }

            if (GUILayout.Button("+"))
            {
                medusa.AddMapLayer();

            }
        }
        GUILayout.EndArea();
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

        var s = GUILayout.Toolbar(select, toolbars, GUILayout.Width(30 * toolbars.Length), GUILayout.Height(20));
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
                    MapCreateTool.Open(medusa);
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
        medusa.Export();
    }
    private void NewBrush()
    {
        medusa.NewBrush();
    }

    private void Clean()
    {
        medusa.Clean();
    }

    private void Load()
    {
        medusa.Load();
    }

    void Save()
    {
        medusa.Save();
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

    void ChangeGameObjectType(GameObject[] gos)
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
                        medusa.ChangeGameObjectToBrushTypeWithUndo(cell, hb);
                    }
                    else
                    {
                        medusa.AddBuildToHexWithUndo(cell, hb);//画刷是事件
                    }
                }
                if (k)
                {
                    HexBuilding building = k.GetComponent<HexBuilding>();
                    if (building && building.hex) //当前选中的是事件
                    {
                        if (hb.data.buildingType == MapCellData.HasEvent.Has)//画刷是事件
                        {
                            medusa.AddBuildToHexWithUndo(building.hex, hb);
                        }
                        else if (hb.data.buildingType == MapCellData.HasEvent.None)//画刷是Floor，那就替换地面了
                        {
                            medusa.ChangeGameObjectToBrushTypeWithUndo(building.hex, hb);
                        }
                    }
                }
            }
        }
    }










}
