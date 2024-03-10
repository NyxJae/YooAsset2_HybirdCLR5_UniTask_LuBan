using UnityEngine;
using UnityEngine.Serialization;
using YooAsset;

[CreateAssetMenu(fileName = "HotUpdateConfig", menuName = "HotUpdate/HotUpdateConfig")]
public class HotUpdateConfig : ScriptableObject
{
    /// <summary>
    ///     默认资源包名
    /// </summary>
    [Header("默认的资源包名")] [Tooltip("默认的资源包名")]
    public string defaultPackageName = "DefaultPackage";

    /// <summary>
    ///     原生资源包名
    /// </summary>
    [Header("热更原生资源包名")] [Tooltip("热更原生资源包名")]
    public string rawPackageName = "RawFilePackage";

    /// <summary>
    ///     资源系统运行模式
    /// </summary>
    [Header("资源系统运行模式")] [Tooltip("资源系统运行模式")]
    public EPlayMode playMode = EPlayMode.EditorSimulateMode;

    /// <summary>
    ///     服务器地址
    /// </summary>
    [Header("服务器地址")] [Tooltip("服务器地址")] public string hostServerIP = "http://192.168.0.4/";

    /// <summary>
    ///     版本号
    /// </summary>
    [Header("版本号")] [Tooltip("版本号")] public string packageVersion = "v1.0.0";

    /// <summary>
    ///    热更新补元数据的程序集Dll列表
    /// </summary>
    [Header("热更新补元数据的程序集Dll列表")] [Tooltip("热更新补元数据的程序集Dll列表")]
    public string[] patchedAOTAssemblyList = new string[]
    {
        "mscorlib.dll"
    };

    /// <summary>
    ///     热更新程序集Dll名字列表
    /// </summary>
    [Header("热更新程序集Dll名字列表")] [Tooltip("热更新程序集Dll名字列表")]
    public string[] hotUpdateDlls = new string[]
    {
        "Assembly-CSharp.dll"
        // "HotUpdate.dll"
    };
}