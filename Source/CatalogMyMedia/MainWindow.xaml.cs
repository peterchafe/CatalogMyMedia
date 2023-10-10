using CMMLib;
using DirectShowLib;
using ImageMagick;
using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace CatalogMyMedia
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LibVLC m_LibVLC;
        private MediaPlayer m_MediaPlayer;
        private Settings m_Settings;

        public uint Index { get; set; }
        public bool SnapshotSaved { get; set; }

        List<DsDevice> m_Devices;

        public string ScreenshotFile => $"D:\\Catalog\\CMM_{Index.ToString().PadLeft(4, '0')}.png";

        public MainWindow()
        {
            m_Settings = Settings.Load();

            if (string.IsNullOrWhiteSpace(m_Settings.WorkingFolder) || Directory.Exists(m_Settings.WorkingFolder) == false)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();

                fbd.Description = "Select folder where snapshots will be located.";

                fbd.ShowDialog();

                if (string.IsNullOrEmpty(fbd.SelectedPath) || Directory.Exists(fbd.SelectedPath) == false)
                {
                    System.Windows.MessageBox.Show("Invalid directory selected! Application will now close!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }

                m_Settings = m_Settings.WithWorkingFolder(fbd.SelectedPath);
                m_Settings.Save();
            }

            SnapshotSaved = false;

            InitializeComponent();

            m_Devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice).ToList();

            ListCameras.ItemsSource = m_Devices.Select(x => x.Name).ToList();

            Core.Initialize();

            // Command Line Options: https://wiki.videolan.org/VLC_command-line_help/
            m_LibVLC = new LibVLC(new string[] { "--dshow-fps=30", "--dshow-aspect-ratio=16:9" });

            m_MediaPlayer = new MediaPlayer(m_LibVLC);

            videoView.MediaPlayer = m_MediaPlayer;

            ListCameras.SelectionChanged += CameraChanged;

            for (int i = 0; i < ListCameras.Items.Count; i++)
            {
                if (ListCameras.Items[i].Equals(m_Settings.CameraName))
                {
                    ListCameras.SelectedIndex = i;
                }
            }
        }

        private void CameraChanged(object sender, SelectionChangedEventArgs e)
        {
            // string mediaOptions = $"dshow-vdev={m_Devices.Where(x => x.Name == ListCameras.Text)}";
            string? name = (string?)(ListCameras.SelectedItem is not null ? ListCameras.SelectedValue : m_Devices.First().Name);

            if (name is not null)
            {
                string mediaOptions = $"dshow-vdev={name}";
                m_MediaPlayer.Play(new Media(m_LibVLC, new Uri("dshow://"), mediaOptions));

                if (m_Settings.CameraName != name)
                {
                    m_Settings = m_Settings.WithCameraName(name);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (SnapshotSaved && string.IsNullOrWhiteSpace(txtName.Text) == false)
            {
                using (var writer = new StreamWriter(Path.Combine(m_Settings.WorkingFolder, "catalog.csv"), true))
                {
                    writer.WriteLine($"{Index},{txtName.Text},{ScreenshotFile}");
                }

                using (var imageMagick = new MagickImage(ScreenshotFile))
                {
                    imageMagick.Resize(640, 480);
                    imageMagick.Write(ScreenshotFile);
                }

                SnapshotSaved = false;
                txtName.Text = string.Empty;
                Index++;
                m_Settings.AddRecord();
            }
            else
            {
                System.Windows.MessageBox.Show("Snapshot not taken, please take a snapshot before saving.");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (m_MediaPlayer is not null)
            {
                // Release LibVLC resources when the application is closed
                m_MediaPlayer.Dispose();
                m_LibVLC.Dispose();
            };           
        }

        private void CreateSnapshot_Click(object sender, RoutedEventArgs e)
        {
            // Ensure the MediaPlayer is playing
            if (m_MediaPlayer.IsPlaying)
            {
                // Capture the current frame from the video output
                if (m_MediaPlayer.TakeSnapshot(0, ScreenshotFile, 960, 720))
                {
                    SnapshotSaved = true;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("No video is playing.");
            }
        }

        // Crop picture:
        //top left	150,0
        //bottom left 150,345

        //top right    416,0
        //bottom right 416,345
    }
}
