using DirectShowLib;
using ImageMagick;
using LibVLCSharp.Shared;
using System;
using System.IO;
using System.Windows;

namespace CatalogMyMedia
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string TrackFile = "D:\\Catalog\\track.log";
        private LibVLC m_LibVLC;
        private MediaPlayer m_MediaPlayer;
        

        public uint Index { get; set; }
        public bool SnapshotSaved { get; set; }

        public string ScreenshotFile => $"D:\\Catalog\\CMM_{Index.ToString().PadLeft(4, '0')}.png";

        public MainWindow()
        {
            if (File.Exists(TrackFile) && uint.TryParse(File.ReadAllText(TrackFile), out uint index))
            {
                Index = index;
            }

            SnapshotSaved = false;

            InitializeComponent();

            DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            string deviceName = string.Empty;

            foreach (var device in devices)
            {
                deviceName = device.Name;

                if (deviceName.Contains("Logi", StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }
            }

            Core.Initialize();

            m_LibVLC = new LibVLC();
            m_MediaPlayer = new MediaPlayer(m_LibVLC);

            videoView.MediaPlayer = m_MediaPlayer;

            string mediaOptions = $"dshow-vdev={deviceName}";

            m_MediaPlayer.Play(new Media(m_LibVLC, new Uri("dshow://"), mediaOptions));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (SnapshotSaved && string.IsNullOrWhiteSpace(txtName.Text) == false)
            {
                using (var writer = new StreamWriter($"D:\\Catalog\\catalog.csv", true))
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
                File.WriteAllText("D:\\Catalog\\track.log", Index.ToString());
            }
            else
            {
                MessageBox.Show("Snapshot not taken, please take a snapshot before saving.");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Release LibVLC resources when the application is closed
            m_MediaPlayer.Dispose();
            m_LibVLC.Dispose();
        }

        private void CreateSnapshot_Click(object sender, RoutedEventArgs e)
        {
            // Ensure the MediaPlayer is playing
            if (m_MediaPlayer.IsPlaying)
            {
                // Capture the current frame from the video output
                if (m_MediaPlayer.TakeSnapshot(0, ScreenshotFile, 640, 480))
                {
                    SnapshotSaved = true;
                }
            }
            else
            {
                MessageBox.Show("No video is playing.");
            }
        }

        // Crop picture:
        //top left	150,0
        //bottom left 150,345

        //top right    416,0
        //bottom right 416,345
    }
}
