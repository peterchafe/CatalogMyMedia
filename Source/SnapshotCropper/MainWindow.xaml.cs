using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SnapshotCropper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string Root = @"D:\Catalog\";
        private Point startPoint;
        private Point currentPoint;
        bool isCropping = false;
        private BitmapSource? originalImageSource;
        private BitmapSource? croppedImageSource;
        private string[] m_ImageFiles;

        private int Index = 0;

        public MainWindow()
        {
            InitializeComponent();

            List<string> ImageFiles = new List<string>();

            foreach (var file in Directory.GetFiles(@"D:\Catalog\", "*.png"))
            {
                if (File.Exists($"{Path.Combine(Root, Path.GetFileNameWithoutExtension(file))}.jpg") == false)
                {
                    ImageFiles.Add(file);
                }
            }

            m_ImageFiles = ImageFiles.ToArray();

            myImageControl.Source = new BitmapImage(new Uri(m_ImageFiles[Index]));

            originalImageSource = GetBitmapSourceFromImageControl(myImageControl);

            Canvas.SetTop(myImageControl, 0);
        }

        private void CropRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(this);
            isCropping = true;
        }

        private void CropRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(this);

            txtCursor.Text = $"{(int)point.X},{(int)point.Y}";
            if (isCropping)
            {
                currentPoint = point;
                // Update the size and position of the cropping rectangle here
                // You can set the Rectangle's Width, Height, and Margin properties.
            }
        }

        private void CropRectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            currentPoint = e.GetPosition(this);
            isCropping = false;
            CropAndDisplayImage();
        }

        // Function to create a cropped BitmapSource
        private static BitmapSource? CropImage(BitmapSource? source, Int32Rect cropArea)
        {
            if (source == null)
            {
                return null;
            }

            // Create a CroppedBitmap object
            CroppedBitmap croppedBitmap = new CroppedBitmap(source, cropArea);

            // Clone the format of the source image (e.g., DPI, PixelFormat)
            FormatConvertedBitmap formattedBitmap = new FormatConvertedBitmap();
            formattedBitmap.BeginInit();
            formattedBitmap.Source = croppedBitmap;
            formattedBitmap.DestinationFormat = source.Format;
            formattedBitmap.EndInit();

            return formattedBitmap;
        }

        // Example usage
        private void CropAndDisplayImage()
        {
            // Assuming you have loaded the original image into 'originalImageSource'
            if (originalImageSource is null)
            {
                return;
            }

            // Get the original image dimensions
            int originalWidth = originalImageSource.PixelWidth;
            int originalHeight = originalImageSource.PixelHeight;

            // Calculate the scaling factors
            double scaleX = (double)originalWidth / myImageControl.ActualWidth;
            double scaleY = (double)originalHeight / myImageControl.ActualHeight;

            // Define the cropping area as an Int32Rect (x, y, width, height)
            Int32Rect cropArea = new Int32Rect(
                (int)(startPoint.X * scaleX),
                (int)(startPoint.Y * scaleY),
                (int)((currentPoint.X - startPoint.X) * scaleX),
                (int)((currentPoint.Y - startPoint.Y) * scaleY)); // Example cropping area

            // Call the CropImage function to create a cropped BitmapSource
            croppedImageSource = CropImage(originalImageSource, cropArea);

            // Display the cropped image in an Image control
            croppedImageControl.Source = croppedImageSource; // 'croppedImageControl' is your Image control
        }

        // Function to get a BitmapSource from an Image control's source
        private BitmapSource? GetBitmapSourceFromImageControl(Image imageControl)
        {
            if (imageControl.Source is BitmapSource bitmapSource)
            {
                return bitmapSource;
            }
            else if (imageControl.Source is BitmapFrame bitmapFrame)
            {
                return bitmapFrame;
            }
            else
            {
                // Handle other source types if needed
                return null;
            }
        }

        private void CmdSave_Click(object sender, RoutedEventArgs e)
        {
            if (croppedImageSource is not null)
            {
                SaveCroppedImageAsJpeg(croppedImageSource, $"{Path.Combine(Root, Path.GetFileNameWithoutExtension(m_ImageFiles[Index]))}.jpg");
                Index++;
                croppedImageControl.Source = null;
                myImageControl.Source = new BitmapImage(new Uri(m_ImageFiles[Index]));
                originalImageSource = GetBitmapSourceFromImageControl(myImageControl);
            }
        }

        private void SaveCroppedImageAsJpeg(BitmapSource croppedImage, string outputPath)
        {
            try
            {
                // Create a JpegBitmapEncoder
                JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder();

                // Set the quality level (optional)
                jpegEncoder.QualityLevel = 100; // Adjust this value as needed (0-100)

                // Add the cropped image to the encoder
                jpegEncoder.Frames.Add(BitmapFrame.Create(croppedImage));

                // Create a FileStream to write the image file
                using (FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    // Encode and save the image
                    jpegEncoder.Save(fs);
                }

                // Display a success message (optional)
                // MessageBox.Show("Image saved as JPEG successfully.");
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the saving process
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void CmdRepeat_Click(object sender, RoutedEventArgs e)
        {
            CropAndDisplayImage();
        }
    }
}
