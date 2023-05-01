using System.Reflection;
using Common;

namespace Application;

class Program
{
    static readonly string PluginDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../MyPlugin/bin/Debug/net7.0");

    public static void Main()
    {
        var assembly = Assembly.LoadFile(Path.Combine(PluginDirectory, "MyPlugin.dll"));
        var type = assembly.GetTypes().First(t => t.GetInterface("IPlugin") == typeof(IPlugin));

        var plugin = (IPlugin?)type.GetConstructor(Type.EmptyTypes)?.Invoke(null);
        if (plugin == null)
        {
            Console.Error.WriteLine("Missing IPlugin in assembly");
            return;
        }

        plugin.Initialize();
        plugin.PrintNameAndVersion();
    }
}
