using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebCipher.Models;
using System.IO;
using System.Text;

namespace WebCipher.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (TempData["cipheredText"] != null && (string)TempData["cipheredData"] != "")
            {
                var form = TempDataHelper.Get<CipherModel>(TempData, "cipherForm");
                form.OutputText = (string)TempData["cipheredText"];
                return View(form);
            }
            return View(new CipherModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Decipher([Bind("InputText,InputFile,Direction,Key,UseFile,FileEncoding")] CipherModel form)
        {
            if (ModelState.IsValid)
            {
                string cipheredText;
                if (form.UseFile)
                {
                    cipheredText = CipherModel.DoCiphering(form.InputFile, form.Key, form.Direction == "Cipher", form.FileEncoding);
                }
                else
                {
                    cipheredText = CipherModel.DoCiphering(form.InputText, form.Key, form.Direction == "Cipher");
                }
                TempData["cipheredText"] = cipheredText;
                TempDataHelper.Put(TempData, "cipherForm", form);
                return RedirectToAction(nameof(Index));
            }
            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult DownloadFileDOCX([Bind("OutputText")] CipherModel form)
        {
            var content = form.OutputText;
            var contentType = "application/octet-stream";
            var fileName = "result.docx";
            XWPFDocument doc = new();
            var par = doc.CreateParagraph();
            var run = par.CreateRun();
            run.SetText(content);
            using MemoryStream stream = new();
            doc.Write(stream);
            byte[] byteContent = stream.ToArray();
            return File(byteContent, contentType, fileName);
        }

        [HttpPost]
        public IActionResult DownloadFileTXT([Bind("OutputText")] CipherModel form)
        {
            byte[] content = new UTF8Encoding(true).GetBytes(form.OutputText);
            var contentType = "application/octet-stream";
            var fileName = "result.txt";
            return File(content, contentType, fileName);
        }
    }
}
