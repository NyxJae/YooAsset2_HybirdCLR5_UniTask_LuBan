using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniFramework.Machine;
using YooAsset;

/// <summary>
/// 下载更新文件
/// </summary>
public class FsmDownloadPackageFiles : IStateNode
{
    private StateMachine _machine;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }

    void IStateNode.OnEnter()
    {
        PatchEventDefine.PatchStatesChange.SendEventMessage("开始下载补丁文件！");
        BeginDownload().Forget();
    }

    void IStateNode.OnUpdate()
    {
    }

    void IStateNode.OnExit()
    {
    }

    private async UniTask BeginDownload()
    {
        var downloader = (ResourceDownloaderOperation)_machine.GetBlackboardValue("Downloader");
        downloader.OnDownloadErrorCallback = PatchEventDefine.WebFileDownloadFailed.SendEventMessage;
        downloader.OnDownloadProgressCallback = PatchEventDefine.DownloadProgressUpdate.SendEventMessage;
        downloader.BeginDownload();
        // 等待下载完成
        await UniTask.WaitUntil(() => downloader.IsDone);
        var rawDownloader = (ResourceDownloaderOperation)_machine.GetBlackboardValue("RawDownloader");
        rawDownloader.OnDownloadErrorCallback = PatchEventDefine.WebFileDownloadFailed.SendEventMessage;
        rawDownloader.OnDownloadProgressCallback = PatchEventDefine.DownloadProgressUpdate.SendEventMessage;
        rawDownloader.BeginDownload();
        // 等待下载完成
        await UniTask.WaitUntil(() => rawDownloader.IsDone);

        // 检测下载结果
        if (downloader.Status != EOperationStatus.Succeed || rawDownloader.Status != EOperationStatus.Succeed)
            return;

        _machine.ChangeState<FsmDownloadPackageOver>();
    }
}