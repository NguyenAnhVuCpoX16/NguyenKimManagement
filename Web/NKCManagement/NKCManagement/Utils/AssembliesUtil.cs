using System.Reflection;

namespace NKCManagement.Utils
{
    public static class AssembliesUtil
    {
        private static List<Assembly>? allAssemblies = null;
        public static IEnumerable<Assembly> GetAssemblies()
        {
            if (allAssemblies == null)
            {
                var modules = new List<Assembly>();
                string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var files = Directory.GetFiles(path ?? "", "*.dll");

                foreach (string dll in files.Where(x => Path.GetFileName(x).StartsWith("NKCManagement")))
                {
                    var name = dll.GetAfterLast("\\").GetBeforeLast(".");
                    if (modules.Any(t => t.GetName().Name == name))
                        continue;

                    modules.Add(Assembly.LoadFrom(dll));
                }

                allAssemblies = modules;
            }

            return allAssemblies;
        }

        public static IEnumerable<Type> GetTypes<T>(this IEnumerable<Assembly> assemblies)
        {
            return assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    typeof(T).IsAssignableFrom(t)
                    && !t.IsAbstract
                    && !t.IsInterface);
        }

        public static IEnumerable<T> GetInstances<T>(this IEnumerable<Assembly> assemblies)
        {
            var instances = new List<T>();
            foreach (var implementation in assemblies.GetTypes<T>())
            {
                var instance =
                    (T)Activator.CreateInstance(implementation)!;
                Console.WriteLine($"Instance created: {instance.GetType().FullName}");
                instances.Add(instance);
            }

            return instances;
        }

        public static IEnumerable<Assembly> GetAspNetAssemblies()
        {
            return GetAssemblies();
        }

    }
}
