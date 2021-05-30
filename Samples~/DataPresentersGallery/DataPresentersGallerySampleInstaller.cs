using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace DataPresentersGallerySample
{
    [InitializeOnLoad]
    public static class DataPresentersGallerySampleInstaller
    {
        private const string _ROOT_SCENE = "Assets/DataPresentersGallery/RootScene/DataPresentersGalleryRootScene.unity";
        private const string _ELEMENTS_SCENE = "Assets/DataPresentersGallery/ElementsScene/DataPresentersGalleryElementsScene.unity";

        private const string _PACKAGE_NAME = "sample_gallery";
        private const string _ASSET_PACKAGE_PATH = "Packages/com.behc.mvptoolkit/AssetPackages/sample_gallery.unitypackage";

        static DataPresentersGallerySampleInstaller()
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
                DataPresentersGallerySampleWindow.ShowWindow();
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

            if (scenes.Find(i => i.path == _ROOT_SCENE) == null)
            {
                scenes.Add(new EditorBuildSettingsScene(_ROOT_SCENE, true));
            }

            if (scenes.Find(i => i.path == _ELEMENTS_SCENE) == null)
            {
                scenes.Add(new EditorBuildSettingsScene(_ELEMENTS_SCENE, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();

            EditorSceneManager.OpenScene(_ROOT_SCENE);
        }

        private static void OnImportPackageCompleted(string packageName)
        {
            if (packageName == _PACKAGE_NAME)
            {
                SetupScenes();
            }
        }
    }
}