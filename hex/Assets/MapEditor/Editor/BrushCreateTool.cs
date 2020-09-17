using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BrushCreateTool : EditorWindow
{
    private GameObject go;
    private MapCellData data = new MapCellData();
    private int type = 0;
    private MapTool window;
    private string path = "Assets/MapEditor/BrushPrefabs";
    private string brushName = "default";
    public static BrushCreateTool Open(MapTool p)
    {
        BrushCreateTool window = (BrushCreateTool)EditorWindow.GetWindow(typeof(BrushCreateTool));
        window.Show();
        window.window = p;
        return window;
    }

    private void OnGUI()
    {
        go = (GameObject)EditorGUILayout.ObjectField("画笔模型",go, typeof(GameObject),false);
        
        brushName = EditorGUILayout.TextField("画笔名称",brushName);

        if (data != null)
        {
            data.buildingType = (MapCellData.BuildingType)EditorGUILayout.EnumPopup("笔刷索引",data.buildingType);
            MapCellTool.DrawBrush(data);
        }
        if (GUILayout.Button("Create"))
        {
            if (!go)
            {
                go = new GameObject("DefaultBrush");
                CreateDefault(go, brushName);
            }
            else
            {
                go = Instantiate(go);
            }
            var brush = go.AddComponent<HexBrush>();
            brush.data = data;
            
            PrefabUtility.SaveAsPrefabAssetAndConnect(go, string.Format("{0}/{1}.prefab", path, brushName), InteractionMode.UserAction);
            GameObject.DestroyImmediate(go);
            go = null;
            data = new MapCellData();
        }
        
           
            
    }

    private void CreateDefault(GameObject go,string name)
    {
        var hm = go.AddComponent<HexModel>();
        var mesh = hm.GenMesh();
        
        string meshpath = string.Format("{0}/{1}.mesh", path, name);
        AssetDatabase.CreateAsset(mesh, meshpath);
        AssetDatabase.Refresh();
        hm.meshFilter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshpath);

        string mat = string.Format("{0}/{1}.mat", path, name);
        AssetDatabase.CreateAsset(new Material(Shader.Find("Diffuse")), mat);
        AssetDatabase.Refresh();
        hm.meshRenderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(mat);

       
    }
}
