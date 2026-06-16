using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace BydClient.Models;

/// <summary>
/// Base model for BYD API responses.
/// </summary>
public class BaseModel
{
    protected IDictionary<string, object?> raw;

    public BaseModel(IDictionary<string, object?>? raw = null)
    {
        this.raw = raw ?? new Dictionary<string, object?>();
        Populate(this.raw);
    }

    /// <summary>
    /// This method should be overridden in child classes
    /// to map API data to model properties
    /// </summary>
    /// <param name="data"></param>
    protected virtual void Populate(IDictionary<string, object?> data)
    {
        // To be overridden by subclasses
    }

    public Dictionary<string, object?> ToDictionary()
    {
        var result = new Dictionary<string, object?>();
        var properties = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach(var prop in properties)
        {
            if(prop.Name == nameof(raw))
                continue;

            var value = prop.GetValue(this);
            if(value != null)
            {
                result[SnakeToCamel(prop.Name)] = value;
            }
            else
            {
                result[SnakeToCamel(prop.Name)] = null;
            }
        }

        return result;
    }

    /// <summary>
    /// Convert snake_case to camelCase.
    /// </summary>
    protected string SnakeToCamel(string str)
    {
        var parts = str.Split('_', StringSplitOptions.RemoveEmptyEntries);
        for(int i = 1; i < parts.Length; i++)
        {
            if(parts[i].Length > 0)
            {
                parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
            }
        }
        var camel = string.Join("", parts);
        if(camel.Length > 0)
            camel = char.ToLower(camel[0]) + camel.Substring(1);
        return camel;
    }

    /// <summary>
    /// Convert camelCase to snake_case.
    /// </summary>
    protected string CamelToSnake(string str)
    {
        return Regex.Replace(str, "(?<!^)([A-Z])", "_$1").ToLower();
    }

    public IDictionary<string, object?> GetRaw()
    {
        return raw;
    }
}
