using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class EditorMenuExtensions
    {
        /// <summary>
        /// 热更新配置文件
        /// </summary>
        private static HotUpdateConfig _hotUpdateConfig;

        // 在Unity编辑器的菜单栏添加"MoveDlls/MoveDll"菜单项
        [MenuItem("MoveDlls/MoveDlls")]
        private static void MoveDlls()
        {
            // 加载HotUpdateConfig配置
            _hotUpdateConfig = Resources.Load<HotUpdateConfig>("HotUpdateConfig");
            var patchedAOTAssemblyList = AOTGenericReferences.PatchedAOTAssemblyList;
            // 创建一个与只读列表长度相同的数组
            string[] array = new string[patchedAOTAssemblyList.Count];
            // 通过循环复制元素
            for (int i = 0; i < patchedAOTAssemblyList.Count; i++)
            {
                array[i] = patchedAOTAssemblyList[i];
            }

            // 将热更DLL文件列表赋值给HotUpdateConfig配置文件
            _hotUpdateConfig.patchedAOTAssemblyList = array;

            MoveDllsToTarget("Assets/HotUpdateAssets/HotUpdateDlls",
                "HybridCLRData/HotUpdateDlls/StandaloneWindows64",
                _hotUpdateConfig.hotUpdateDlls);

            MoveDllsToTarget("Assets/HotUpdateAssets/AssembliesPostIl2CppStrip",
                "HybridCLRData/AssembliesPostIl2CppStrip/StandaloneWindows64",
                _hotUpdateConfig.patchedAOTAssemblyList);

            // 保存对配置文件的更改
            EditorUtility.SetDirty(_hotUpdateConfig);
            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 将DLL文件从源目录移动到目标目录，并更改扩展名为.bytes
        /// </summary>
        /// <param name="targetPath">目标目录路径</param>
        /// <param name="sourcePath">源目录路径</param>
        /// <param name="dllNames">dll文件名称列表</param>
        private static void MoveDllsToTarget(string targetPath, string sourcePath, IReadOnlyList<string> dllNames)
        {
            ClearDirectory(targetPath);
            foreach (var dllName in dllNames)
            {
                try
                {
                    string sourceFilePath = Path.Combine(sourcePath, dllName);
                    string targetFilePath = Path.Combine(targetPath, dllName) + ".bytes";
                    File.Copy(sourceFilePath, targetFilePath, true);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"移动文件时出错: {ex.Message}");
                }
            }

            Debug.Log($"DLL文件已经移动到 {targetPath} 并更改为.bytes扩展名！");
        }

        /// <summary>
        /// 清空指定的目录，包括所有文件和子目录
        /// </summary>
        /// <param name="path">目录路径</param>
        private static void ClearDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (FileInfo file in dir.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                subDir.Delete(true);
            }
        }
    }
}