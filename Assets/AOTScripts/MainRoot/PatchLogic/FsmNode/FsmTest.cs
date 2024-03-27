using System.Linq;
using Cysharp.Threading.Tasks;
using UniFramework.Machine;
using UnityEngine;
using YooAsset;

/// <summary>
/// 测试资源热更与代码热更节点
/// </summary>
public class FsmTest : IStateNode
{
    private StateMachine _machine;

    public void OnCreate(StateMachine machine)
    {
        _machine = machine;
    }

    public void OnEnter()
    {
        Test().Forget();
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
    }

    private async UniTask Test()
    {
        var package = YooAssets.TryGetPackage(_machine.GetBlackboardValue("DefaultPackageName") as string);
        if (package == null) Debug.Log("包获取失败");
        var location = "TestTools_TestObj";

        var handle = package?.LoadAssetAsync<GameObject>(location);
        await handle.ToUniTask();
        var obj = handle?.InstantiateAsync();
        _machine.ChangeState<FsmClearPackageCache>();
    }
}