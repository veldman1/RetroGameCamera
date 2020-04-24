using RetroGameCamera;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Xamarin.Forms;

[assembly: Dependency(typeof(Retro_Game_Camera.UWP.PhotoLibrary))]
namespace Retro_Game_Camera.UWP
{
    public class PhotoLibrary : IPhotoLibrary
    {
        public async Task<bool> SavePhotoAsync(byte[] data, string folder, string filename)
        {
            StorageFolder picturesDirectory = KnownFolders.PicturesLibrary;
            StorageFolder folderDirectory = picturesDirectory;

            // Get the folder or create it if necessary
            if (!string.IsNullOrEmpty(folder))
            {
                try
                {
                    folderDirectory = await picturesDirectory.GetFolderAsync(folder);
                }
                catch
                { }

                if (folderDirectory == null)
                {
                    try
                    {
                        folderDirectory = await picturesDirectory.CreateFolderAsync(folder);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            try
            {
                // Create the file.
                StorageFile storageFile = await folderDirectory.CreateFileAsync(filename,
                                                    CreationCollisionOption.GenerateUniqueName);

                // Convert byte[] to Windows buffer and write it out.
                IBuffer buffer = WindowsRuntimeBuffer.Create(data, 0, data.Length, data.Length);
                await FileIO.WriteBufferAsync(storageFile, buffer);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
