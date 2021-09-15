using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MedusaToolWindow2D : EditorWindow
{
    static string mapSavePath = "Assets/Medusa/Editor/Data";
    static string exportPath = "Assets/Medusa/Editor/Export";


    private int width => 300;
    private int startY = 0;
    private Rect toolBarSize => new Rect(330, 0, width, 20);
    public Rect topSize => new Rect(0, 0, this.position.width, 300);

    [MenuItem("Tools/Medusa Editor 2D &n")]
    static void Init()
    {
        MedusaToolWindow2D window = (MedusaToolWindow2D)GetWindow(typeof(MedusaToolWindow2D));
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
                    //Load();
                    break;
                case 1:
                    //Save();
                    break;
                case 2:
                    //MapCreateTool.Open(this);
                    break;
                case 3:
                    //Clean();
                    break;
                case 4:
                    //NewBrush();
                    break;
                case 5:
                    //Export();
                    break;
            }
            select = -1;
        }
        GUILayout.EndArea();
    }

    

}
