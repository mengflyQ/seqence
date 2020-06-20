using System.Collections.Generic;
using UnityEditor;


namespace UnityEngine.Timeline
{
    public class XResources
    {
        class Asset : ISharedObject
        {
            public Object asset;
            public uint refence;

            public Asset()
            {
                this.refence = 1;
            }

            public void Dispose()
            {
                refence = 0;
            }
        }

        private static Dictionary<string, Asset> goPool = new Dictionary<string, Asset>();
        private static Dictionary<string, Asset> sharedPool = new Dictionary<string, Asset>();

        public static void Clean()
        {
            sharedPool.Clear();
            goPool.Clear();
            Resources.UnloadUnusedAssets();
        }

        public static GameObject LoadGameObject(string path)
        {
#if UNITY_EDITOR
            if (goPool.ContainsKey(path))
            {
                return Object.Instantiate((GameObject) goPool[path].asset);
            }
            else
            {
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var tmp = SharedObjects<Asset>.Get();
                tmp.asset = obj;
                goPool.Add(path, tmp);
                return Object.Instantiate(obj);
            }
#else
        // assetbundle implements 
#endif
        }

        public static void DestroyGameObject(string path,GameObject go)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(go);
            }
            else
            {
                Object.DestroyImmediate(go);
            }
            if (goPool.ContainsKey(path))
            {
                var it = goPool[path];
                it.refence--;
                if (it.refence <= 0)
                {
                    goPool.Remove(path);
                    SharedObjects<Asset>.Return(it);
                }
            }
        }


        public static T LoadSharedAsset<T>(string path) where T : Object
        {
#if UNITY_EDITOR
            if (sharedPool.ContainsKey(path))
            {
                return sharedPool[path] as T;
            }
            else
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                var tmp = SharedObjects<Asset>.Get();
                tmp.asset = asset;
                sharedPool.Add(path, tmp);
                return asset;
            }
#else
        // assetbundle implements 
#endif
        }

        public static void DestroySharedAsset(string path)
        {
            if (sharedPool.ContainsKey(path))
            {
                var asset = sharedPool[path];
                asset.refence--;
                if (asset.refence <= 0)
                {
#if !UNITY_EDITOR
                     Resources.UnloadAsset(asset.asset);
#endif
                    SharedObjects<Asset>.Return(asset);
                }
            }
        }
    }
}
