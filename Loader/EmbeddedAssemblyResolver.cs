using System;
using System.IO;
using System.Reflection;

#nullable disable
namespace Loader;

internal static class EmbeddedAssemblyResolver
{
  private static bool _hooked;

  public static void Hook()
  {
    if (EmbeddedAssemblyResolver._hooked)
      return;
    EmbeddedAssemblyResolver._hooked = true;
    AppDomain.CurrentDomain.AssemblyResolve += EmbeddedAssemblyResolver.OnAssemblyResolve;
  }

  private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
  {
    string name = new AssemblyName(args.Name).Name;
    string resourceName = "SakuraaCastingMod.Embedded." + name + ".dll";
    Assembly executingAssembly = Assembly.GetExecutingAssembly();
    using Stream resourceStream = executingAssembly.GetManifestResourceStream(resourceName);
    if (resourceStream == null)
      return null;
    byte[] buffer = new byte[resourceStream.Length];
    int offset = 0;
    while (offset < buffer.Length)
    {
      int read = resourceStream.Read(buffer, offset, buffer.Length - offset);
      if (read == 0)
        break;
      offset += read;
    }
    return Assembly.Load(buffer);
  }
}
