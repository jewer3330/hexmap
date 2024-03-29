﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MedusaToolWindow2D : EditorWindow
{

    private int startY = 0;

    private int width => 300;
    private int topHeight => 300;
    private Rect toolBarSize => new Rect(0, 0, width, 20);
    public Rect topSize => new Rect(0, 20, width, topHeight);
    public Rect topSizeBrush => new Rect(width + 10, 20, this.position.width - topSize.width - width * 2, 100);
    public Rect topSizeAstar => new Rect(width + 10, 20 + topSizeBrush.yMax, this.position.width - topSize.width - width * 2, 100);

    public Rect contentSize => new Rect(0, topSize.y + topSize.height + 10, this.position.width, this.position.height - topHeight - 10);

    private Rect infoSize => new Rect(clickPos.x, clickPos.y, width, 300);

    private Rect layerSize => GUILayer ? new Rect(this.position.width - width, startY, 300, topHeight) : new Rect(this.position.width - 80, startY, 300, 20);


    private Vector2 clickPos;

    public Medusa medusa;

    [MenuItem("Tools/Medusa Editor 2D &n")]
    static void Init()
    {
        MedusaToolWindow2D window = (MedusaToolWindow2D)GetWindow(typeof(MedusaToolWindow2D));
        if (window.medusa == null)
        {
            window.medusa = new Medusa();
        }
        window.Show();
        window.minSize = new Vector2(1440, 900);
        window.Start();

    }



    public void Start()
    {

    }

    private void OnGUI()
    {
        OnGUIToolbar(toolBarSize);
        if (medusa != null && medusa.map)
        {
           
            OnGUITopSize(topSize);
            OnGUIBrush(topSizeBrush);
            OnGUIAStart(topSizeAstar);
            OnGUILayer(layerSize);
            OnGUIMapcell(contentSize);
            OnGUIInfo(infoSize);
        }
    }

    int select = -1;
    public GUIContent[] toolbars => new GUIContent[]{
        new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(string.Format("Assets/Medusa/Editor/Icons/{0}","folder-line.png")),"Open"),
        new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(string.Format("Assets/Medusa/Editor/Icons/{0}","save-line.png")),"Save"),
        new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(string.Format("Assets/Medusa/Editor/Icons/{0}","creative-commons-line.png")),"Create"),
        new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(string.Format("Assets/Medusa/Editor/Icons/{0}","delete-bin-line.png")),"Delete"),
        new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(string.Format("Assets/Medusa/Editor/Icons/{0}","brush-2-line.png")),"NewBrush"),
        new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(string.Format("Assets/Medusa/Editor/Icons/{0}","external-link-line.png")),"Export"),


    };
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
                    medusa.Load(false);
                    break;
                case 1:
                    medusa.Save();
                    break;
                case 2:
                    MapCreateTool.Open(medusa,false);
                    break;
                case 3:
                    medusa.Clean();
                    break;
                case 4:
                    medusa.NewBrush();
                    break;
                case 5:
                    medusa.Export();
                    break;
            }
            select = -1;
        }
        GUILayout.EndArea();
    }

    private void OnDestroy()
    {
        if (medusa != null)
        {
            medusa.Clean();  
        }
    }


    private void OnGUITopSize(Rect size)
    {
        using (new RectScrope(size,"简介"))
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
    }
    private int scale = 10;
    private Vector2 offset;
    private MapCellData selectData;
    private void OnGUIMapcell(Rect size)
    {
       
        
        using (new RectScrope(size,"地图信息"))
        {

            showXY = EditorGUILayout.ToggleLeft("显示坐标轴", showXY);
            showPic = EditorGUILayout.ToggleLeft("显示背景参考", showPic);
            if (showPic)
            {
                OnGUIBackground(size);
            }
            sp = EditorGUILayout.ObjectField(sp, typeof(Texture), true, GUILayout.Width(width)) as Texture;
            if (showXY)
            {
                MapCellData.OnEditorXY(size, scale, offset);
            }
            

            if (Event.current.isScrollWheel)
            {
                scale += (int)Event.current.delta.y;
                scale = Mathf.Max(1, scale);
                Event.current.Use();
            }
            if (Event.current.isMouse && Event.current.button == 2)
            {
                offset += Event.current.delta;
                Event.current.Use();
                Repaint();
            }
            
            if (Event.current.isMouse && Event.current.button == 0) //鼠标左键
            {
                var screenP = Event.current.mousePosition;
                clickPos = screenP;
                var wp = MapCellData.ToWorld(screenP, scale, size, offset);
                var h = World.ToHex(wp);
                if (medusa != null && medusa.map)
                {
                    if (h.x < medusa.map.mapWidth && h.y < medusa.map.mapHeight)
                    {
                        var index = medusa.map.HexPositionToIndex((int)h.x, (int)h.y, layerSelect);
                        //Debug.Log(index);
                        selectData = medusa.map.cells[index];

                        if (draw && medusa.defaultBrush)
                        {
                            medusa.ChangeCell(selectData, medusa.defaultBrush);
                        }
                        if (astar)
                        {
                            Debug.Log("set first node wait for end " + index);
                            startIndex = index;
                        }
                    }
                }

                Event.current.Use();

            }


            if (Event.current.isMouse && Event.current.button == 1)//鼠标右键
            {
                var screenP = Event.current.mousePosition;

                var wp = MapCellData.ToWorld(screenP, scale, size, offset);
                var h = World.ToHex(wp);
                if (medusa != null && medusa.map)
                {
                    if (h.x < medusa.map.mapWidth && h.y < medusa.map.mapHeight)
                    {
                        var index = medusa.map.HexPositionToIndex((int)h.x, (int)h.y, layerSelect);
                            
                        if (astar)
                        {
                            Debug.Log("end set " + index);
                            endIndex = index;
                            
                        }
                    }
                }
                Event.current.Use();
            }



            if (medusa.map)
            {
                foreach (var k in medusa.map.cells)
                {
                    k.OnEditorGUI(size, scale,offset);
                }

                OnGUIPath(size);
            }
            
        }
    }

    private Texture sp;
    private void OnGUIBackground(Rect size)
    {
       
        if (sp)
        {
            var mapwidth = sp.width / 100f;
            var mapheight = sp.height / 100f; //realworld pos

            var p0 = MapCellData.ToArea(new Vector2(mapwidth, mapheight), scale, size, offset);
            var p1 = MapCellData.ToArea(Vector2.zero,scale,size,offset);
            var p2 = MapCellData.ToArea(new Vector2(0, mapheight), scale, size, offset);
            var p3 = MapCellData.ToArea(new Vector2(mapwidth, 0), scale, size, offset);
            var min = Vector2.Min(Vector2.Min(p0, p1), Vector2.Min(p2, p3));
            var max = Vector2.Max(Vector2.Max(p0, p1), Vector2.Max(p2, p3));

            var rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            GUI.DrawTexture(rect, sp);
            EditorGUI.DrawRect(rect, Color.gray * 0.7f);
            //GUILayout.BeginArea(Rect.MinMaxRect(min.x,min.y,max.x,max.y), new GUIContent(sp));
            //GUILayout.EndArea();
        }
    }

    private bool showPic = true;
    private bool showXY = true;
    private GameObject currentSelect;
    private bool draw;

    private void OnGUIBrush(Rect size)
    {
        using (new RectScrope(size,"画板"))
        {
            bool ret = EditorGUILayout.ToggleLeft("绘制", draw);
            if (ret != draw)
            {
                draw = ret;
                if (draw)
                {
                    astar = false;
                }
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
        }
    }

    private string[] tabs = new string[] { "地基", "事件" };
    private TabView tabview;
   

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
        var content = medusa.PreviewBases((MapCellData.Catalogue)type, out objs);
        var s = GUILayout.SelectionGrid(selectBrushBase, content, 15, GUILayout.Width(600), GUILayout.Height(30));
        if (s != selectBrushBase)
        {
            Selection.objects = null;
            selectBrushBase = s;
            currentSelect = objs[s];
        }

    }



    void OnGUIInfo(Rect size)
    {
        if (selectData != null)
        {

            using (new RectScrope(size, "详细信息"))
            {

                if (selectData.eventType != MapCellData.EventType.None)
                    EditorGUILayout.LabelField("选中了事件");
                MapCellTool.Draw(selectData);
            }

        }
    }


    private bool GUILayer = true;
    private Vector2 layerScroll;
    private int layerSelect = 0;
    private int defaultBrushIndex;
    private void OnGUILayer(Rect size)
    {

        EditorGUI.DrawRect(size, Color.gray * 0.7f);
        GUILayout.BeginArea(size);

        GUILayer = EditorGUILayout.BeginFoldoutHeaderGroup(GUILayer, "新增层级");
        //EditorGUILayout.LabelField("层级");

        if (GUILayer)
        {
            using (new RectScrope(new Rect( 0,20, size.width, size.height / 2), "layers"))
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
            }

            using (new RectScrope(new Rect(0, size.height / 2 + 20, size.width, size.height / 2), "新增层级"))
            {

                EditorGUILayout.LabelField("选择默认地基");

                GameObject[] objs = null;
                var sel = GUILayout.SelectionGrid(defaultBrushIndex, medusa.PreviewBases(MapCellData.Catalogue.Floor, out objs), 4, GUILayout.Height(70));
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
        }
        GUILayout.EndArea();
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

    private bool astar = false;
    private int startIndex;
    private int endIndex;
    void OnGUIAStart(Rect size)
    {
        using (new RectScrope(size, "寻路"))
        {
            astar = EditorGUILayout.ToggleLeft("寻路", astar);
            if (astar)
            {
                draw = false;
            }
        }
    }


    void OnGUIPath(Rect size)
    {
        
        if (astar)
        {
            var ret = MapCellData.searchRoute(medusa.map.cells[startIndex], medusa.map.cells[endIndex]);
            if (ret != null && ret.Count > 1)
            {
                var pos = ret.Select(r =>
                {
                    var worldpos = World.ToWorldPos(new Vector2(r.x, r.y));
                    var toscreen = MapCellData.ToArea(new Vector2(worldpos.x, worldpos.z), scale, size, offset);
                    return toscreen;
                }).ToArray();

                Handles.BeginGUI();
                var oldcolor = Handles.color;
                Handles.color = Color.green;
                for (int i = 0; i < pos.Length - 1; i++)
                {
                    Handles.DrawLine(pos[i], pos[i + 1]);
                }

                Handles.color = oldcolor;
                Handles.EndGUI();
            }
        }
    }
}
