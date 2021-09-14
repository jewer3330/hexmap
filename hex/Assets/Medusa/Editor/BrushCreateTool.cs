using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BrushCreateTool : EditorWindow
{
    private GameObject go;
    private MapCellData data = new MapCellData();
    private MedusaToolWindow window;
    private string facilities_path = "Assets/Medusa/Facilities";
    private string floor_path = "Assets/Medusa/Floor";
    private string brushName = "默认画笔";
    private string path
    {
        get
        {
            var path = floor_path;
            if (data.buildingType == MapCellData.HasEvent.Has)
            {
                path = facilities_path;
            }
            return path;
        }
    }
    public static BrushCreateTool Open(MedusaToolWindow p)
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
            //data.buildingType = (MapCellData.BuildingType)EditorGUILayout.EnumPopup("笔刷分类",data.buildingType);
            MapCellTool.DrawBrush(data);
        }
        if (GUILayout.Button("Create"))
        {
            GameObject cloned = null;
            if (!go)
            {
                cloned = new GameObject("DefaultBrush");
                CreateDefault(cloned, brushName);
            }
            else
            {
                cloned = Instantiate(go);
            }
            var brush = cloned.GetComponent<HexBrush>();
            if (!brush)
                brush = cloned.AddComponent<HexBrush>();
            brush.data = data;
            cloned.name = brushName.ToString();
            MapCreateTool.InitBrush(brush);
          
            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(cloned, string.Format("{0}/{1}.prefab", path, brushName), InteractionMode.UserAction);
            GameObject.DestroyImmediate(cloned);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = prefab;
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
