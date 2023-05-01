using Common;

namespace MyPlugin;

public class MyPlugin : IPlugin
{
    public void Initialize()
    {
        Console.WriteLine("Initializing MyPlugin");
    }

    public void PrintNameAndVersion()
    {
        Console.WriteLine("MyPlugin v1.1");
    }
}

