# 整合YooAsset2+HybirdCLR5+UniTask_LuBan 的项目模板
## 项目介绍
- 2024年初
- 项目使用了HybirdCLR5作为热更新框架
- 项目使用了YooAsset2作为资源管理框架
- 项目使用了UniTask作为异步框架
- 项目使用了LuBan作为配置表框架
- 只进行里面的代码整合，几乎没有其他冗余代码
- 菜单栏添加MoveDlls工具与GenerateAssetsName工具
## 重点文件
- [AOT](Assets%2FAOT) AOT文件夹(该文件夹外的其实都是热更)
- [Assets/HotUpdateScripts](Assets/HotUpdateScripts) 热更新代码文件夹(推荐将热更代码写在这里)
- [HotUpdateAssets](Assets%2FHotUpdateAssets) 热更新资源文件夹
- [HotUpdateConfig.asset](Assets%2FAOT%2FResources%2FHotUpdateConfig.asset) 热更新配置
- [AOTGenericReferences.cs](Assets/HybridCLRGenerate/AOTGenericReferences.cs) 补元DLL列表
- [FsmInitializePackage.cs](Assets%2FAOT%2FScripts%2FMainRoot%2FPatchLogic%2FFsmNode%2FFsmInitializePackage.cs) 热更新初始化服务器地址与解密配置自定义修改处
- [MainRoot.cs](Assets%2FAOT%2FScripts%2FMainRoot%2FMainRoot.cs) gamePlay入口逻辑
- [Test.cs](Assets%2FHotUpdateScripts%2FTestTools%2FTest.cs) 测试代码
- [DataTables/Datas](DataTables/Datas) 配置表存放位置
- [DataTables/gen.bat](DataTables/gen.bat) 每次修改配置表后运行此脚本生成配置表代码
## 工具
- 菜单栏 NFramwarkTools/MoveDlls 在HybirdCLR执行Generate/All后,可将所需热更代码Dll和补元Dll移动到指定位置,并自动配置补元Dll到项目中
- 菜单栏 NFramwarkTools/GenerateAssetsName 在YooAsset2设定完资源收集器后,可生成资源常量名静态类[AssetsName.cs](Assets%2FHotUpdateScripts%2FGen%2FAssetsName.cs)(当然只用于热更新程序集中)