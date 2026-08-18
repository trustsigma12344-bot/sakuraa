using System.IO;
using System.Text;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.Recording;

public class WavWriter
{
  private FileStream _fs;
  private BinaryWriter _bw;
  private int _dataBytes;

  public WavWriter(string path, int sampleRate)
  {
    if (string.IsNullOrEmpty(path))
      return;
    string directoryName = Path.GetDirectoryName(path);
    if (!string.IsNullOrEmpty(directoryName))
      Directory.CreateDirectory(directoryName);
    this._fs = new FileStream(path, FileMode.Create, FileAccess.Write);
    this._bw = new BinaryWriter((Stream) this._fs);
    this.WriteHeaderPlaceholder(sampleRate);
  }

  private void WriteHeaderPlaceholder(int sampleRate)
  {
    int num = sampleRate * 2;
    this._bw.Write(Encoding.ASCII.GetBytes("RIFF"));
    this._bw.Write(0);
    this._bw.Write(Encoding.ASCII.GetBytes("WAVE"));
    this._bw.Write(Encoding.ASCII.GetBytes("fmt "));
    this._bw.Write(16 /*0x10*/);
    this._bw.Write((short) 1);
    this._bw.Write((short) 1);
    this._bw.Write(sampleRate);
    this._bw.Write(num);
    this._bw.Write((short) 2);
    this._bw.Write((short) 16 /*0x10*/);
    this._bw.Write(Encoding.ASCII.GetBytes("data"));
    this._bw.Write(0);
  }

  public void WriteSample(float sample)
  {
    if (this._bw == null)
      return;
    this._bw.Write((short) Mathf.RoundToInt(((double) sample < -1.0 ? -1f : ((double) sample > 1.0 ? 1f : sample)) * (float) short.MaxValue));
    this._dataBytes += 2;
  }

  public void Dispose()
  {
    if (this._bw == null)
      return;
    this._bw.Flush();
    this._fs.Seek(4L, SeekOrigin.Begin);
    this._bw.Write(36 + this._dataBytes);
    this._fs.Seek(40L, SeekOrigin.Begin);
    this._bw.Write(this._dataBytes);
    this._bw.Flush();
    this._bw.Dispose();
    this._fs.Dispose();
    this._bw = (BinaryWriter) null;
    this._fs = (FileStream) null;
  }
}
