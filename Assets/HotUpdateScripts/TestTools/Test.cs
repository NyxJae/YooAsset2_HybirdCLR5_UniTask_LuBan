using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log("这是由YooAsset管理加载的gameObject");
        Debug.Log("这条日志是打包后的热更代码");
    }

    // Update is called once per frame
    private void Update()
    {
    }
}