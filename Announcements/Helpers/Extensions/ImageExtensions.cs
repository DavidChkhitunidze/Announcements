using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Announcements.Helpers.Extensions
{
    public static class ImageExtensions
    {
        public static async Task UloadFilesIfExistsAsync<T>(this T announcement, IFormFileCollection files, IHostingEnvironment hostingEnvironment) where T : AnnouncementManipulation
        {
            foreach (var image in files)
            {
                if (image != null && image.Length > 0)
                {
                    var uploads = Path.Combine(hostingEnvironment.WebRootPath, "images\\content");
                    var fileName = Path.GetTempFileName() + Path.GetExtension(image.FileName);

                    using (var fileStream = new FileStream(Path.Combine(uploads, fileName), FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                        announcement.ImageName = fileName;
                    }
                }
            }
        }
    }
}
