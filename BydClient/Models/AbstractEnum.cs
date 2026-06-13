using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace BydClient.Models;

/// <summary>
/// Abstract base class for enums.
/// </summary>
public abstract class AbstractEnum
{
    protected static Dictionary<Type, Dictionary<string, int>> _cache = new Dictionary<Type, Dictionary<string, int>>();

    /**
     * Get all constants in the enum.
     *
     * @return array<string, int>
     */
    public static Dictionary<string, int> GetAll<T>() where T : AbstractEnum
    {
        Type type = typeof(T);
        if(!_cache.ContainsKey(type))
        {
            // In C#, constants in a class are retrieved via Reflection
            var constants = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(int))
                .ToDictionary(f => f.Name, f => (int)f.GetValue(null));

            _cache[type] = constants;
        }

        return _cache[type];
    }

    /**
     * Check if a value exists in the enum.
     */
    public static bool IsValid<T>(int value) where T : AbstractEnum
    {
        return GetAll<T>().ContainsValue(value);
    }

    /**
     * Get the name of a value.
     */
    public static string? GetName<T>(int value) where T : AbstractEnum
    {
        var constants = GetAll<T>();

        // Reversing the dictionary logic (equivalent to array_flip)
        return constants.FirstOrDefault(x => x.Value == value).Key;
    }

    /**
     * Get the value for a name.
     */
    public static int? GetValue<T>(string name) where T : AbstractEnum
    {
        var constants = GetAll<T>();

        if(constants.TryGetValue(name, out int value))
        {
            return value;
        }

        return null;
    }
}
