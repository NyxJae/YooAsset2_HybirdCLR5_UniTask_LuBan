
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Luban;
using SimpleJSON;


namespace cfg.demo
{
public partial class TbReward
{
    private readonly System.Collections.Generic.Dictionary<int, demo.Reward> _dataMap;
    private readonly System.Collections.Generic.List<demo.Reward> _dataList;
    
    public TbReward(JSONNode _buf)
    {
        _dataMap = new System.Collections.Generic.Dictionary<int, demo.Reward>();
        _dataList = new System.Collections.Generic.List<demo.Reward>();
        
        foreach(JSONNode _ele in _buf.Children)
        {
            demo.Reward _v;
            { if(!_ele.IsObject) { throw new SerializationException(); }  _v = demo.Reward.DeserializeReward(_ele);  }
            _dataList.Add(_v);
            _dataMap.Add(_v.Id, _v);
        }
    }

    public System.Collections.Generic.Dictionary<int, demo.Reward> DataMap => _dataMap;
    public System.Collections.Generic.List<demo.Reward> DataList => _dataList;

    public demo.Reward GetOrDefault(int key) => _dataMap.TryGetValue(key, out var v) ? v : null;
    public demo.Reward Get(int key) => _dataMap[key];
    public demo.Reward this[int key] => _dataMap[key];

    public void ResolveRef(Tables tables)
    {
        foreach(var _v in _dataList)
        {
            _v.ResolveRef(tables);
        }
    }

}

}
