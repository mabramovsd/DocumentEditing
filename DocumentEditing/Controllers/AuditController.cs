using DocumentEditing.Libs;
using DocumentEditing.Models;
using DocumentEditing.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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

        private readonly ILogger<AuditController> _logger;
        private readonly IDocumentFileSystemService _documentFileSystemService;

        public AuditController(
            ILogger<AuditController> logger,
            [FromKeyedServices(DependencyKeys.AuditService)] IDocumentFileSystemService documentFileSystemService,
            IOptions<DirectorySettings> directorySettings)
        {
            _dir = directorySettings.Value.Audit;
            _logger = logger;
            _documentFileSystemService = documentFileSystemService;

            if (!Directory.Exists(_dir))
                Directory.CreateDirectory(_dir);
        }

        [HttpGet("Index")]
        public ActionResult<DocumentsModel> GetDocuments()
        {
            var documents = _documentFileSystemService.GetDocumentsList();
            var model = new DocumentsModel { Path = _dir, Documents = documents };
            return Ok(model);
        }

        [HttpGet("Details/{id}")]
        public IActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("File name is empty");

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