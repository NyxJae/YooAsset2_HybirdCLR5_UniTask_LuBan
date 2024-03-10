using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using YooAsset;

namespace NFramework
{
    /// <summary>
    ///     主入口 包含资源系统初始化 热更新初始化
    /// </summary>
    public class MainRoot : MonoBehaviour
    {
        /// <summary>
        ///     是否打开测试节点,用以测试资源更新和代码热更新
        /// </summary>
        [SerializeField] [Header("是否打开测试节点")] public bool isTest = false;

        private async void Start()
        {
            // 开始更新资源流程
            var operation = new PatchOperation(isTest);
            YooAssets.StartOperation(operation);
            await operation;
        }
    }
}