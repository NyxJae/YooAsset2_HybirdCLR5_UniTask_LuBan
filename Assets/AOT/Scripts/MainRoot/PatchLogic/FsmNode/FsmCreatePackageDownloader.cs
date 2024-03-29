using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniFramework.Machine;
using YooAsset;

/// <summary>
/// 创建文件下载器
/// </summary>
public class FsmCreatePackageDownloader : IStateNode
{
    private StateMachine _machine;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }

    void IStateNode.OnEnter()
    {
        PatchEventDefine.PatchStatesChange.SendEventMessage("创建补丁下载器！");
        CreateDownloader();
    }

    void IStateNode.OnUpdate()
    {
    }

    void IStateNode.OnExit()
    {
    }

    private void CreateDownloader()
    {
        var defaultPackageName = (string)_machine.GetBlackboardValue("DefaultPackageName");
        var defaultPackage = YooAssets.GetPackage(defaultPackageName);
        var downloadingMaxNum = 10;
        var failedTryAgain = 3;
        var downloader = defaultPackage.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
        _machine.SetBlackboardValue("Downloader", downloader);

        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log("Not found any download files !");
            _machine.ChangeState<FsmDownloadPackageOver>();
        }
        else
        {
            // 发现新更新文件后，挂起流程系统
            // 注意：开发者需要在下载前检测磁盘空间不足
            var totalDownloadCount = downloader.TotalDownloadCount;
            var totalDownloadBytes = downloader.TotalDownloadBytes;
            PatchEventDefine.FoundUpdateFiles.SendEventMessage(totalDownloadCount, totalDownloadBytes);
            // 下载文件
            _machine.ChangeState<FsmDownloadPackageFiles>();
        }
    }
}