using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;
using Avalonia.Input.TextInput;
using Path = System.IO.Path;

namespace DavinciStudioConverter;

public class Ffmpeg
{
    private readonly string _pathFfmpeg;
    private readonly string _pathFfprobe;

    public Ffmpeg()
    {
        if (!File.Exists(@"/usr/bin/ffmpeg") && !File.Exists(@"/usr/bin/ffprobe"))
            throw new Exception("cannot found ffmpeg!");
        _pathFfmpeg = @"/usr/bin/ffmpeg";
        _pathFfprobe = @"/usr/bin/ffprobe";
    }

    public async Task ExtractAudio(string input, int tracks, string outputPath)
    {
        var arguments = "";
        for (var i = 0; i < tracks; i++)
        {
            var bw = new BackgroundWorker();
            arguments = string.Format("-i \"{0}\" -map 0:a:{2} \"{3}/audio{2}.wav\" ", input, i, outputPath);
            bw.DoWork += (x, y) => { Execute(_pathFfmpeg, arguments); };
            bw.RunWorkerAsync();
        }
    }

    public async Task ConvertVideo(string input, string outputPath, int bitRate = 0)
    {
        if (bitRate == 0)
            bitRate = 10;
        var arguments = string.Format("-i \"{0}\" -c:v mpeg4 -b:v {1}M -map 0:v \"{2}/video.m4v\" ", input, bitRate,
            outputPath);
        var bw = new BackgroundWorker();
        bw.DoWork += (x, y) => { Execute(_pathFfmpeg, arguments); };
        bw.RunWorkerAsync();
    }

    private string Execute(string file, string arguments)
    {
        var process = new System.Diagnostics.Process();
        process.StartInfo = new System.Diagnostics.ProcessStartInfo(
            fileName: file, arguments: arguments)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true
        };
        process.Start();
        process.WaitForExit();
        return process.StandardOutput.ReadToEnd();
    }
}