using UnityEditor;
using UnityEngine; // 引入Unity编辑器命名空间

public class EditorMenuExtensions
{
    // 在Unity编辑器的菜单栏添加"MoveDlls/MoveDll"菜单项
    [MenuItem("MoveDlls/MoveDlls")]
    private static void MoveDlls()
    {
        // Dlls文件要转移到的文件位置
        string targetPath = "Assets/HotUpdateAssets/HotUpdateDlls";
        // 清空目标文件夹内的所有文件
        System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(targetPath);
        // 删除文件夹内的所有文件
        foreach (System.IO.FileInfo file in dir.GetFiles())
        {
            file.Delete();
        }

        // 删除文件夹内的所有子文件夹
        foreach (System.IO.DirectoryInfo subDir in dir.GetDirectories())
        {
            subDir.Delete(true);
        }

        // Dlls文件的原始位置
        string sourcePath = "HybridCLRData/HotUpdateDlls/StandaloneWindows64";
        // 读取HotUpdateConfig配置文件
        var hotUpdateConfig = Resources.Load<HotUpdateConfig>("HotUpdateConfig");
        // 获取热更DLL文件列表
        var hotUpdateDlls = hotUpdateConfig.hotUpdateDlls;
        // 遍历热更DLL文件列表
        foreach (var hotUpdateDll in hotUpdateDlls)
        {
            // 拼接热更DLL文件的原始路径
            string sourceFilePath = $"{sourcePath}/{hotUpdateDll}";
            // 拼接热更DLL文件的目标路径
            string targetFilePath = $"{targetPath}/{hotUpdateDll}";
            // 将热更DLL文件从原始路径移动到目标路径
            System.IO.File.Copy(sourceFilePath, targetFilePath);
            // 在Dlls文件名后面加上".bytes"后缀
            System.IO.File.Move(targetFilePath, targetFilePath + ".bytes");
        }

        // 打印日志
        UnityEngine.Debug.Log("热更DLL文件已经移动！");
    }
}