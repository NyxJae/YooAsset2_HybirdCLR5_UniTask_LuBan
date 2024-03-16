using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniFramework.Machine;
using YooAsset;

/// <summary>
/// 更新资源清单
/// </summary>
public class FsmUpdatePackageManifest : IStateNode
{
    private StateMachine _machine;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }

    void IStateNode.OnEnter()
    {
        PatchEventDefine.PatchStatesChange.SendEventMessage("更新资源清单！");
        UpdateManifest().Forget();
    }

    void IStateNode.OnUpdate()
    {
    }

    void IStateNode.OnExit()
    {
    }

    /// <summary>
    /// 异步更新资源包清单
    /// </summary>
    private async UniTask UpdateManifest()
    {
        var defaultPackageName = (string)_machine.GetBlackboardValue("DefaultPackageName");
        var defaultPackageVersion = (string)_machine.GetBlackboardValue("DefaultPackageVersion");
        var defaultPackage = YooAssets.GetPackage(defaultPackageName);
        var savePackageVersion = true;
        var defaultOperation = defaultPackage.UpdatePackageManifestAsync(defaultPackageVersion, savePackageVersion);
        // 等待异步操作完成
        await UniTask.WaitUntil(() => defaultOperation.IsDone);

        if (defaultOperation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning(defaultOperation.Error);
            PatchEventDefine.PatchManifestUpdateFailed.SendEventMessage();
        }
        else
        {
            _machine.ChangeState<FsmCreatePackageDownloader>();
        }
    }
}