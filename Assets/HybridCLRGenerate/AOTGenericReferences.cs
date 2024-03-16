using System.Collections.Generic;

public class AOTGenericReferences : UnityEngine.MonoBehaviour
{
    // {{ AOT assemblies
    public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
    {
        "mscorlib.dll",
    };
    // }}

    // {{ constraint implement type
    // }} 

    // {{ AOT generic types
    // System.Action<object>
    // System.Collections.Generic.ArraySortHelper<object>
    // System.Collections.Generic.Comparer<object>
    // System.Collections.Generic.ICollection<object>
    // System.Collections.Generic.IComparer<object>
    // System.Collections.Generic.IEnumerable<object>
    // System.Collections.Generic.IEnumerator<object>
    // System.Collections.Generic.IList<object>
    // System.Collections.Generic.List.Enumerator<object>
    // System.Collections.Generic.List<object>
    // System.Collections.Generic.ObjectComparer<object>
    // System.Collections.ObjectModel.ReadOnlyCollection<object>
    // System.Comparison<object>
    // System.Predicate<object>
    // }}

    public void RefMethods()
    {
    }
}