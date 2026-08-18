using System;
using System.Runtime.InteropServices;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.ReplayManagers;

internal class FileSelector
{
  [DllImport("Comdlg32.dll", CharSet = CharSet.Auto)]
  internal static extern bool GetOpenFileName([In, Out] FileSelector.OpenFileName ofn);

  [DllImport("Comdlg32.dll", CharSet = CharSet.Auto)]
  internal static extern bool GetSaveFileName([In, Out] FileSelector.OpenFileName ofn);

  public static string GetFilePathFromUser(string FileTypeLimiter = "*", string startLocation = "C:\\")
  {
    FileSelector.OpenFileName openFileName = new FileSelector.OpenFileName();
    openFileName.structSize = Marshal.SizeOf<FileSelector.OpenFileName>(openFileName);
    openFileName.filter = FileTypeLimiter;
    openFileName.file = new string(new char[256 /*0x0100*/]);
    openFileName.maxFile = openFileName.file.Length;
    openFileName.fileTitle = new string(new char[64 /*0x40*/]);
    openFileName.maxFileTitle = openFileName.fileTitle.Length;
    openFileName.initialDir = "C:\\";
    openFileName.title = "Pick a file";
    return FileSelector.GetOpenFileName(openFileName) ? openFileName.file : "";
  }

  public static string GetFilePathToSaveTo(string defaultFileName)
  {
    FileSelector.OpenFileName openFileName = new FileSelector.OpenFileName();
    openFileName.structSize = Marshal.SizeOf<FileSelector.OpenFileName>(openFileName);
    openFileName.filter = "All Files\0*.*\0";
    openFileName.file = defaultFileName + new string(new char[(int) byte.MaxValue]);
    openFileName.maxFile = openFileName.file.Length;
    openFileName.fileTitle = new string(new char[64 /*0x40*/]);
    openFileName.maxFileTitle = openFileName.fileTitle.Length;
    openFileName.filterIndex = 1;
    openFileName.initialDir = "C:\\";
    openFileName.title = "Pick a location to save your file";
    return !FileSelector.GetSaveFileName(openFileName) ? "" : openFileName.file;
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
  public class OpenFileName
  {
    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;
    public string filter = (string) null;
    public string customFilter = (string) null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public string file = (string) null;
    public int maxFile = 0;
    public string fileTitle = (string) null;
    public int maxFileTitle = 0;
    public string initialDir = (string) null;
    public string title = (string) null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public string defExt = (string) null;
    public IntPtr custData = IntPtr.Zero;
    public IntPtr hook = IntPtr.Zero;
    public string templateName = (string) null;
    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
  }
}
