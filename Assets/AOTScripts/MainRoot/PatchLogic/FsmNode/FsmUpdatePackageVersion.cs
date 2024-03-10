using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniFramework.Machine;
using YooAsset;

/// <summary>
/// 更新资源版本号
/// </summary>
internal class FsmUpdatePackageVersion : IStateNode
{
    private StateMachine _machine;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }

    void IStateNode.OnEnter()
    {
        PatchEventDefine.PatchStatesChange.SendEventMessage("获取最新的资源版本 !");
        UpdatePackageVersion().Forget();
    }

    void IStateNode.OnUpdate()
    {
    }

    void IStateNode.OnExit()
    {
    }

    private async UniTask UpdatePackageVersion()
    {
        var defaultPackageName = (string)_machine.GetBlackboardValue("DefaultPackageName");
        var defaultPackage = YooAssets.GetPackage(defaultPackageName);
        var operation = defaultPackage.UpdatePackageVersionAsync();
        var rawPackageName = (string)_machine.GetBlackboardValue("RawPackageName");
        var rawPackage = YooAssets.GetPackage(rawPackageName);
        var rawOperation = rawPackage.UpdatePackageVersionAsync();
        // 等待异步操作完成
        await UniTask.WaitUntil(() => operation.IsDone);
        await UniTask.WaitUntil(() => rawOperation.IsDone);

        if (operation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning(operation.Error);
            PatchEventDefine.PackageVersionUpdateFailed.SendEventMessage();
        }
        else if (rawOperation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning(rawOperation.Error);
            PatchEventDefine.PackageVersionUpdateFailed.SendEventMessage();
        }
        else
        {
            _machine.SetBlackboardValue("DefaultPackageVersion", operation.PackageVersion);
            _machine.SetBlackboardValue("RawPackageVersion", rawOperation.PackageVersion);
            _machine.ChangeState<FsmUpdatePackageManifest>();
        }
    }
}