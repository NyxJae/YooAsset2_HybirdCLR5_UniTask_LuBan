using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using UniFramework.Event;
using YooAsset;

/// <summary>
/// 补丁操作
/// </summary>
public class PatchOperation : GameAsyncOperation
{
    private readonly EventGroup _eventGroup = new();
    private readonly StateMachine _machine;
    private ESteps _steps = ESteps.None;

    public PatchOperation(bool isTest)
    {
        // 注册监听事件
        _eventGroup.AddListener<UserEventDefine.UserTryInitialize>(OnHandleEventMessage);
        _eventGroup.AddListener<UserEventDefine.UserBeginDownloadWebFiles>(OnHandleEventMessage);
        _eventGroup.AddListener<UserEventDefine.UserTryUpdatePackageVersion>(OnHandleEventMessage);
        _eventGroup.AddListener<UserEventDefine.UserTryUpdatePatchManifest>(OnHandleEventMessage);
        _eventGroup.AddListener<UserEventDefine.UserTryDownloadWebFiles>(OnHandleEventMessage);

        // 创建状态机
        _machine = new StateMachine(this);
        _machine.AddNode<FsmInitializePackage>();
        _machine.AddNode<FsmUpdatePackageVersion>();
        _machine.AddNode<FsmUpdatePackageManifest>();
        _machine.AddNode<FsmCreatePackageDownloader>();
        _machine.AddNode<FsmDownloadPackageFiles>();
        _machine.AddNode<FsmDownloadPackageOver>();
        _machine.AddNode<FsmHotUpdateDLLs>();
        _machine.AddNode<FsmTest>();
        _machine.AddNode<FsmClearPackageCache>();
        _machine.AddNode<FsmUpdaterDone>();

        // 从resource中读取配置
        var hotUpdateConfig = Resources.Load<HotUpdateConfig>("HotUpdateConfig");
        var playMode = hotUpdateConfig.playMode;


#if !UNITY_EDITOR
        if (playMode != EPlayMode.HostPlayMode)
        {
            playMode = EPlayMode.HostPlayMode;
            Debug.Log($"检测到真机运行,已切换运行模式至：{playMode}");
        }
#endif
        _machine.SetBlackboardValue("DefaultPackageName", hotUpdateConfig.defaultPackageName);
        _machine.SetBlackboardValue("PlayMode", playMode);
        _machine.SetBlackboardValue("HostServerIP", hotUpdateConfig.hostServerIP);
        _machine.SetBlackboardValue("PackageVersion", hotUpdateConfig.packageVersion);
        _machine.SetBlackboardValue("IsTest", isTest);
    }

    protected override void OnStart()
    {
        _steps = ESteps.Update;
        _machine.Run<FsmInitializePackage>();
    }

    protected override void OnUpdate()
    {
        if (_steps is ESteps.None or ESteps.Done)
            return;

        if (_steps == ESteps.Update)
        {
            _machine.Update();
            if (_machine.CurrentNode == typeof(FsmUpdaterDone).FullName)
            {
                _eventGroup.RemoveAllListener();
                Status = EOperationStatus.Succeed;
                _steps = ESteps.Done;
            }
        }
    }

    protected override void OnAbort()
    {
    }

    /// <summary>
    /// 接收事件
    /// </summary>
    private void OnHandleEventMessage(IEventMessage message)
    {
        if (message is UserEventDefine.UserTryInitialize)
            _machine.ChangeState<FsmInitializePackage>();
        else if (message is UserEventDefine.UserBeginDownloadWebFiles)
            _machine.ChangeState<FsmDownloadPackageFiles>();
        else if (message is UserEventDefine.UserTryUpdatePackageVersion)
            _machine.ChangeState<FsmUpdatePackageVersion>();
        else if (message is UserEventDefine.UserTryUpdatePatchManifest)
            _machine.ChangeState<FsmUpdatePackageManifest>();
        else if (message is UserEventDefine.UserTryDownloadWebFiles)
            _machine.ChangeState<FsmCreatePackageDownloader>();
        else
            throw new NotImplementedException($"{message.GetType()}");
    }

    private enum ESteps
    {
        None,
        Update,
        Done
    }
}