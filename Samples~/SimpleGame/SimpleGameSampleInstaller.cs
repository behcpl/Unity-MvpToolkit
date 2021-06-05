using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DataPresentersGallerySample
{
    [InitializeOnLoad]
    public static class SimpleGameSampleInstaller
    {
        private const string _PRELOADER_SCENE = "Assets/SimpleGame/PreloaderScene/SimpleGamePreloaderScene.unity";
        private const string _ROOT_SCENE = "Assets/SimpleGame/RootScene/SimpleGameRootScene.unity";
        private const string _LOBBY_SCENE = "Assets/SimpleGame/LobbyScene/SimpleGameLobbyScene.unity";
        private const string _GAMEPLAY_SCENE = "Assets/SimpleGame/GameplayScene/SimpleGameGameplayScene.unity";
        private const string _RANKING_SCENE = "Assets/SimpleGame/RankingScene/SimpleGameRankingScene.unity";

        private const string _PACKAGE_NAME = "simple_game";
        private const string _ASSET_PACKAGE_PATH = "Packages/com.behc.mvptoolkit/AssetPackages/simple_game.unitypackage";

        static SimpleGameSampleInstaller()
        {
            AssetDatabase.importPackageCompleted += OnImportPackageCompleted;

            if (IsSampleInstalled())
                return;

            if (HasTextMeshProPackage() && HasTextMeshProEssentials())
            {
                InstallAssetPackage();
            }
            else
            {
                SimpleGameSampleWindow.ShowWindow();
            }
        }

        public static bool HasTextMeshProPackage()
        {
#if SAMPLE_TEXTMESHPRO_PRESENT
            return true;
#else
            return false;
#endif
        }

        public static bool HasTextMeshProEssentials()
        {
            // taken from TMP code
            return File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset");
        }

        public static bool IsSampleInstalled()
        {
            return File.Exists(_ROOT_SCENE);
        }

        public static void InstallAssetPackage()
        {
            AssetDatabase.ImportPackage(_ASSET_PACKAGE_PATH, false);
        }

        private static void SetupScenes()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
  
            if (scenes.Find(i => i.path == _PRELOADER_SCENE) == null)
            {
                scenes.Add(new EditorBuildSettingsScene(_PRELOADER_SCENE, true));
            }

            if (scenes.Find(i => i.path == _ROOT_SCENE) == null)
            {
                scenes.Add(new EditorBuildSettingsScene(_ROOT_SCENE, true));
            }

            if (scenes.Find(i => i.path == _LOBBY_SCENE) == null)
            {
                scenes.Add(new EditorBuildSettingsScene(_LOBBY_SCENE, true));
            }

            if (scenes.Find(i => i.path == _GAMEPLAY_SCENE) == null)
            {
                scenes.Add(new EditorBuildSettingsScene(_GAMEPLAY_SCENE, true));
            }

            if (scenes.Find(i => i.path == _RANKING_SCENE) == null)
            {
                scenes.Add(new EditorBuildSettingsScene(_RANKING_SCENE, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();

            EditorSceneManager.OpenScene(_PRELOADER_SCENE);
        }

        private static void SetupLayers()
        {
            Object tagManager = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset");
            Preset preset = (Preset)AssetDatabase.LoadMainAssetAtPath("Assets/TagManager.preset");
            preset.ApplyTo(tagManager);
        }

        private static void OnImportPackageCompleted(string packageName)
        {
            if (packageName == _PACKAGE_NAME)
            {
                SetupScenes();
                SetupLayers();
            }
        }
    }
}