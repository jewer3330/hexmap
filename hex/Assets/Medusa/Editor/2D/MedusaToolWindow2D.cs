using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MedusaToolWindow2D : EditorWindow
{


    private int width => 300;
    private Rect toolBarSize => new Rect(0, 0, width, 20);
    public Rect topSize => new Rect(0, 0, this.position.width, 300);

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
                    medusa.Load();
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

}
