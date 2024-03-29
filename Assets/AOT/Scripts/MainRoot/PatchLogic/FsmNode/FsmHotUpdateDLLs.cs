using System;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UniFramework.Machine;
using UnityEngine;
using YooAsset;

/// <summary>
/// 热更新程序集DLLs
/// </summary>
public class FsmHotUpdateDLLs : IStateNode
{
    private StateMachine _machine;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }

    void IStateNode.OnEnter()
    {
        HotUpdate().Forget();
    }

    void IStateNode.OnUpdate()
    {
    }

    void IStateNode.OnExit()
    {
    }

    // 热更新 补充元数据和加载热更新程序集
    private async UniTask HotUpdate()
    {
        // 从resource中读取配置
        var hotUpdateConfig = Resources.Load<HotUpdateConfig>("HotUpdateConfig");
        var aotMetaAssemblyFiles = hotUpdateConfig.patchedAOTAssemblyList;
        var hotUpdateDlls = hotUpdateConfig.hotUpdateDlls;
        // 先补充元数据
        foreach (var aotDllName in aotMetaAssemblyFiles)
        {
            // 使用给定的名称异步加载资产以初始化AssetHandle对象
            AssetHandle handle = YooAssets.LoadAssetAsync(aotDllName);

            // 等待资产加载操作完成
            await handle.ToUniTask();

            // 从加载的资产中检索字节数组，假设它是一个TextAsset
            byte[] dllBytes = ((TextAsset)handle.AssetObject).bytes;

            // 如果字节数组为null，则跳过当前循环的迭代
            if (dllBytes == null) continue;

            // 使用字节数组加载AOT程序集的元数据
            // HomologousImageMode.SuperSet参数表示元数据图像是运行时图像的超集
            var err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HomologousImageMode.SuperSet);
        }

        // 再加载热更新程序集
        foreach (var hotUpdateDllName in hotUpdateDlls)
        {
            var dataHandle = YooAssets.LoadAssetAsync(hotUpdateDllName);
            await dataHandle.ToUniTask();
            if (dataHandle.Status != EOperationStatus.Succeed)
            {
                Debug.Log("资源加载失败" + hotUpdateDllName);
                return;
            }

            var dllData = ((TextAsset)dataHandle.AssetObject).bytes;
            if (dllData == null)
            {
                Debug.Log("获取Dll数据失败");
                return;
            }

            var assembly = Assembly.Load(dllData);
        }

        Debug.Log("代码热更新完成");
        _machine.ChangeState<FsmClearPackageCache>();
    }
}