using System;
using System.Collections.Generic;

namespace WayfarerKit.Patterns.EventBus.Helpers
{
    public static class PredefinedAssemblyHelper
    {
        private static readonly Dictionary<string, AssemblyType> __stringToAssemblyType = new()
        {
            {
                "Assembly-CSharp", AssemblyType.AssemblyCSharp
            },
            {
                "Assembly-CSharp-firstpass", AssemblyType.AssemblyCSharpFirstPass
            },
            {
                "Assembly-CSharp-Editor", AssemblyType.AssemblyCSharpEditor
            },
            {
                "Assembly-CSharp-Editor-firstpass", AssemblyType.AssemblyCSharpEditorFirstPass
            },
            {
                "WayfarerKit.Runtime", AssemblyType.WayfarerKit
            },
            {
                "WayfarerKit.Editor", AssemblyType.WayfarerKitEditor
            }
        };

        private static void AddTypesFromAssemblies(Dictionary<AssemblyType, Type[]> assemblyTypes, List<Type> types, Type interfaceType)
        {
            foreach (var assemblyType in assemblyTypes.Keys)
            {
                var assembly = assemblyTypes[assemblyType];

                if (assembly == null) continue;

                foreach (var type in assembly)

                    // checks whether the type is assignable from the interfaceType
                    if (type != interfaceType && interfaceType.IsAssignableFrom(type))
                        types.Add(type);
            }
        }

        public static List<Type> GetTypes(Type interfaceType)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assemblyTypes = new Dictionary<AssemblyType, Type[]>();
            var types = new List<Type>();

            foreach (var assembly in assemblies)
                if (__stringToAssemblyType.TryGetValue(assembly.GetName().Name, out var assemblyType))
                    assemblyTypes.Add(assemblyType, assembly.GetTypes());

            AddTypesFromAssemblies(assemblyTypes, types, interfaceType);

            return types;
        }
        private enum AssemblyType
        {
            AssemblyCSharp,
            AssemblyCSharpFirstPass,
            AssemblyCSharpEditor,
            AssemblyCSharpEditorFirstPass,
            WayfarerKit,
            WayfarerKitEditor
        }
    }
}