using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RectScrope : System.IDisposable
{
    public RectScrope(Rect size,string name)
    {
        EditorGUI.DrawRect(size, Color.gray * 0.7f);
        GUILayout.BeginArea(size);
        GUILayout.Label(name);
    }
    public void Dispose()
    {
        GUILayout.EndArea();
    }
}
