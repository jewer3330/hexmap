using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TabView
{
    private string[] tabs;
    private int currentSelect = 0;
    private Rect position;
    private Vector2 scrollPosition;

    public Dictionary<int, System.Action<int>> guiCallbacks = new Dictionary<int, System.Action<int>>();
    public TabView(string[] tabs,Rect position)
    {
        this.tabs = tabs;
        this.position = position;
    }

    public TabView Reg(int i, System.Action<int> guiCallback)
    {
      
        guiCallbacks[i] = guiCallback;
        return this;
    }

    public void OnGUI()
    {
       
        var s = GUILayout.Toolbar(currentSelect, tabs);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        if (s != currentSelect)
        {
            currentSelect = s;

            
        }
        System.Action<int> a = null;
        if (guiCallbacks.TryGetValue(currentSelect, out a))
        {
            a?.Invoke(currentSelect);
        }
        GUILayout.EndScrollView();
        
    }
}
