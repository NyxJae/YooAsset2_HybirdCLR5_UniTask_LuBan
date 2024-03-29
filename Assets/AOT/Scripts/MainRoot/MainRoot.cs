using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace NFramework
{
    /// <summary>
    ///     主入口 包含资源系统初始化 热更新初始化
    /// </summary>
    public class MainRoot : MonoBehaviour
    {
        private async void Start()
        {
            // 开始更新资源流程
            var operation = new PatchOperation();
            YooAssets.StartOperation(operation);
            await operation;
            // 以下写GamePlay入口逻辑
            Debug.Log("资源更新完毕");
            var testObj = YooAssets.LoadAssetSync<GameObject>("TestTools_TestObj");
            await testObj.ToUniTask();
            var obj = testObj.InstantiateAsync();
        }
    }
}