using System.Collections.Generic;
using Cratesmith.ScriptExecutionOrder;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Cratesmith.Actors
{
    /// <summary>
    /// Base for auto-constructed singletons that exist within a scene
    /// </summary>
    /// <typeparam name="TSelfType"></typeparam>
    public abstract class SceneSingleton<TSelfType> : SceneSingletonBase where TSelfType:SceneSingleton<TSelfType>
    {	
        private static Dictionary<Scene,TSelfType> s_instances = new Dictionary<Scene, TSelfType>();
        private bool m_initialized = false;

        public Scene scene { get { return gameObject.scene; }}

        void Awake()
        {
            TSelfType output;
            if (s_instances.TryGetValue(gameObject.scene, out output) && output!=null && output!=this)
            {
                Debug.LogErrorFormat("A second instance of {0} is being created in scene {1}. Destroying it in awake.", typeof(TSelfType).Name, gameObject.scene.name);
                DestroyImmediate(this);
            }

            // we set the instance in awake so that other Awake calls in the same frame don't create multiple instances of the singleton
            s_instances[gameObject.scene] = (TSelfType)this;
            m_initialized = true;

            if (transform.parent == null)
            {
                transform.parent = GetSingletonSceneRoot(gameObject.scene);			
            }

            OnSingletonInit();
            OnAwake();
        }

        protected virtual void OnSingletonInit()
        {        
        }

        protected virtual void OnAwake()
        {        
        }

        public static TSelfType Get(Component forComponent, bool constructIfMissing=true)
        {
            if (forComponent == null)
            {
                return null;
            }

            return Get(forComponent.gameObject.scene, constructIfMissing);
        }

        public static TSelfType Get(GameObject forGameObject, bool constructIfMissing=true)
        {
            if (forGameObject == null)
            {
                return null;
            }

            return Get(forGameObject.scene, constructIfMissing);
        }

        public static TSelfType Get(Scene scene, bool constructIfMissing=true)
        {
            TSelfType output;
            if (!s_instances.TryGetValue(scene, out output) && constructIfMissing)
            {
                var root = GetSingletonSceneRoot(scene);
                var go = new GameObject(typeof(TSelfType).Name);
                go.transform.parent = root;
                s_instances[scene] = output = go.AddComponent<TSelfType>();			
            }
		
            // This brings up an error if you'are accessing a singleton that wasn't set up in awake
            // both because Awake 
            if(Application.isPlaying && output!=null && !output.m_initialized)
            {
                Assert.IsTrue(output==null||output.m_initialized, string.Format("Singleton {0} wasn't initialized! Did you override Awake() instead of OnAwake() ... and forget to call base.Awake()?", typeof(TSelfType).FullName));
            }
            return output;
        }	
    }


// A base class that provides the root objects for singletons in each scene
// this is a simple way to ensure we have one table used by all SceneSingletons despite them being generics
    [ScriptExecutionOrder(-100)] // base exection order -100 (before normal objects)
    public abstract class SceneSingletonBase : Actor
    {
        protected static Transform GetSingletonSceneRoot(Scene scene)
        {
            return SceneRootSingleton.Get(scene).transform;
        }
    }
}