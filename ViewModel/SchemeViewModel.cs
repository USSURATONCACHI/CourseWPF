using CourseWPF.Commands;
using CourseWPF.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CourseWPF.ViewModel
{
    public class SchemeViewModel : ViewModelBase {
        private Project _project;

        private ImageSource? _imageSource;
        public ImageSource? ImageSource {
            get => _imageSource;
            set {
                _imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
            }
        }
        public ICommand ChangeImage { get; }

        public SchemeViewModel(Project project) {
            _project = project;
            ChangeImage = new ChangeImageCommand(project);
            _project.ImageChanged += LoadImage;
            LoadImage();
        }

        private void LoadImage() {
            try {
                var imageuri = new Uri(_project.ImagePath); 
                ImageSource = ImageSourceFromBitmap(BitmapImageToBitmap(new BitmapImage(imageuri)));
                Debug.WriteLine($"Image source f: {ImageSource}");
            } catch (Exception) {
                ImageSource = null;
                Debug.WriteLine($"Image source g: {ImageSource}");
            }
            Debug.WriteLine($"Image source: {ImageSource}");
        }

        public static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage) {
            using (MemoryStream outStream = new MemoryStream()) {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public static ImageSource ImageSourceFromBitmap(Bitmap bmp) {
            var handle = bmp.GetHbitmap();
            try {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            } finally { 
                //DeleteObject(handle); 
            }
        }
    }
}
