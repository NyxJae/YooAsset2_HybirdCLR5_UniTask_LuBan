using SimpleJSON;
using UnityEngine;
using YooAsset;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log("这是由YooAsset管理加载的gameObject");
        // Debug.Log("以下日志是打包后解除注释进行热更的代码");
        // var tables = new cfg.Tables(LoadByteBuf);
        // cfg.demo.Reward reward = tables.TbReward.Get(1000);
        // Debug.Log($"reward:{reward}");
    }

    // 定义LoadByteBuf方法，参数为文件名，返回JSONNode类型
    private static JSONNode LoadByteBuf(string file)
    {
        // 从AssetBundle中异步加载指定名称的资源
        AssetHandle handle = YooAssets.LoadAssetSync(file);
        var Json = (handle.AssetObject as TextAsset)?.text;
        // 读取指定路径下的json文件内容，编码格式为UTF8，并解析为JSONNode对象
        return JSON.Parse(Json);
    }
}