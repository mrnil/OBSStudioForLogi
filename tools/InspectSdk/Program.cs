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

// Look for image-related methods in the hierarchy
Console.WriteLine("=== Image-related methods in ActionEditorCommand hierarchy ===");
var current = actionEditorCmd;
while (current != null)
{
    var methods = current.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .Where(m => m.Name.Contains("Image") || m.Name.Contains("Bitmap") || m.Name.Contains("Draw") || m.Name.Contains("Render") || m.Name.Contains("Display") || m.Name.Contains("Widget"));
    
    if (methods.Any())
    {
        Console.WriteLine($"\n--- {current.Name} ---");
        foreach (var method in methods)
        {
            var parms = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
            Console.WriteLine($"  {(method.IsVirtual ? "virtual " : "")}{(method.IsAbstract ? "abstract " : "")}{method.ReturnType.Name} {method.Name}({parms})");
        }
    }
    
    if (current.BaseType?.FullName == "System.Object") break;
    current = current.BaseType;
}

// Also check for IsWidget property
Console.WriteLine("\n=== IsWidget property ===");
current = actionEditorCmd;
while (current != null)
{
    var isWidget = current.GetProperty("IsWidget", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
    if (isWidget != null)
    {
        Console.WriteLine($"  Found in: {current.Name}, CanWrite: {isWidget.CanWrite}");
    }
    if (current.BaseType?.FullName == "System.Object") break;
    current = current.BaseType;
}

// Check BitmapBuilder constructors
Console.WriteLine("\n=== BitmapBuilder constructors ===");
var bb = asm.GetTypes().FirstOrDefault(t => t.Name == "BitmapBuilder");
if (bb != null)
{
    foreach (var ctor in bb.GetConstructors())
    {
        var parms = string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
        Console.WriteLine($"  BitmapBuilder({parms})");
    }
}

// Check PluginImageSize values
Console.WriteLine("\n=== PluginImageSize ===");
var pis = asm.GetTypes().FirstOrDefault(t => t.Name == "PluginImageSize");
if (pis != null)
{
    foreach (var field in pis.GetFields(BindingFlags.Public | BindingFlags.Static))
    {
        Console.WriteLine($"  {field.Name}");
    }
}
