using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using Mono.Cecil;
using FieldAttributes = Mono.Cecil.FieldAttributes;

namespace MooAPI_Patcher
{
    public static class MapsPatch
    {
        internal static readonly List<string> Maps = [];
        public static void Patch(AssemblyDefinition assembly)
        {
            var maps_enum = assembly.MainModule.GetType("MainManager/Maps");
            var battlemaps_enum = assembly.MainModule.GetType("MainManager/BattleMaps");
            Patcher.Log.LogInfo($"MAPS: {maps_enum.Fields.Count}");
            Patcher.Log.LogInfo($"BATTLEMAPS: {battlemaps_enum.Fields.Count}");
            var files = Directory.GetFiles(Paths.PluginPath, "*.dll", SearchOption.AllDirectories);
            foreach (var item in files) // loading plugins to extract maps to load
            {
                try // loads maps.txt from plugin's res (don't really like this way but it works)
                {
                    var pluginassembly = Assembly.ReflectionOnlyLoadFrom(item);
                    var mapslist = pluginassembly.GetManifestResourceNames().Where(i => i.EndsWith("maps.txt"));
                    foreach (var _ in mapslist)
                    {
                        using var reader = new StreamReader(pluginassembly.GetManifestResourceStream(_));
                        Maps.AddRange(reader.ReadToEnd().Split('|').Select(trim => trim.Trim()).Where(i1 => i1.Length >= 1 && !maps_enum.Fields.Any(i2 => i2.Name == i1)));
                        // maybe i made this too overloaded/complex
                    }
                }
                catch (System.Exception) { }
            }
            foreach (var map in Maps) // adding maps
            {
                maps_enum.Fields.Add(new FieldDefinition(map, FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.Public | FieldAttributes.HasDefault, maps_enum) { Constant = maps_enum.Fields.Count - 1 });
                Patcher.Log.LogInfo($"Adding {map}");
            }
            //assembly.Write($"{Paths.PatcherPluginPath}/PATCHED.dll");
        }
    }
}
