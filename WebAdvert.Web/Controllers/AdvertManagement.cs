using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using WebAdvert.Web.Models.AdvertManagement;
using WebAdvert.Web.Services;

namespace WebAdvert.Web.Controllers
{
    public class AdvertManagement : Controller
    {
        private readonly IFileUploader _fileUploader;

        public AdvertManagement(IFileUploader fileUploader)
        {
            _fileUploader = fileUploader;
        }

        public async Task<IActionResult> Create(CreateAdvertViewModel model, IFormFile imageFile)
        {

            if (!ModelState.IsValid) return View(model);

            var id = "12343434"; // any random for now. We'll get from api soon.

            if (imageFile == null) return View(model);

            var fileName = !string.IsNullOrEmpty(imageFile.FileName) ? Path.GetFileName(imageFile.FileName) : id;
            var filePath = $"{id}/{fileName}";

            try
            {
                using (var readStream = imageFile.OpenReadStream())
                {
                    var result = await _fileUploader.UploadFileAsync(filePath, readStream);
                    if (!result)
                        throw new Exception("Could not upload the image to file repository. Please see the logs for more details.");
                }

                // TODO: call api and confirm ad creation

                return RedirectToAction("Index", "Home");

            }
            catch (Exception e)
            {
                // TODO: call api and cancel ad
                Console.WriteLine(e);
            }
            return View(model);
        }
    }
}