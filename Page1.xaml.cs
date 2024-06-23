using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;
using System.Drawing.Imaging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ForDaCameraTest_FINALS_
{
    public partial class Page1 : Page
    {
        private FilterInfoCollection _videoDevices;
        private VideoCaptureDevice _videoDevice;

        public Page1()
        {
            InitializeComponent();
            tbSurDate.Text = DateTime.Now.ToString("MMMM dd, yyyy");
            LoadVideoDevices();
        }

        private void LoadVideoDevices()
        {
            _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in _videoDevices)
            {
                cmbCamera.Items.Add(device.Name);
            }
            if (cmbCamera.Items.Count > 0)
            {
                cmbCamera.SelectedIndex = 0;
            }
        }

        private void cmbCamera_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_videoDevice != null)
            {
                _videoDevice.SignalToStop();
                _videoDevice.WaitForStop();
            }

            _videoDevice = new VideoCaptureDevice(_videoDevices[cmbCamera.SelectedIndex].MonikerString);
            _videoDevice.NewFrame += VideoDevice_NewFrame;
        }

        private void VideoDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            pic.Dispatcher.Invoke(() =>
            {
                pic.Source = BitmapToImageSource(eventArgs.Frame.Clone() as Bitmap);
            });
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (_videoDevice != null)
            {
                _videoDevice.Start();
            }
        }

        private void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            if (_videoDevice != null && _videoDevice.IsRunning)
            {
                _videoDevice.SignalToStop();
                _videoDevice.WaitForStop();
            }

            if (pic.Source is BitmapSource bitmapSource)
            {
                SaveBitmapSourceToFile(bitmapSource, "photo.jpg");
            }
        }

        private void SaveBitmapSourceToFile(BitmapSource bitmapSource, string filePath)
        {
            BitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
    }
}
