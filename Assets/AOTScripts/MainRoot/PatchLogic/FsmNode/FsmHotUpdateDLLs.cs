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
            AssetHandle handle = YooAssets.LoadAssetAsync(aotDllName);
            await handle.ToUniTask();
            byte[] dllBytes = ((TextAsset)handle.AssetObject).bytes;
            if (dllBytes == null) continue;
            var err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HomologousImageMode.SuperSet);
            Debug.Log($"加载 AOT 程序集的元数据:{aotDllName}. ret:{err}");
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
            Debug.Log($"加载热更新Dll:{hotUpdateDllName}");
        }

        // 如果是测试模式，切换到测试节点
        if ((bool)_machine.GetBlackboardValue("IsTest"))
            _machine.ChangeState<FsmTest>();
        else
            _machine.ChangeState<FsmClearPackageCache>();
    }
}