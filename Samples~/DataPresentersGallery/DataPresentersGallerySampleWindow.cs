using UnityEditor;
using UnityEngine;

namespace DataPresentersGallerySample
{
    public class DataPresentersGallerySampleWindow : EditorWindow
    {
        [MenuItem ("MvpToolkit/Data Presenters Gallery Sample")]
        public static void ShowWindow()
        {
            GetWindow(typeof(DataPresentersGallerySampleWindow));
        }
    
        private void OnGUI () 
        {
            if (DataPresentersGallerySampleInstaller.IsSampleInstalled())
            {
                EditorGUILayout.HelpBox("Data Presenters Gallery Sample is already installed.\nOpen DataPresentersGalleryRoot scene for Play mode.", MessageType.Info);
                return;
            }
            
            if(DataPresentersGallerySampleInstaller.HasTextMeshProPackage())
            {
                if (DataPresentersGallerySampleInstaller.HasTextMeshProEssentials())
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

            GUI.enabled = DataPresentersGallerySampleInstaller.HasTextMeshProPackage() && DataPresentersGallerySampleInstaller.HasTextMeshProEssentials();
            if( GUILayout.Button( "Install Sample") )
            {
                DataPresentersGallerySampleInstaller.InstallAssetPackage();
            }

            GUI.enabled = true;
        }
    }
}