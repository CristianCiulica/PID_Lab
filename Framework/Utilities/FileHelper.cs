using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Emgu.CV;
namespace Framework.Utilities
{
    internal static class FileHelper
    {
        private static readonly FilePickerFileType ImageFilter = new FilePickerFileType("Image files (*.jpg, *.jpeg, *.jfif, *.jpe, *.bmp, *.png)")
        { Patterns=new[]{"*.jpg","*.jpeg","*.jfif","*.jpe","*.bmp","*.png"}, MimeTypes=new[]{"image/jpeg","image/bmp","image/png"} };
        public static async Task<string> LoadFileDialog(string title)
        {
            var files=await DesktopRuntime.MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {Title=title,AllowMultiple=false,FileTypeFilter=new[]{ImageFilter}});
            return files.FirstOrDefault()?.TryGetLocalPath();
        }
        public static Image<TColor,byte> LoadImage<TColor>(string path) where TColor:struct,IColor => Image<TColor,byte>.Load(path);
        public static async Task<string> SaveFileDialog(string imageName)
        {
            var file=await DesktopRuntime.MainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {SuggestedFileName=imageName,DefaultExtension="jpg",FileTypeChoices=new[]{ImageFilter},ShowOverwritePrompt=true});
            return file?.TryGetLocalPath();
        }
        public static void SaveImage<TColor>(Image<TColor,byte> image,string path) where TColor:struct,IColor => image.Save(path);
    }
}
