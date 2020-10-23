/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XLua;
using System;
using System.IO;
namespace Game.Lua
{
    [System.Serializable]
    public class Injection
    {
        public string name;
        public GameObject value;
    }

    [LuaCallCSharp]
    public class LuaBehaviour : MonoBehaviour
    { 
        public Injection[] injections;

  
        internal static float lastGCTime = 0;
        internal const float GCInterval = 1;//1 second 

        private Action luaStart;
        private Action luaUpdate;
        private Action luaLateUpdate;
        private Action luaFixedUpdate;
        private Action luaOnDestroy;
        private LuaTable scriptEnv;

        //private byte[] CustomLuaLoader(ref string fileName)
        //{
        //    string luaPath = Application.dataPath + "/LuaScripts/src/" + fileName + ".lua";
        //    string strLuaContent = File.ReadAllText(luaPath);
        //    byte[] result = System.Text.Encoding.UTF8.GetBytes(strLuaContent);
        //    return result;
        //}
        //public static LuaEnv luaEnviorment
        //{
        //    get
        //    {
        //        return luaEnv;
        //    }
        //}

        void Awake()
        {

            Init();
           
        } 

        public void Init()
        {
            //if (luaEnv.customLoaders == null || luaEnv.customLoaders.Count == 0)
            //    luaEnv.AddLoader(CustomLuaLoader);
            scriptEnv = Main.luaEnv.NewTable();
            // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
            LuaTable meta = Main.luaEnv.NewTable();
            meta.Set("__index", Main.luaEnv.Global);
            scriptEnv.SetMetaTable(meta);
            meta.Dispose();

            scriptEnv.Set("self", this);

            if (injections != null)
            {
                foreach (var injection in injections)
                {
                    scriptEnv.Set(injection.name, injection.value);
                }
            }
            var path = "World/World";

            Main.luaEnv.DoString(Main.LuaLoader(ref path), "XLuaScript", scriptEnv);

            Action luaAwake = scriptEnv.Get<Action>("awake");
            scriptEnv.Get("start", out luaStart);
            scriptEnv.Get("update", out luaUpdate);
            scriptEnv.Get("onDestroy", out luaOnDestroy);

            scriptEnv.Get("fixedupdate", out luaFixedUpdate);
            scriptEnv.Get("lateupdate", out luaLateUpdate);

          

            if (luaAwake != null)
            {
                luaAwake();
            }


            if (luaStart != null)
            {
                luaStart();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (luaUpdate != null)
            {
                luaUpdate();
            }
            if (Time.time - LuaBehaviour.lastGCTime > GCInterval)
            {
                Main.luaEnv.Tick();
                LuaBehaviour.lastGCTime = Time.time;
            }
        }

        private void FixedUpdate()
        {
            if(luaFixedUpdate!=null)
            {
                luaFixedUpdate();
            }
        }

        private void LateUpdate()
        {
            if(luaLateUpdate!=null)
            {
                luaLateUpdate();
            }
        }

        void OnDestroy()
        {
            if (luaOnDestroy != null)
            {
                luaOnDestroy();
            }
            luaOnDestroy = null;
            luaUpdate = null;
            luaStart = null;
            scriptEnv.Dispose();
            injections = null;
        }
    }
}
