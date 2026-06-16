using DocumentEditing.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DocumentEditing.Controllers
{
    [Route("Audit")]
    public class AuditController : Controller
    {
        /// <summary>
        /// Folder with documents to edit
        /// </summary>
        private readonly string _dir;
        /// <summary>
        /// List of documents (refreshed every time we opened /Audit)
        /// </summary>
        private List<string> _documents;

        private readonly ILogger<AuditController> _logger;

        public AuditController(ILogger<AuditController> logger)
        {
            _documents = new List<string>();
            _dir = "C:\\Users\\abram\\source\\repos\\DocumentEditing\\Audit";
            _logger = logger;

            if (!Directory.Exists(_dir))
                Directory.CreateDirectory(_dir);
        }

        [HttpGet]
        public IActionResult Index()
        {
            _documents = Directory
                .GetFiles(_dir)
                .Where(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                .Select(f => Path.GetFileName(f))
                .ToList();

            var model = new DocumentsModel { Documents = _documents };
            return View(model);
        }

        [HttpGet("Details/{id}")]
        public IActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("File name is empty");

            var filePath = Path.Combine(_dir, id);
            if (!id.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                return NotFound("File have wrong extension");
            try 
            {
                var model = DocumentModel.FillDataFromFile(_dir, id);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Try to open file wasn't successfull: {id}. Error: {ex.Message}");
                return BadRequest("Error");
            }
        }

        //Disable caching for error messages
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("Error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}