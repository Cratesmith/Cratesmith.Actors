using System;
using System.Collections.Generic;
using Cratesmith.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Cratesmith.Actors
{
    public abstract class SceneRoot<T> : SceneRootBase where T:SceneRoot<T>, new()
    {
        private static T _sDontDestroySceneRoot = null;
        static readonly Dictionary<Scene, T> s_sceneRoots = new Dictionary<Scene, T>();
        private bool m_initialized;

        public static T GetDontDestroy()
        {
            if (_sDontDestroySceneRoot == null)
            {
                var newGO = new GameObject(GetName() + "s (DontDestroyOnLoad)");
                Object.DontDestroyOnLoad(newGO);
                _sDontDestroySceneRoot = newGO.AddComponent<T>();
            }

            return _sDontDestroySceneRoot;
        }

        private static string GetName()
        {
            var name = typeof(T).Name;
            var prefix = "SceneRoot";
            if (name.Length > prefix.Length && name.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
            {
                name = name.Substring(prefix.Length);
            }

            return name;
        }

        public static T Get(Component component)
        {
            return Get(component.gameObject.scene);
        }

        public static T Get(GameObject gameObject)
        {
            return Get(gameObject.scene);
        }

        public static T Get(Scene scene)
        {
            T output = null;
            Assert.IsTrue(scene.IsValid());
            if (!s_sceneRoots.TryGetValue(scene, out output))
            {
                if (!scene.IsDontDestroy())
                {
                    foreach (var rootObj in scene.GetRootGameObjects())
                    {
                        var inst = rootObj.GetComponent<T>();
                        if (inst != null)
                        {
                            s_sceneRoots[scene] = inst;
                            return inst;
                        }
                    }

                    var newGO = new GameObject(GetName() + "s (" + scene.name + ")");
                    SceneManager.MoveGameObjectToScene(newGO, scene);
                    s_sceneRoots[scene] = output = newGO.AddComponent<T>();
                }
                else
                {
                    output = GetDontDestroy();
                }
            }

            // This brings up an error if you'are accessing a singleton that wasn't set up in awake
            // both because Awake 
            if(Application.isPlaying && output!=null && !output.m_initialized)
            {
                Assert.IsTrue(output==null||output.m_initialized, string.Format("SceneRoot {0} wasn't initialized! Did you override Awake() instead of OnAwake() ... and forget to call base.Awake()?", typeof(T).FullName));
            }

            return output;
        }

        void Awake()
        {
            if (!gameObject.scene.IsDontDestroy())
            {			
                s_sceneRoots[gameObject.scene] = this as T;
            }
            else
            {
                _sDontDestroySceneRoot = this as T;
            }

            m_initialized = true;
            transform.SetAsFirstSibling();
            OnAwake();
        }

        protected virtual void OnAwake()
        {        
        }

        protected virtual void OnDestroy()
        {
            if(!Application.isPlaying) return;

            T temp = null;
            if (_sDontDestroySceneRoot == this)
            {
                _sDontDestroySceneRoot = null;
            }
            if (!gameObject.scene.IsDontDestroy() && s_sceneRoots.TryGetValue(gameObject.scene, out temp) && temp == this)
            {
                s_sceneRoots.Remove(gameObject.scene);
            }
        }
    }

    public abstract class SceneRootBase : Actor
    {	
        public static implicit operator Transform(SceneRootBase @this)
        {
            return @this.transform;
        }
    }
}