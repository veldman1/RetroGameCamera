using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using RetroGameCamera;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(Retro_Game_Camera.iOS.PhotoLibrary))]
namespace Retro_Game_Camera.iOS
{
    public class PhotoLibrary : IPhotoLibrary
    {

        public Task<bool> SavePhotoAsync(byte[] data, string folder, string filename)
        {
            NSData nsData = NSData.FromArray(data);
            UIImage image = new UIImage(nsData);
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            image.SaveToPhotosAlbum((UIImage img, NSError error) =>
            {
                taskCompletionSource.SetResult(error == null);
            });

            return taskCompletionSource.Task;
        }
    }
}