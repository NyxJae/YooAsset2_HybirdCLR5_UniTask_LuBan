using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UniFramework.Machine;
using YooAsset;

/// <summary>
/// 清理未使用的缓存文件
/// </summary>
internal class FsmClearPackageCache : IStateNode
{
    private StateMachine _machine;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }

    void IStateNode.OnEnter()
    {
        PatchEventDefine.PatchStatesChange.SendEventMessage("清理未使用的缓存文件！");
        var defaultPackageName = (string)_machine.GetBlackboardValue("DefaultPackageName");
        var package = YooAssets.GetPackage(defaultPackageName);
        var operation = package.ClearUnusedCacheFilesAsync();
        var rawPackageName = (string)_machine.GetBlackboardValue("RawPackageName");
        var rawPackage = YooAssets.GetPackage(rawPackageName);
        var rawOperation = rawPackage.ClearUnusedCacheFilesAsync();
        operation.Completed += Operation_Completed;
        rawOperation.Completed += Operation_Completed;
    }

    void IStateNode.OnUpdate()
    {
    }

    void IStateNode.OnExit()
    {
    }

    private void Operation_Completed(AsyncOperationBase obj)
    {
        _machine.ChangeState<FsmUpdaterDone>();
    }
}