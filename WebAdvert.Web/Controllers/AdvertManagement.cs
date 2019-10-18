using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using WebAdvert.Web.Models.AdvertManagement;
using WebAdvert.Web.ServiceClients;
using WebAdvert.Web.Services;

namespace WebAdvert.Web.Controllers
{
    public class AdvertManagement : Controller
    {
        private readonly IFileUploader _fileUploader;
        private readonly IAdvertApiClient _advertClientApi;
        private readonly IMapper _mapper;

        public AdvertManagement(IFileUploader fileUploader, IAdvertApiClient advertClientApi, IMapper mapper)
        {
            _fileUploader = fileUploader;
            _advertClientApi = advertClientApi;
            _mapper = mapper;
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateAdvertViewModel model, IFormFile imageFile)
        {

            if (!ModelState.IsValid) return View(model);

            if (imageFile == null) return View(model);

            string id = await CreateAdvert(model);
            string filePath = GetFilePath(imageFile, id);

            try
            {
                await UploadImage(imageFile, filePath);
                await ConfirmAdvert(id);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                await CancelAdvertCreation(id);

                // Missing loggers for now
                Console.WriteLine(e);
            }
            return View(model);
        }

        private async Task CancelAdvertCreation(string id)
        {
            var cancelModel = new ConfirmAdvertRequest { Id = id, Status = AdvertApi.Models.AdvertStatus.Pending };
            await _advertClientApi.Confirm(cancelModel);
        }

        private async Task ConfirmAdvert(string id)
        {
            var confirmModel = new ConfirmAdvertRequest { Id = id, Status = AdvertApi.Models.AdvertStatus.Active };
            var confirmed = await _advertClientApi.Confirm(confirmModel);
            if (!confirmed)
            {
                throw new Exception($"Cannot confirm advert of id = {id}");
            }
        }

        private async Task UploadImage(IFormFile imageFile, string filePath)
        {
            using (var readStream = imageFile.OpenReadStream())
            {
                var result = await _fileUploader.UploadFileAsync(filePath, readStream);
                if (!result)
                    throw new Exception("Could not upload the image to file repository. Please see the logs for more details.");
            }
        }

        private static string GetFilePath(IFormFile imageFile, string id)
        {
            var fileName = !string.IsNullOrEmpty(imageFile.FileName) ? Path.GetFileName(imageFile.FileName) : id;
            var filePath = $"{id}/{fileName}";
            return filePath;
        }

        private async Task<string> CreateAdvert(CreateAdvertViewModel model)
        {
            var createAdvertModel = _mapper.Map<CreateAdvertModel>(model);
            var response = await _advertClientApi.Create(createAdvertModel);
            var id = response.Id;
            return id;
        }
    }
}