using System;
using System.IO;
using System.Linq;
using Cratesmith.Settings;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace Cratesmith.Actors
{
    public static class CreateActorMenu
    {
        const string ACTOR_PREFIX = "A";
        const bool ALWAYS_APPLY_PREFIX = true;

        [MenuItem("GameObject/Convert To Actor", true, 0)]
        [MenuItem("GameObject/Convert To Actor (with settings)", true, 0)]
        public static bool ConvertToActor_Validate()
        {
            return Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Actor>()==null;
        }

        [MenuItem("GameObject/Convert To Actor", false, 0)]
        public static void ConvertToActor(MenuCommand menuCommand)
        {
            ConvertGameObjectToActor(menuCommand.context as GameObject, false);
        }

        [MenuItem("GameObject/Convert To Actor (with settings)", false, 0)]
        public static void ConvertToActorWithSettings(MenuCommand menuCommand)
        {		
            ConvertGameObjectToActor(menuCommand.context as GameObject, true);
        }

        private static void ConvertGameObjectToActor(GameObject gameObject, bool createSettings)
        {
            Undo.RegisterCompleteObjectUndo(gameObject, "Converted to actor");

            var format = createSettings ? formatSettings : formatNoSettings;
            var actorPrefix = ACTOR_PREFIX;

            EditorApplication.LockReloadAssemblies();   		
            try
            {	
                var baseName = !ALWAYS_APPLY_PREFIX && gameObject.name.StartsWith(actorPrefix, StringComparison.InvariantCultureIgnoreCase)
                    ? gameObject.name.Substring(actorPrefix.Length)
                    : gameObject.name;

                gameObject.name = ScriptAssetUtil.MakeValidFileName(actorPrefix + baseName).Replace(" ", "");

                // create script if it doesn't exist	

                var dir = ScriptAssetUtil.GetNearestScriptDirectory(gameObject);
                Directory.CreateDirectory(dir);
                var path = dir + "/" + gameObject.name + ".cs";
                if (!File.Exists(path))
                {
                    CreateActorScript(typeof(Actor), path, format, createSettings);				
                    AssetDatabase.ImportAsset(path);
                }

                // apply it
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                var monoBehaviour = Undo.AddComponent<Actor>(gameObject);
                var so = new SerializedObject(monoBehaviour);
                so.FindProperty("m_Script").objectReferenceValue = script;
                so.ApplyModifiedProperties();	
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
            EditorApplication.UnlockReloadAssemblies();
        }
	

        [MenuItem("Assets/Create/Actors/C# Actor", false, 100)]
        public static void CreateActor()
        {
            ShowActorDialog(typeof(Actor), formatNoSettings, false);
        }

        [MenuItem("Assets/Create/Actors/C# Actor (with SettingsAsset)", false, 100)] 
        public static void CreateActorWithSettings()
        {
            ShowActorDialog(typeof(Actor), formatSettings, true);
        }

        const string formatSettings = "using UnityEngine;\n" +
                                      "using Cratesmith.Actors;\n" +
                                      "using Cratesmith.Settings;\n" +
                                      "\n" +
                                      "// Actor type. The 'main' behaviour for a GameObject and all of it's children.\n" +
                                      "public class {0} : {1}\n" +
                                      "{{\n" +
                                      "    public {0}Settings.Reference settings = new {0}Settings.Reference();\n" +
                                      "    \n"+
                                      "    \n"+
                                      "    \n"+
                                      "}}\n";

        const string formatNoSettings = "using UnityEngine;\n" +
                                        "using Cratesmith.Actors;\n" +
                                        "\n" +
                                        "// Actor type. The 'main' behaviour for a GameObject and all of it's children.\n" +	                                         
                                        "public class {0} : {1}\n" +
                                        "{{\n" +
                                        "    \n"+
                                        "    \n"+
                                        "    \n"+
                                        "}}\n";

        public static void ShowActorDialog(Type baseType, string formatString, bool createSettings)
        {
            var baseTypeName = ScriptAssetUtil.TrimGenericsFromType(baseType.Name);
        
            var icon     = ScriptAssetUtil.GetIconForType(baseType);
            ModalTextboxWindow.Create("Create New " + baseTypeName, input =>
            {
                var uniqeDir = ScriptAssetUtil.GetCreateAssetPath(Selection.assetGUIDs.FirstOrDefault());
                var filename = uniqeDir+"/"+ScriptAssetUtil.GetUniqueFilename(uniqeDir + "/" + input + ".cs");

                CreateActorScript(baseType, filename, formatString, createSettings);
            }, baseTypeName, icon);
        }

        private static void CreateActorScript(Type baseType, string filename, string formatString, bool createSettings)
        {
            var singletonTypeName = ScriptAssetUtil.TrimGenericsFromType(baseType.Name);        

            var className = Path.GetFileNameWithoutExtension(filename);
            ScriptAssetUtil.CreateScript(filename, formatString, className, singletonTypeName);

            if (createSettings)
            {            
                var settingsFilename = Path.GetDirectoryName(filename)+"/"+ScriptAssetUtil.GetUniqueFilename(Path.GetFileNameWithoutExtension(filename) + "Settings.cs");
                CreateSettingsMenu.CreateSettingsScript(settingsFilename);
            }
        }
    }
}
#endif