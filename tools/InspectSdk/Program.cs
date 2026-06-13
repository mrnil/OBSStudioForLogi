using System.Reflection;
using System.Runtime.InteropServices;

var runtimeDir = RuntimeEnvironment.GetRuntimeDirectory();
var resolver = new PathAssemblyResolver(Directory.GetFiles(runtimeDir, "*.dll")
    .Append(@"C:\Program Files\Logi\LogiPluginService\PluginApi.dll"));

using var mlc = new MetadataLoadContext(resolver);
var asm = mlc.LoadFromAssemblyPath(@"C:\Program Files\Logi\LogiPluginService\PluginApi.dll");

var actionEditorCmd = asm.GetTypes().FirstOrDefault(t => t.Name == "ActionEditorCommand");
if (actionEditorCmd == null)
{
    Console.WriteLine("ActionEditorCommand not found");
    return;
}

Console.WriteLine($"=== ActionEditorCommand hierarchy ===");
var current = actionEditorCmd;
while (current != null)
{
    Console.WriteLine($"\n--- {current.Name} (in {current.Assembly.GetName().Name}) ---");
    foreach (var prop in current.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    {
        Console.WriteLine($"  Property: {prop.Name} ({prop.PropertyType.Name})");
    }
    if (current.BaseType?.FullName == "System.Object") break;
    current = current.BaseType;
}
