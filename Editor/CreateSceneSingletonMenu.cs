using UnityEditor;

#if UNITY_EDITOR

namespace Cratesmith.Actors
{
    public static class CreateSceneSingletonMenu
    {
        [MenuItem("Assets/Create/Singletons/C# SceneSingleton", false, 100)] 
        public static void CreateSceneSingleton()
        {
            CreateActorMenu.ShowActorDialog(typeof(SceneSingleton<>), singletonFormatNoSettings, false);
        }

        [MenuItem("Assets/Create/Singletons/C# SceneSingleton (with SettingsAsset)", false, 100)] 
        public static void CreateSceneSingletonWithSettings()
        {
            CreateActorMenu.ShowActorDialog(typeof(SceneSingleton<>), singletonFormatSettings, true);
        }

        [MenuItem("Assets/Create/Singletons/C# DontDestroySingleton", false, 100)] 
        public static void CreateDontDestroySingleton()
        {
            CreateActorMenu.ShowActorDialog(typeof(DontDestroySingleton<>), singletonFormatNoSettings, false);
        }

        [MenuItem("Assets/Create/Singletons/C# DontDestroySingleton (with SettingsAsset)", false, 100)] 
        public static void CreateDontDestroySingletonWithSettings()
        {
            CreateActorMenu.ShowActorDialog(typeof(DontDestroySingleton<>), singletonFormatSettings, true);
        }

        const string singletonFormatSettings = "using UnityEngine;\n" +
                                               "using Cratesmith.Actors;\n" +
                                               "using Cratesmith.Settings;\n" +
                                               "\n" +
                                               "// Singleton type. Can be placed in scene or will auto-construct if requested when no instance exists.\n" +
                                               "// Will auto destroy if an instance already exists within it's scope.\n" +
                                               "public class {0} : {1}<{0}>\n" +
                                               "{{\n" +
                                               "    public {0}Settings.Reference settings = new {0}Settings.Reference();\n" +
                                               "    \n" +
                                               "    \n" +
                                               "}}\n";

        const string singletonFormatNoSettings = "using UnityEngine;\n" +
                                                 "using Cratesmith.Actors;\n" +
                                                 "\n" +
                                                 "// Singleton type. Can be placed in scene or will auto-construct if requested when no instance exists.\n" +
                                                 "// Will auto destroy if an instance already exists within it's scope.\n" +
                                                 "public class {0} : {1}<{0}>\n" +
                                                 "{{\n" +
                                                 "    \n" +
                                                 "}}\n";    
    }
}
#endif
 