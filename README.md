# 整合YooAsset2+HybirdCLR5+UniTask 的项目模板
## 项目介绍
- 2024年初
- 项目使用了HybirdCLR5作为热更新框架
- 项目使用了YooAsset2作为资源管理框架
- 项目使用了UniTask作为异步框架
- 只进行里面的代码整合，几乎没有其他冗余代码
- 尚未添加其他工具
## 重点文件
- [Assets/AOTScripts](Assets/AOTScripts) AOT代码
- [HotUpdateScripts](Assets/HotUpdateScripts) 热更新代码
- [HotUpdateConfig.asset](Assets/Resources/HotUpdateConfig.asset) 热更新配置
- [AOTGenericReferences.cs](Assets/HybridCLRGenerate/AOTGenericReferences.cs) 补元DLL列表
- [FsmInitializePackage.cs](Assets/AOTScripts/MainRoot/PatchLogic/FsmNode/FsmInitializePackage.cs) 热更新初始化服务器地址与解密配置
- [Test.cs](Assets%2FHotUpdateScripts%2FTestTools%2FTest.cs)测试代码