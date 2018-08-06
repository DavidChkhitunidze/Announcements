using Announcements.Helpers;
using Announcements.Helpers.Extensions;
using Announcements.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Announcements.Controllers
{
    public class AnnouncementController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IOptions<AnnouncementsApiUrl> _apiConfig;
        private readonly IUrlHelper _urlHelper;
        private readonly AnnouncementClient _announcementClient;

        private struct PaginationData
        {
            public int TotalCount;
            public int PageSize;
            public int CurrentPage;
            public int TotalPages;
            public string PreviousPageLink;
            public string NextPageLink;
        }

        public AnnouncementController(IOptions<AnnouncementsApiUrl> apiConfig, IUrlHelper urlHelper, AnnouncementClient announcementClient, IHostingEnvironment hostingEnvironment)
        {
            _apiConfig = apiConfig;
            _urlHelper = urlHelper;
            _announcementClient = announcementClient;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<IActionResult> Index(AnnouncementResourceParameters announcementResourceParameters)
        {
            var response = await _announcementClient.GetAnnouncementsAsync(announcementResourceParameters.PageNumber,
                announcementResourceParameters.PageSize,
                announcementResourceParameters.SearchString,
                "id,title,imagename");

            var announcements = response.Stream.GetDeserializedDataFromStream<IEnumerable<AnnouncementGettingViewModel>>();
            var paginationJson = response.Headers["X-Pagination"].FirstOrDefault();
            var pagination = JsonConvert.DeserializeObject<PaginationData>(paginationJson);

            ViewBag.PrevPage = pagination.PreviousPageLink;
            ViewBag.NextPage = pagination.NextPageLink;
            ViewBag.PageNumber = announcementResourceParameters.PageNumber;

            return View(announcements);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
                return NotFound();

            var response = await _announcementClient.GetAnnouncementAsync(id, "");
            var announcement = response.Stream.GetDeserializedDataFromStream<AnnouncementGettingViewModel>();

            return View(announcement);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnnouncementForCreation announcement)
        {
            if (announcement == null)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files != null && files.Count > 0)
                    await announcement.UloadFilesIfExistsAsync(files, _hostingEnvironment);

                var response = await _announcementClient.CreateAnnouncementAsync(announcement);
                var createdAnnouncement = response.Stream.GetDeserializedDataFromStream<AnnouncementGettingViewModel>();

                return RedirectToAction(nameof(Details), new { id = createdAnnouncement.ID });
            }
            return View(announcement);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
                return NotFound();

            var response = await _announcementClient.GetAnnouncementAsync(id, "");
            var announcement = response.Stream.GetDeserializedDataFromStream<AnnouncementForUpdate>();

            return View(announcement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AnnouncementForUpdate announcement)
        {
            if (announcement == null)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files != null && files.Count > 0)
                    await announcement.UloadFilesIfExistsAsync(files, _hostingEnvironment);
                else
                {
                    var responseForImageName = await _announcementClient.GetAnnouncementAsync(id, "imagename");
                    var imageName = responseForImageName.Stream.GetDeserializedDataFromStream<AnnouncementGettingViewModel>().ImageName;
                    announcement.ImageName = imageName;
                }

                var response = await _announcementClient.PutAnnouncementAsync(id, announcement);
                var updatedAnnouncement = response.Stream.GetDeserializedDataFromStream<AnnouncementGettingViewModel>();

                return RedirectToAction(nameof(Details), new { id = updatedAnnouncement.ID });
            }

            return View(announcement);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
                return NotFound();

            var response = await _announcementClient.DeleteAnnouncementAsync(id);
            if (response == null)
                return RedirectToAction(nameof(Index));
            return RedirectToAction(nameof(Details), new { id });
        }

        public IActionResult Contact() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
