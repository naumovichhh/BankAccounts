using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BankAccounts.Data;
using BankAccounts.Models;
using BankAccounts.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace BankAccounts.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, IWebHostEnvironment environment)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var filesList = await _unitOfWork.FileRepository.GetAllAsync();
            return View(filesList);
        }

        [HttpGet]
        public IActionResult UploadFile()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile uploadFile)
        {
            if (uploadFile != null)
            {
                var excelHelper = new ExcelHelper(_unitOfWork);
                try
                {
                    using (var stream = uploadFile.OpenReadStream())
                    {
                        await excelHelper.SaveFileToDbAsync(stream, uploadFile.FileName);
                    }
                }
                catch
                {
                    return RedirectToAction("InvalidFile");
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult InvalidFile()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ViewFile(int? id)
        {
            if (id == null)
                return NotFound();
            
            var excelHelper = new ExcelHelper(_unitOfWork);
            var fileView = await excelHelper.GetFileViewAsync(id.Value);
            if (fileView != null)
                return View(fileView);
            else
                return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> DeleteFile(int? id)
        {
            if (id == null)
                return NotFound();

            var file = await _unitOfWork.FileRepository.GetByIdAsync(id);
            if (file == null)
                return NotFound();

            return View(file);
        }

        [HttpPost]
        [ActionName("DeleteFile")]
        public async Task<IActionResult> DeleteFileConfirmed(int id)
        {
            var excelHelper = new ExcelHelper(_unitOfWork);
            await excelHelper.DeleteFileAsync(id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DownloadFile(int? id)
        {
            if (id == null)
                return NotFound();


            var excelHelper = new ExcelHelper(_unitOfWork);
            string fileName = (await _unitOfWork.FileRepository.GetByIdAsync(id)).Name;
            fileName = Path.GetFileNameWithoutExtension(fileName) + "." + "xlsx";
            var byteArray = await excelHelper.GetFileContentAsync(id.Value);

            return File(byteArray, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
