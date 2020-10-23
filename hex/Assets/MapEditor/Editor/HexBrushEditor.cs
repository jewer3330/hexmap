using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.IO;
using System;
using Object = UnityEngine.Object;
using UnityEngine.EventSystems;

// Custom Editor using SerializedProperties.
// Automatic handling of multi-object editing, undo, and Prefab overrides.
[CustomEditor(typeof(HexBrush))]
[CanEditMultipleObjects]
public class HexBrushEditor : Editor
{
   

    public override void OnInspectorGUI()
    {
        
         if (GUILayout.Button("Init"))
         {
            MapCreateTool.InitBrush(target as HexBrush);
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
            base.OnInspectorGUI();
        serializedObject.ApplyModifiedProperties();
    }

   
}