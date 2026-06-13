using System;
using System.Collections.Generic;
using System.Text;

namespace BydClient.Models;

/// <summary>
/// A permission scope granted to a shared user.
/// </summary>
public sealed class EmpowerRange : BaseModel
{
    // Properties (public getters)
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    private readonly List<EmpowerRange> _children = new();
    /// <summary>
    /// Child permission ranges.
    /// </summary>
    public IReadOnlyList<EmpowerRange> Children => _children.AsReadOnly();

    public EmpowerRange() { }

    public EmpowerRange(IDictionary<string, object?> data) : base(data)
    {
        if(data == null) throw new ArgumentNullException(nameof(data));
        Populate(data);
    }

    protected override void Populate(IDictionary<string, object?> data)
    {
        Code = data["code"]?.ToString() ?? string.Empty;
        Name = data["name"]?.ToString() ?? string.Empty;

        // Handle children - look for both 'children' and 'childList'
        object? childrenObj = data.ContainsKey("children")
            ? data["children"]
            : data.ContainsKey("childList")
                ? data["childList"]
                : null;

        if(childrenObj is IEnumerable<object> childrenEnumerable)
        {
            foreach(var child in childrenEnumerable)
            {
                if(child is IDictionary<string, object?> childDict)
                {
                    _children.Add(new EmpowerRange(childDict));
                }
                else if(child is IDictionary<string, object> childDictNonNull)
                {
                    // handle non-nullable dictionary variant
                    var tmp = new Dictionary<string, object?>();
                    foreach(var kv in childDictNonNull) tmp[kv.Key] = kv.Value;
                    _children.Add(new EmpowerRange(tmp));
                }
                else
                {
                    // If child is JSON string or JsonElement, caller should parse into dictionary before calling Populate.
                }
            }
        }
        else if(childrenObj is IEnumerable<KeyValuePair<string, object?>> kvpEnumerable)
        {
            // defensive: if children come as dictionary-like enumerable
            foreach(var kvp in kvpEnumerable)
            {
                if(kvp.Value is IDictionary<string, object?> childDict)
                    _children.Add(new EmpowerRange(childDict));
            }
        }
    }

    // Factory helper
    public static EmpowerRange FromDictionary(IDictionary<string, object?> data) => new EmpowerRange(data);

    // Optional: Java-like getters if you prefer
    public EmpowerRange[] GetChildren() => _children.ToArray();
}
