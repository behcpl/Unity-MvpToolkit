using UnityEditor;
using UnityEngine;

namespace DataPresentersGallerySample
{
    public class SimpleGameSampleWindow : EditorWindow
    {
        [MenuItem ("MvpToolkit/Simple Game Sample")]
        public static void ShowWindow()
        {
            GetWindow(typeof(SimpleGameSampleWindow));
        }
    
        private void OnGUI () 
        {
            if (SimpleGameSampleInstaller.IsSampleInstalled())
            {
                EditorGUILayout.HelpBox("Data Presenters Gallery Sample is already installed.\nOpen SimpleGameRoot scene for Play mode.", MessageType.Info);
                return;
            }
            
            if(SimpleGameSampleInstaller.HasTextMeshProPackage())
            {
                if (SimpleGameSampleInstaller.HasTextMeshProEssentials())
                {
                    EditorGUILayout.HelpBox("All requirements are met", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("TextMesh PRO essentials are missing!\nPlease import essentials from TextMesh Pro settings panel.", MessageType.Error);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("TextMesh PRO package is missing!\nPlease import package and essentials.", MessageType.Error);
            }

            GUI.enabled = SimpleGameSampleInstaller.HasTextMeshProPackage() && SimpleGameSampleInstaller.HasTextMeshProEssentials();
            if( GUILayout.Button( "Install Sample") )
            {
                SimpleGameSampleInstaller.InstallAssetPackage();
            }

            GUI.enabled = true;
        }
    }
}