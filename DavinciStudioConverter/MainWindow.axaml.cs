using System;
using System.IO;
using Avalonia.Input;
using Avalonia.Controls;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.Data;

namespace Teste
{
    public partial class MainWindow : Window
    {
        private const string VideoExtension = "mkv,mp4,amv,webm,flv,vob,ogv,ogg,drc,gif,gifv,mng,avi" +
                                              ",MTS,M2TS,TS,mov,qt,wmv,yuv,rm,rmvb,viv,asf,m4p,m4v,mpg,mp2" +
                                              ",mpeg,mpe,mpv,m2v,svi,3gp,3g2,mxf,roq,nsv,flv,f4v,f4p,f4a,f4b";
        readonly string _applicationPath;
        readonly string _ffmpegPath;

        public MainWindow()
        {
            InitializeComponent();
            _applicationPath = Directory.GetCurrentDirectory();
            if (System.Diagnostics.Debugger.IsAttached)
                _applicationPath += "/bin/Debug/net6.0";
            _ffmpegPath = _applicationPath + "/ffmpeg";
        }

        private async Task<string?> SearchFile(List<FileDialogFilter>? filter = null)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (filter != null)
                fileDialog.Filters.AddRange(collection: filter);
            
            string[]? file = await fileDialog.ShowAsync(parent: this);
            return file?[0];
        }

        private async Task<string?> SearchPath()
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog();
            string? path = await folderDialog.ShowAsync(parent: this);
            return path;
        }

        public async void btnSourceFile_Click(object sender, RoutedEventArgs e)
        {
            List<FileDialogFilter> filter = new List<FileDialogFilter>();
            foreach (string item in VideoExtension.Split(separator: ","))
                filter.Add(item: new FileDialogFilter() { Name = item, Extensions = { item } });
            TxtSourceFile.Text = await SearchFile(filter);
        }

        public async void btnOutputPath_Click(object sender, RoutedEventArgs e)
        {
            TxtOutputPath.Text = await SearchPath();
        }

        public void txtAudioTracks_KeyUp(object sender, KeyEventArgs e)
        {
            string aux = "";
            foreach (char item in TxtAudioTracks.Text)
            {
                try
                {
                    if (Int32.Parse(s: item.ToString()) >= 0)
                        aux += item;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            TxtAudioTracks.Text = aux;
        }

        private bool ExecuteFfmpeg(string parameters)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo = new System.Diagnostics.ProcessStartInfo(
                    fileName: _ffmpegPath, arguments: parameters)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                process.Start();
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private bool Converter()
        {
            string prefixCommand = " -i " + TxtSourceFile.Text;
            try
            {
                if (DefineBool(boolean: CkbAudio.IsChecked))
                {
                    string aux = "";
                    for (int i = 0; i < Int32.Parse(s: TxtAudioTracks.Text); i++)
                    {
                        aux += " -map 0:a:" + i + " "
                            + TxtOutputPath.Text + "/audio" + i + ".wav";
                    }
                    if (!ExecuteFfmpeg(parameters: prefixCommand + aux))
                    {
                        throw new Exception(message: "There was an error converting the audio.");
                    }
                }
                if (DefineBool(boolean: CkbVideo.IsChecked))
                {
                    if (!ExecuteFfmpeg(parameters: prefixCommand + " -c:v mpeg4 -b:v 10M -map 0:v " +
                        TxtOutputPath.Text + "/video.m4v"))
                    {
                        throw new Exception(message: "There was an error converting the video.");
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void btnConverter_Click(object sender, RoutedEventArgs e)
        {
            BtnConverter.IsEnabled = false;
            new MessageBox.MessageBox().Show(Converter() ? "Conclude!" : "Errors occurred.",
                "DaVinci Resolve video converter", MessageBox.MessageBox.MessageBoxButtons.Ok);
            BtnConverter.IsEnabled = true;
        }

        private static bool DefineBool(bool? boolean)
        {
            return boolean ?? false;
        }


    }
}
