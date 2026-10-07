using System.Reflection;
using System.Runtime.Loader;

// Load LLamaSharp 0.27.0 from the NuGet cache and dump the vision-relevant API:
// InteractiveExecutor members, ClipModel/Mtmd members, ChatSession image overloads.
var dll = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".nuget", "packages", "llamasharp", "0.27.0", "lib", "net6.0", "LLamaSharp.dll");
Console.WriteLine("DLL: " + dll + " exists=" + File.Exists(dll));
var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);

void Dump(Type t)
{
    Console.WriteLine($"\n===== {t.FullName} =====");
    foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        Console.WriteLine($"  prop {p.PropertyType.Name} {p.Name}");
    foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
    {
        if (m.IsSpecialName) continue;
        var ps = string.Join(", ", m.GetParameters().Select(x => x.ParameterType.Name + " " + x.Name));
        Console.WriteLine($"  method {m.ReturnType.Name} {m.Name}({ps})");
    }
    foreach (var c in t.GetConstructors())
        Console.WriteLine($"  ctor({string.Join(", ", c.GetParameters().Select(x => x.ParameterType.Name + " " + x.Name))})");
}

foreach (var name in new[] {
    "LLama.InteractiveExecutor",
    "LLama.StatefulExecutorBase",
    "LLama.ChatSession",
    "LLama.Common.ChatHistory",
    "LLama.Native.ClipModel",
    "LLama.Native.MtmdContext",
    "LLama.Native.NativeApi",
})
{
    var t = asm.GetType(name);
    if (t is null) { Console.WriteLine($"\n!!!!! {name} NOT FOUND"); continue; }
    Dump(t);
    foreach (var nested in t.GetNestedTypes(BindingFlags.Public))
        Dump(nested);
}
