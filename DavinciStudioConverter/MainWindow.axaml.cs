using System;
using System.IO;
using Avalonia.Input;
using Avalonia.Controls;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using System.Collections.Generic;
using Avalonia.Input.Raw;
using static System.Int32;

namespace DavinciStudioConverter
{
    public partial class MainWindow : Window
    {
        private bool _audioProcessing;
        private bool _videoProcessing;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async Task<string?> SearchFile(List<FileDialogFilter>? filter = null)
        {
            var fileDialog = new OpenFileDialog();
            if (filter != null)
                fileDialog.Filters.AddRange(collection: filter);
            var file = await fileDialog.ShowAsync(parent: this);
            return file?[0];
        }

        private async Task<string?> SearchPath()
        {
            var folderDialog = new OpenFolderDialog();
            var path = await folderDialog.ShowAsync(parent: this);
            return path;
        }

        public async void btnSourceFile_Click(object sender, RoutedEventArgs e)
        {
            TxtSourceFile.Text = await SearchFile();
        }

        public async void btnOutputPath_Click(object sender, RoutedEventArgs e)
        {
            TxtOutputPath.Text = await SearchPath();
        }

        public void txtAudioTracks_KeyUp(object sender, KeyEventArgs e)
        {
            var aux = "";
            foreach (var item in TxtAudioTracks.Text)
            {
                if (Parse(s: item.ToString()) >= 0)
                    aux += item;
            }

            TxtAudioTracks.Text = aux;
        }

        private async Task AudioExtract()
        {
            _audioProcessing = true;
            await new Ffmpeg().ExtractAudio(TxtSourceFile.Text, int.Parse(TxtAudioTracks.Text), TxtOutputPath.Text);
            _audioProcessing = false;
        }

        private async Task ConvertVideo()
        {
            _videoProcessing = true;
            await new Ffmpeg().ConvertVideo(TxtSourceFile.Text, TxtOutputPath.Text);
            _videoProcessing = false;
        }

        private void Converter()
        {
            try
            {
                CreatePath(TxtOutputPath.Text);
                if (DefineBool(CkbAudio.IsChecked))
                    AudioExtract();
                if (DefineBool(boolean: CkbVideo.IsChecked))
                    ConvertVideo();
                while (_videoProcessing || _audioProcessing)
                {
                }
                new MessageBox.MessageBox().Show("Process Concluded", "DaVinci Resolve video converter",
                    MessageBox.MessageBox.MessageBoxButtons.Ok);
            }
            catch (Exception e)
            {
                new MessageBox.MessageBox().Show(e.Message,
                    "DaVinci Resolve video converter", MessageBox.MessageBox.MessageBoxButtons.Ok);
            }
        }

        public void btnConverter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnConverter.IsEnabled = false;
                if (!ValidatePathIsSeted())
                {
                    new MessageBox.MessageBox().Show("Source not selected!", "DaVinci Resolve video converter",
                        MessageBox.MessageBox.MessageBoxButtons.Ok);
                    return;
                }

                Converter();
            }
            catch (Exception exception)
            {
            }
        }

        private bool ValidatePathIsSeted()
        {
            return TxtSourceFile.Text != null && TxtOutputPath.Text != null;
        }

        private static bool DefineBool(bool? boolean)
        {
            return boolean ?? false;
        }

        private void CreatePath(string path)
        {
            if (Directory.Exists(path))
                return;
            var verificationPath = "";
            foreach (var item in path.Split("/"))
            {
                verificationPath += item + "/";
                if (!Directory.Exists(verificationPath))
                    Directory.CreateDirectory(verificationPath);
            }
        }
    }
}