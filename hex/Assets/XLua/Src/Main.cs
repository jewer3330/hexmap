using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Game.Lua;

using System;
using XLua.LuaDLL;
using XLua;


public class Main : MonoBehaviour
{
    public enum Mode
    { 
        Editor,
        AssetBundle,
        Zip,
    }

    [System.Serializable]
    public class LuaMap
    {
        public string luaName;
        public string luaPath;
        [HideInInspector]
        public byte[] luaContent;
    }
    public static LuaEnv luaEnv { get => _luaEnv; }
    private static LuaEnv _luaEnv = new LuaEnv();
    private static Dictionary<string, byte[]> buffers = new Dictionary<string, byte[]>();

    public static LuaEnv.CustomLoader LuaLoader { get; private set; }

    [DoNotGen]
    public static void Init(Mode mode)
    {
        //switch (mode)
        //{
        //    case Mode.AssetBundle:
        //        break;
        //    case Mode.Editor:
        //        break;
        //    case Mode.Zip:
        //        break;
        //}


        if (mode == Mode.Editor)
        {
            LuaLoader = ReadBytesFromEditor;
            luaEnv.AddLoader(LuaLoader);
        }
        else if (mode == Mode.AssetBundle)
        {
            LuaLoader = ReadBytesFromAssetBundle;
            luaEnv.AddLoader(LuaLoader);
        }
        else if (mode == Mode.Zip)
        {
            //LuaLoader = ReadBytesFromZip;
            //luaEnv.AddLoader(LuaLoader);
        }

    }

    public void Clear()
    {
        buffers.Clear();
    }

    public void Dispose()
    {
        loaded = false;
        Clear();  
    }
     
    private const string luafile_format = "Assets/LuaScripts/src/{0}.lua";

    private const string bytesfile_format = "Assets/LuaScripts/src/{0}.bytes";

    

    private static byte[] ReadBytesFromAssetBundle2(ref string filename)
    {
        string luaPath = Application.dataPath + "/LuaScripts/src/" + filename + ".lua";
        if (Application.platform == RuntimePlatform.Android)
        {
            luaPath = Application.persistentDataPath + "/LuaScripts/src/" + filename + ".lua";
            Debug.Log("Application.persistentDataPath:" + luaPath);
        }
        string strLuaContent = File.ReadAllText(luaPath);
        byte[] result = System.Text.Encoding.UTF8.GetBytes(strLuaContent);
        return result;

    }

    private static byte[] ReadBytesFromAssetBundle(ref string filename)
    {
      
        var path = string.Format(bytesfile_format, filename);

        Debug.Log(" ReadBytesFromAssetBundle :" + filename);

        byte[] bytes;
       
        if (!buffers.TryGetValue(path, out bytes))
        {         
            var request = libx.Assets.LoadAsset(path, typeof(TextAsset));

            var ta = request.asset as TextAsset;
            if (ta != null)
            {
                bytes = ta.bytes;
                buffers[path] = bytes;
            }
            else
            {
                Debug.Log("request.asset error!!! :" + filename);
            }
            Resources.UnloadAsset(ta);
            request.Release();
            request = null;
        }
        return bytes;
    }

    private static byte[] ReadBytesFromEditor(ref string filename)
    {
        var path = string.Format(luafile_format, filename.Replace('.', '/'));
        if (!System.IO.File.Exists(path))
        {
            throw new System.IO.FileNotFoundException(path);
        }

        byte[] bytes;
        if (!buffers.TryGetValue(path, out bytes))
        {
            bytes = System.IO.File.ReadAllBytes(path);
            buffers[path] = bytes;
        }
        return bytes;
    }
    private static bool loaded = false;

    private const string luafilezip_format = "LuaScripts/src/{0}.lua";
    //private static byte[] ReadBytesFromZip(ref string filename)
    //{
    //    if (!loaded)
    //    {
    //        var fileName = "LuaScripts.zip";
    //        var path = Application.persistentDataPath + "/" + fileName;
    //        byte[] ret = null;
    //        if (File.Exists(path))
    //        {
    //            ret = File.ReadAllBytes(path);
    //        }
    //        else
    //        {
    //            var www = new WWW(Application.streamingAssetsPath + "/" + fileName);
    //            while (!www.isDone);
    //            ret = www.bytes;
    //        }
    //        if (ret == null || ret.Length == 0) return null;

    //        int size = 2048;
    //        byte[] data = new byte[size];
    //        using (var ms =  new MemoryStream(ret))
    //        {
    //            using (var s = new ZipInputStream(ms))
    //            {
    //                ZipEntry entry; 
    //                while ((entry = s.GetNextEntry()) != null)
    //                {
                       
    //                    if (entry.IsFile)
    //                    {

                            
    //                        using (MemoryStream rms = new MemoryStream())
    //                        {
    //                            while (true)
    //                            {
    //                                size = s.Read(data, 0, data.Length);
    //                                if (size > 0)
    //                                {
    //                                    rms.Write(data, 0, size);
    //                                }
    //                                else
    //                                {
    //                                    break;
    //                                }
    //                            }
    //                            var buffer = rms.ToArray();
    //                            buffers.Add(entry.Name, buffer);
    //                            //var name = Application.streamingAssetsPath + "/" + entry.Name;
    //                            //if (!Directory.Exists(name))
    //                            //{
    //                            //    Directory.CreateDirectory(Path.GetDirectoryName(name));
    //                            //    File.WriteAllBytes(name, buffer);
    //                            //}
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        loaded = true;
    //    }
    //    byte[] bytes;
    //    buffers.TryGetValue(string.Format(luafilezip_format, filename.Replace('.', '/')).ToLower(), out bytes);
    //    //foreach (var kv in fileCaches)
    //    //{
    //    //    Debug.Log(kv.Key);
    //    //}
    //    return bytes;
    //}
   

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
    }

    private byte[] LoadLuaFile(string v)
    {
        return LuaLoader(ref v);
    }

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
  //    Init(Mode.Zip);
        Init(Mode.Editor);
#else
        Init(Mode.Editor);
#endif
  
        libx.Assets.Initialize().completed = delegate (libx.AssetRequest req)
        {       
            LuaBehaviour lua = gameObject.AddComponent<LuaBehaviour>(); 
        };
     
    }

    // Update is called once per frame
   
}
