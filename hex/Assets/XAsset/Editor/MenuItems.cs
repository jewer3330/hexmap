//
// AssetsMenuItem.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2020 fjy
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace libx
{
    public static class MenuItems
    {
        private const string KApplyBuildRules   = "Assets/Build/Rules"; 
        private const string KBuildPlayer       = "Assets/Build/Player";
        private const string KBuildManifest     = "Assets/Build/Manifest";
 

        private const string KGetAllAssetNames = "Assets/Build/GetAllAssetBundlesNames";  //获取所有的assetbundle
        private const string KBuildAssetBundles = "Assets/Build/AssetBundles";
        private const string KClearAssetBundles = "Assets/Build/Clear AssetBundles";
        private const string KCopyAssetBundles  = "Assets/Build/Copy AssetBundles";
        private const string KCopyPath          = "Assets/Build/CopyPath";

        private const string KCreateLua2Bytes = "Assets/Build/CreateLua2Bytes";
        private const string KDeleteLua2Bytes = "Assets/Build/DeleteLua2Bytes";
        private const string KDeleteLuaManifest = "Assets/Build/Clean";

        //[MenuItem("Assets/Regroup")]
        //private static void Regroup()
        //{
        //    foreach (var item in Selection.GetFiltered<Object>(SelectionMode.DeepAssets))
        //    {
        //        var path = AssetDatabase.GetAssetPath(item);
        //        var s = Path.GetFileNameWithoutExtension(path);
        //        if (s.Contains("-"))
        //        {
        //            var group = s.Substring(0, s.LastIndexOf('-')).Replace("-", "/");
        //            var dir = Path.GetDirectoryName(path);
        //            var newdir = dir + "/" + group;
        //            if (!Directory.Exists(newdir))
        //            {
        //                Directory.CreateDirectory(newdir);
        //            }
        //            FileUtil.MoveFileOrDirectory(path, newdir + "/" + Path.GetFileName(path));
        //        }
        //    }
        //}

        [MenuItem("Assets/Apply Rule/Text", false, 1)] 
        private static void ApplyRuleText()
        {
            var rules = BuildScript.GetBuildRules();
            AddRulesForSelection(rules, rules.searchPatternText);
        }

        [MenuItem("Assets/Apply Rule/Prefab", false, 1)] 
        private static void ApplyRulePrefab()
        {
            var rules = BuildScript.GetBuildRules();
            AddRulesForSelection(rules, rules.searchPatternPrefab);
        }

        [MenuItem("Assets/Apply Rule/PNG", false, 1)]
        private static void ApplyRulePNG()
        {
            var rules = BuildScript.GetBuildRules();
            AddRulesForSelection(rules, rules.searchPatternPNG);
        }

        [MenuItem("Assets/Apply Rule/Material", false, 1)]
        private static void ApplyRuleMaterail()
        {
            var rules = BuildScript.GetBuildRules(); 
            AddRulesForSelection(rules, rules.searchPatternMaterial);
        }

        [MenuItem("Assets/Apply Rule/Controller", false, 1)]
        private static void ApplyRuleController()
        {
            var rules = BuildScript.GetBuildRules();
            AddRulesForSelection(rules, rules.searchPatternController);
        }

        [MenuItem("Assets/Apply Rule/Asset", false, 1)]
        private static void ApplyRuleAsset()
        {
            var rules = BuildScript.GetBuildRules();
            AddRulesForSelection(rules, rules.searchPatternAsset);
        }

        [MenuItem("Assets/Apply Rule/Scene", false, 1)]
        private static void ApplyRuleScene()
        {
            var rules = BuildScript.GetBuildRules();
            AddRulesForSelection(rules, rules.searchPatternScene);
        }

        [MenuItem("Assets/Apply Rule/Dir", false, 1)]
        private static void ApplyRuleDir()
        {
            var rules = BuildScript.GetBuildRules();
            AddRulesForSelection(rules, rules.searchPatternDir);
        }

        private static void AddRulesForSelection(BuildRules rules, string searchPattern)
        {
            var isDir = rules.searchPatternDir.Equals(searchPattern);
            foreach (var item in Selection.objects)
            {
                var path = AssetDatabase.GetAssetPath(item);
                var rule = new BuildRule();
                rule.searchPath = path;
                rule.searchPattern = searchPattern;
                rule.searchOption = SearchOption.AllDirectories;
                rule.unshared = false;
                rule.searchDirOnly = isDir;
                ArrayUtility.Add<BuildRule>(ref rules.rules, rule);
            }
            EditorUtility.SetDirty(rules);
            AssetDatabase.SaveAssets();
            Selection.activeObject = rules;
            EditorUtility.FocusProjectWindow();
        }



        [MenuItem(KApplyBuildRules,false,99)]
        private static void ApplyBuildRules()
        {
            var watch = new Stopwatch();
            watch.Start();

      //      BuildScript.ClearAssetBundlesName();  //执行规则之前， 删除原来的 bundles
    

            BuildScript.ApplyBuildRules();
            watch.Stop();
            Debug.Log("ApplyBuildRules " + watch.ElapsedMilliseconds + " ms.");
            Debug.Log("生成规则，完毕！！！");
        }

        [MenuItem(KBuildPlayer,false,100)]
        private static void BuildStandalonePlayer()
        {
            Debug.Log("开始生成APK !!!");
            BuildScript.BuildStandalonePlayer();
            Debug.Log("生成APK ,完毕!!!");
        }

        //打包之前，会执行这个功能
        [MenuItem(KBuildManifest,false,101)]
        private static void BuildManifest()
        {
            BuildScript.BuildManifest();
            Debug.Log("BuildManifest 完毕!!!");
        }


        [MenuItem(KGetAllAssetNames,false,102)]
        private static void GetAllAssetNames()
        {
            string[] allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();

            Debug.Log("GetAllAssetNames 开始"  );
            for (int i = 0; i < allAssetBundleNames.Length; i++)
            {
                string text = allAssetBundleNames[i];
                Debug.Log("AssetBundle: " + text);
            }

            Debug.Log("GetAllAssetNames 完毕。总数量：" + allAssetBundleNames.Length);          

        }



        [MenuItem(KBuildAssetBundles,false,103)]
        private static void BuildAssetBundles()
        {
            var watch = new Stopwatch();
            watch.Start(); 
            BuildScript.BuildManifest();
            BuildScript.BuildAssetBundles();
            watch.Stop();
            Debug.Log("BuildAssetBundles " + watch.ElapsedMilliseconds + " ms."); 
        }   

        [MenuItem(KClearAssetBundles,false,200)]
        private static void ClearAssetBundles()
        {
            BuildScript.ClearAssetBundles();
        }

        [MenuItem(KCopyAssetBundles,false, 201)]
        private static void CopyAssetBundles()
        {
            BuildScript.CopyAssetBundlesTo(Path.Combine(Application.streamingAssetsPath, Assets.AssetBundles));
            AssetDatabase.Refresh();
        }

        [MenuItem(KCopyPath,false,202)]
        private static void CopyPath()
        {
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            EditorGUIUtility.systemCopyBuffer = assetPath;
            Debug.Log(assetPath);
        }

        [MenuItem(KCreateLua2Bytes, false, 203)]
        private static void CreateLua2Bytes()
        {
            string luaPath = Application.dataPath + "/LuaScripts";

            DirectoryInfo folder = new DirectoryInfo(luaPath);
            BuildScript.CreateLua2Bytes(folder);
            AssetDatabase.Refresh();
            Debug.Log("CreateLua2Bytes 完毕");
        }

        [MenuItem(KDeleteLua2Bytes, false, 204)]
        private static void DeleteLua2Bytes()
        {
              string luaPath = Application.dataPath + "/LuaScripts";
            DirectoryInfo folder = new DirectoryInfo(luaPath);

            BuildScript.DeleteLua2Bytes(folder);
            AssetDatabase.Refresh();

            Debug.Log("DeleteLua2Bytes 完毕");

        }

        [MenuItem(KDeleteLuaManifest, false, 205)]
        private static void DeleteLuaManifest()
        {
            string luaPath = Application.dataPath + "/LuaScripts";
            DirectoryInfo folder = new DirectoryInfo(luaPath);

            BuildScript.Clean(folder);
            AssetDatabase.Refresh();

            Debug.Log("Clean 完毕");

        }

     
        
    }
}