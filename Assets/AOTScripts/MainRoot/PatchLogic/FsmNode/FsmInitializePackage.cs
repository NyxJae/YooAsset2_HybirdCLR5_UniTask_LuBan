using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniFramework.Machine;
using YooAsset;

/// <summary>
/// 初始化资源包
/// </summary>
internal class FsmInitializePackage : IStateNode
{
    private StateMachine _machine;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }

    void IStateNode.OnEnter()
    {
        PatchEventDefine.PatchStatesChange.SendEventMessage("初始化资源包！");
        // 启动Unitask InitPackage
        InitDefaultPackage().Forget();
    }

    void IStateNode.OnUpdate()
    {
    }

    void IStateNode.OnExit()
    {
    }

    private async UniTask InitDefaultPackage()
    {
        var playMode = (EPlayMode)_machine.GetBlackboardValue("PlayMode");
        var defaultPackageName = (string)_machine.GetBlackboardValue("DefaultPackageName");
        var rawPackageName = (string)_machine.GetBlackboardValue("RawPackageName");
        // 初始化YooAssets资源管理系统
        YooAssets.Initialize();
        // 创建默认资源包裹类
        var defaultPackage = YooAssets.TryGetPackage(defaultPackageName);
        if (defaultPackage == null)
            defaultPackage = YooAssets.CreatePackage(defaultPackageName);
        // 设置默认资源包裹类
        YooAssets.SetDefaultPackage(defaultPackage);
        // 创建原始资源包裹类
        var rawPackage = YooAssets.TryGetPackage(rawPackageName);
        if (rawPackage == null)
            rawPackage = YooAssets.CreatePackage(rawPackageName);

        InitializationOperation defaultinitializationOperation = null;
        InitializationOperation rawinitializationOperation = null;
        // 编辑器下的模拟模式
        if (playMode == EPlayMode.EditorSimulateMode)
        {
            var defaultCreateParameters = new EditorSimulateModeParameters();
            defaultCreateParameters.SimulateManifestFilePath =
                EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.BuiltinBuildPipeline.ToString(),
                    defaultPackageName);
            var rawCreateParameters = new EditorSimulateModeParameters();
            rawCreateParameters.SimulateManifestFilePath =
                EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.RawFileBuildPipeline.ToString(),
                    rawPackageName);
            defaultinitializationOperation = defaultPackage.InitializeAsync(defaultCreateParameters);
            rawinitializationOperation = rawPackage.InitializeAsync(rawCreateParameters);
        }

        // 单机运行模式
        else if (playMode == EPlayMode.OfflinePlayMode)
        {
            var createParameters = new OfflinePlayModeParameters();
            createParameters.DecryptionServices = new FileStreamDecryption();
            defaultinitializationOperation = defaultPackage.InitializeAsync(createParameters);
            rawinitializationOperation = rawPackage.InitializeAsync(createParameters);
        }

        // 联机运行模式
        else if (playMode == EPlayMode.HostPlayMode)
        {
            var defaultHostServer = GetHostServerURL();
            var fallbackHostServer = GetHostServerURL();
            var createParameters = new HostPlayModeParameters();
            createParameters.DecryptionServices = new FileStreamDecryption();
            createParameters.BuildinQueryServices = new GameQueryServices();
            createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            defaultinitializationOperation = defaultPackage.InitializeAsync(createParameters);
            rawinitializationOperation = rawPackage.InitializeAsync(createParameters);
        }

        // WebGL运行模式
        if (playMode == EPlayMode.WebPlayMode)
        {
            var defaultHostServer = GetHostServerURL();
            var fallbackHostServer = GetHostServerURL();
            var createParameters = new WebPlayModeParameters();
            // WebGL平台不支持加密
            createParameters.DecryptionServices = null;
            createParameters.BuildinQueryServices = new GameQueryServices();
            createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            defaultinitializationOperation = defaultPackage.InitializeAsync(createParameters);
            // WebGL平台不支持多package
        }

        // 等待异步初始化操作完成
        await UniTask.WaitUntil(() => defaultinitializationOperation.IsDone);
        await UniTask.WaitUntil(() => rawinitializationOperation.IsDone);

        // 如果初始化失败弹出提示界面
        if (defaultinitializationOperation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning($"{defaultinitializationOperation.Error}");
            PatchEventDefine.InitializeFailed.SendEventMessage();
        }
        else if (rawinitializationOperation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning($"{rawinitializationOperation.Error}");
            PatchEventDefine.InitializeFailed.SendEventMessage();
        }
        else
        {
            var version = defaultinitializationOperation.PackageVersion;
            // Debug.Log($"Init resource package version : {version}");
            _machine.ChangeState<FsmUpdatePackageVersion>();
        }
    }


    /// <summary>
    /// 获取资源服务器地址
    /// </summary>
    private string GetHostServerURL()
    {
        //string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
        var hostServerIP = _machine.GetBlackboardValue("HostServerIP") as string;
        var appVersion = _machine.GetBlackboardValue("PackageVersion") as string;

#if UNITY_EDITOR
        if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
            return $"{hostServerIP}/CDN/Android/{appVersion}";
        else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
            return $"{hostServerIP}/CDN/IPhone/{appVersion}";
        else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
            return $"{hostServerIP}/CDN/WebGL/{appVersion}";
        else
            return $"{hostServerIP}/CDN/PC/{appVersion}";
#else
        if (Application.platform == RuntimePlatform.Android)
            return $"{hostServerIP}/CDN/Android/{appVersion}";
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
            return $"{hostServerIP}/CDN/IPhone/{appVersion}";
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
            return $"{hostServerIP}/CDN/WebGL/{appVersion}";
        else
            return $"{hostServerIP}/CDN/PC/{appVersion}";
#endif
    }

    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    private class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }

        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }

        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }

    /// <summary>
    /// 资源文件流加载解密类
    /// </summary>
    private class FileStreamDecryption : IDecryptionServices
    {
        /// <summary>
        /// 同步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        AssetBundle IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            var bundleStream = new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            managedStream = bundleStream;
            return AssetBundle.LoadFromStream(bundleStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
        }

        /// <summary>
        /// 异步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        AssetBundleCreateRequest IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo,
            out Stream managedStream)
        {
            var bundleStream = new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            managedStream = bundleStream;
            return AssetBundle.LoadFromStreamAsync(bundleStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
        }

        private static uint GetManagedReadBufferSize()
        {
            return 1024;
        }
    }

    /// <summary>
    /// 资源文件偏移加载解密类
    /// </summary>
    private class FileOffsetDecryption : IDecryptionServices
    {
        /// <summary>
        /// 同步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        AssetBundle IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            managedStream = null;
            return AssetBundle.LoadFromFile(fileInfo.FileLoadPath, fileInfo.ConentCRC, GetFileOffset());
        }

        /// <summary>
        /// 异步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        AssetBundleCreateRequest IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo,
            out Stream managedStream)
        {
            managedStream = null;
            return AssetBundle.LoadFromFileAsync(fileInfo.FileLoadPath, fileInfo.ConentCRC, GetFileOffset());
        }

        private static ulong GetFileOffset()
        {
            return 32;
        }
    }
}

/// <summary>
/// 资源文件解密流
/// </summary>
public class BundleStream : FileStream
{
    public const byte KEY = 64;

    public BundleStream(string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode, access,
        share)
    {
    }

    public BundleStream(string path, FileMode mode) : base(path, mode)
    {
    }

    public override int Read(byte[] array, int offset, int count)
    {
        var index = base.Read(array, offset, count);
        for (var i = 0; i < array.Length; i++) array[i] ^= KEY;
        return index;
    }
}