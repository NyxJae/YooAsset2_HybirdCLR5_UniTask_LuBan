using System; // 引用系统命名空间
using System.Collections; // 引用系统集合命名空间
using System.Collections.Generic; // 引用系统集合泛型命名空间
using UnityEngine; // 引用Unity引擎命名空间

// 定义ConsoleToScreen类，继承自MonoBehaviour，可以将其挂载到游戏对象上。
public class ConsoleToScreen : MonoBehaviour
{
    // 定义屏幕上可显示的最大行数为50行。
    const int maxLines = 50;

    // 定义每行日志的最大长度为120字符。
    const int maxLineLength = 120;

    // 可在Unity编辑器中设置的公共字体大小，默认值为15。
    public int fontSize = 60;

    // 列表用于储存分割后的日志行。
    private readonly List<string> _lines = new List<string>();

    // 用于储存整合后要显示在屏幕上的所有日志信息。
    private string _logStr = "";

    // 当脚本被激活时调用，添加日志消息的监听。
    void OnEnable()
    {
        Application.logMessageReceived += Log;
    }

    // 当脚本被禁用或不激活时调用，移除日志消息的监听。
    void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }

    // Unity中的GUI绘制方法，会在每个渲染帧中调用。
    void OnGUI()
    {
        // 设置GUI缩放，以适应不同分辨率的屏幕。
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
            new Vector3(Screen.width / 1200.0f, Screen.height / 800.0f, 1.0f));
        // 绘制一个文本标签，显示_logStr中的日志信息，字体大小根据设置调整，但不小于10。
        GUI.Label(new Rect(10, 10, 800, 370), _logStr, new GUIStyle() { fontSize = Math.Max(10, fontSize) });
    }

    // 用于处理日志消息的方法，当有日志消息时会被调用。
    public void Log(string logString, string stackTrace, LogType type)
    {
        // 遍历分割后的每一行日志。
        foreach (var line in logString.Split('\n'))
        {
            // 如果行长度小于等于最大长度，直接添加到列表。
            if (line.Length <= maxLineLength)
            {
                _lines.Add(line);
                continue;
            }

            // 如果行长度大于最大长度，需要进一步分割。
            var lineCount = line.Length / maxLineLength + 1;
            for (int i = 0; i < lineCount; i++)
            {
                // 对长行进行分割并逐段添加到列表中。
                if ((i + 1) * maxLineLength <= line.Length)
                {
                    _lines.Add(line.Substring(i * maxLineLength, maxLineLength));
                }
                else
                {
                    _lines.Add(line.Substring(i * maxLineLength, line.Length - i * maxLineLength));
                }
            }
        }

        // 如果列表中的行数超过了最大限制，移除旧的行直至列表行数符合限制。
        if (_lines.Count > maxLines)
        {
            _lines.RemoveRange(0, _lines.Count - maxLines);
        }

        // 将列表中的行合并成一个字符串，用换行符\n连接，准备显示。
        _logStr = string.Join("\n", _lines);
    }
}