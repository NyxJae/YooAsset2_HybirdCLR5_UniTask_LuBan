using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;

namespace Editor
{
    public partial class NFrameworkTools
    {
        private static AssetBundleCollectorSetting _assetBundleCollectorSetting;


        static List<CollectAssetInfo> _collectAssetInfoList;

        [MenuItem("NFramwarkTools/GenerateAssetsName")]
        public static void GenerateAssetsName()
        {
            if (!FindAssetBundleCollectorSetting()) return;
            var stringBuilder = new System.Text.StringBuilder();
            stringBuilder.AppendLine("// 该文件由工具自动生成，请勿手动修改");
            stringBuilder.AppendLine("public static class AssetsName");
            stringBuilder.AppendLine("{");
            foreach (AssetBundleCollectorPackage package in _assetBundleCollectorSetting.Packages) // 遍历所有的包
            {
                foreach (AssetBundleCollectorGroup group in package.Groups) // 遍历所有的组
                {
                    stringBuilder.AppendLine($"    public static class {group.GroupName}"); // 创建一个组类
                    stringBuilder.AppendLine("    {");
                    foreach (AssetBundleCollector collector in group.Collectors) // 遍历所有的收集器
                    {
                        var command = new CollectCommand(EBuildMode.SimulateBuild, package.PackageName,
                            package.EnableAddressable, package.LocationToLower, package.IncludeAssetGUID,
                            package.IgnoreDefaultType, package.AutoCollectShaders, false); // 创建一个收集命令
                        _collectAssetInfoList = collector.GetAllCollectAssets(command, group); // 获取所有的收集资源信息
                        foreach (var collectAssetInfo in _collectAssetInfoList) // 遍历所有的收集资源信息
                        {
                            // 将资源信息的地址转换为常量名 并输出到文件
                            stringBuilder.AppendLine(
                                $"        public const string {collectAssetInfo.Address.Replace("-", "_").Replace(".", "_").ToUpper()} =  \"{collectAssetInfo.Address}\";");
                        }
                    }

                    stringBuilder.AppendLine("    }"); // 结束组类
                }
            }

            stringBuilder.AppendLine("}"); // 结束AssetsName类
            // 将stringBuilder输出到文件 AssetsNames.cs
            File.WriteAllText("Assets/HotUpdateScripts/Gen/AssetsName.cs", stringBuilder.ToString());
            // 刷新资源
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 查找AssetBundleCollectorSetting文件
        /// </summary>
        private static bool FindAssetBundleCollectorSetting()
        {
            // 使用AssetDatabase查找所有的.asset文件
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (string guid in guids)
            {
                // 获取文件的路径
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                // 检查是否为目标文件
                if (Path.GetFileName(assetPath) == "AssetBundleCollectorSetting.asset")
                {
                    // 输出文件路径，并选中该文件
                    Debug.Log("AssetBundleCollectorSetting.asset found at: " + assetPath);
                    _assetBundleCollectorSetting =
                        AssetDatabase.LoadAssetAtPath<AssetBundleCollectorSetting>(assetPath);
                    return true;
                }
            }

            // 如果没有找到，输出未找到文件的信息
            Debug.Log("AssetBundleCollectorSetting.asset not found.");
            return false;
        }
    }
}