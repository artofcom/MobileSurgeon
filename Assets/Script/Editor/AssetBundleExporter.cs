using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Core.AssetBundle;
using System;

public class AssetBundleExporter : EditorWindow
{
    [MenuItem("BenevolendMed/Tools/AssetBundle Exporter")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AssetBundleExporter));
    }

    public void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label($"[Bundle List]");

        //if(Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
        if(EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android && EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
        {
            GUILayout.Space(50);
            GUILayout.Label($"=== Error] Need to switch build-target either Andorid or IOS ! ===");
            return;
        }


        string strPlatform = EditorUserBuildSettings.activeBuildTarget==BuildTarget.iOS ? "ios" : "android";
        string LOCAL_BUNDLE_BUILD_PATH = Path.GetFullPath(Application.dataPath + "/Bundles/");
        

        
        GUILayout.Space(10);

        var dirInfo = new DirectoryInfo(LOCAL_BUNDLE_BUILD_PATH);


        if (GUILayout.Button("Build Bundle", GUILayout.Width(200)))
        {
            string assetOutputPath = LOCAL_BUNDLE_BUILD_PATH + "../../AssetBundleOutputs/";//  + fi.Name + "/";
            Debug.Log($"Building AssetBundle..[{assetOutputPath}]");



            //==================================================================//
            //
            // Tagging Bundle Names. 
            //
            ClearAllAssetBundleTags();
 
            DirectoryInfo[] arrDirInfo = dirInfo.GetDirectories();
            for(int k = 0; k < arrDirInfo.Length; ++k)
            {
                DirectoryInfo fi = arrDirInfo[k];
                AssetImporter importer = AssetImporter.GetAtPath("Assets/Bundles/" + fi.Name);
                if (importer != null)
                {
                    importer.assetBundleName = fi.Name.ToLower();
                    importer.SaveAndReimport();
                }
            }

            string buildPath = $"{assetOutputPath}{strPlatform}";
            if (!Directory.Exists(buildPath))
                Directory.CreateDirectory(buildPath);
            
            var assetManifest = BuildPipeline.BuildAssetBundles(buildPath+"/", BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
            if (assetManifest != null)
            {
                ManifestInfo info = new ManifestInfo();
                info.header = new HeaderInfo();
                info.header.date = DateTime.UtcNow.ToShortTimeString() + " " + DateTime.UtcNow.ToShortDateString();

                info.bundles = new List<BundleInfo>();

                foreach (var bundleName in assetManifest.GetAllAssetBundles())
                {
                    BundleInfo bundleInfo = new BundleInfo();
                    bundleInfo.name = bundleName;
                    bundleInfo.hash = assetManifest.GetAssetBundleHash(bundleName).ToString();
                    bundleInfo.path = $"assetbundles/{strPlatform}/{bundleName}/{bundleInfo.hash}";

                    string bundleP = $"{assetOutputPath}{strPlatform}/{bundleName}";
                    bundleInfo.fileSize = new FileInfo(bundleP).Length;

                    info.bundles.Add(bundleInfo);

                    //string projectRelativePath = buildInput.outputPath + "/" + bundleName;
                    //Debug.Log($"Size of AssetBundle {projectRelativePath} is {new FileInfo(projectRelativePath).Length}");
                    Debug.Log(bundleName);
                }

                string fileName = $"assetManifest-{strPlatform}_{Application.version}.json";
                string manifestPath = assetOutputPath + fileName;

                if (Directory.Exists(manifestPath))
                    Directory.Delete(manifestPath, true);
                
                string strJson = JsonUtility.ToJson(info, prettyPrint : true);
                TextWriter manifestWriter = new StreamWriter(manifestPath);
                manifestWriter.Write( strJson );
                manifestWriter.Close();

            }
            else Debug.LogError("Building Bundle has been failed....! : " + assetOutputPath);

            // Re-clearing.
            ClearAllAssetBundleTags();
        


            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();
            {
                foreach (DirectoryInfo fi in dirInfo.GetDirectories())
                {
                    GUILayout.Label($"* {fi.Name} ", GUILayout.Width(150));
                }
            }
            EditorGUILayout.EndVertical();
        }
    }


    [MenuItem("BenevolendMed/Tools/Clear Asset Tags")]
    private static void ClearAllAssetBundleTags()
    {
        var usedAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        foreach (var name in usedAssetBundleNames)
        {
            AssetDatabase.RemoveAssetBundleName(name, true);
        }
    }
}
