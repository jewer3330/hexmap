using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MedusaToolWindow2D : EditorWindow
{


    private int width => 300;
    private int topHeight => 200;
    private Rect toolBarSize => new Rect(0, 0, width, 20);
    public Rect topSize => new Rect(0, 20, width, topHeight);
    public Rect topSizeBrush => new Rect(width + 10, 20, this.position.width - topSize.width - width * 2, 100);

    public Rect contentSize => new Rect(0, topSize.y + topSize.height + 10, this.position.width, 600);

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
            OnGUIMapcell(contentSize);
            OnGUIBrush(topSizeBrush);
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
                    MapCreateTool.Open(medusa);
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
    private void OnGUIMapcell(Rect size)
    {
        using (new RectScrope(size,"地图信息"))
        {
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
                var wp = MapCellData.ToWorld(screenP, scale, size, offset);
                var h = World.ToHex(wp);
                if (medusa != null && medusa.map)
                {
                    var index = medusa.map.HexPositionToIndex((int)h.x, (int)h.y, 0);
                    Debug.Log(index);
                    Event.current.Use();
                }
            }



            if (medusa.map)
            {
                foreach (var k in medusa.map.cells)
                {
                    k.OnEditorGUI(size, scale,offset);
                }
            }

            MapCellData.OnEditorXY(size, scale, offset);
        }
    }

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
        var content = medusa.PreviewBases((MapCellData.HasEvent)type, out objs);
        var s = GUILayout.SelectionGrid(selectBrushBase, content, 15, GUILayout.Width(600), GUILayout.Height(30));
        if (s != selectBrushBase)
        {
            Selection.objects = null;
            selectBrushBase = s;
            currentSelect = objs[s];
        }

    }
}
