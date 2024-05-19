using System.Collections.Generic;
using UnityEngine;

namespace MooAPI.CustomMaps;

public class Core
{
    public static Dictionary<string, Object> Maps = [];
    public static void AddToMaps(string name, Object map)
    {
        Maps.Add(name, map);
    }
    public static Object LoadMap(string map)
    {
        if (Maps.TryGetValue(map.Split('/')[2], out var value))
        {
            return value;
        }
        else
        {
            return Resources.Load(map);
        }
    }
}
