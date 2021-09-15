using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Menu 
{
    public class MenuItem
    {
        public int index;
        public string text;
        public GenericMenu.MenuFunction callback;
    }

    public List<MenuItem> items = null;
    public int currentIndex = 0;
    public string title;
    public Rect position;
    public Menu(string title,Rect position)
    {
        this.title = title;
        items = new List<MenuItem>();
        this.position = position;
    }

    public void AddItem(string text, GenericMenu.MenuFunction callback)
    {
        int index = items.Count;
        MenuItem item = new MenuItem();
        item.index = index;
        item.text = text;
        item.callback = callback;

        items.Add(item);
    }

   
    public void OnGUI()
    {
        int v = 0;
        if (EditorGUILayout.DropdownButton(new GUIContent(title), FocusType.Passive,GUILayout.Width(position.width)))
        {
            GenericMenu menu = new GenericMenu();
            for (int i = 0; i < items.Count;i++)
            {
                menu.AddItem(new GUIContent(items[i].text), false, items[i].callback);
                menu.DropDown(position);
            }
         }
        if (v != currentIndex)
        {
            currentIndex = v;

            if (items[currentIndex] != null && items[currentIndex].callback != null)
            {
                items[currentIndex].callback();
            }
        }
    }
}
