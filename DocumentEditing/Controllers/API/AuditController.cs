using DocumentEditing.Libs;
using DocumentEditing.Models;
using DocumentEditing.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DocumentEditing.Controllers.API
{
    [ApiController]
    [Route("Api/Audit")]
    public class AuditController : ControllerBase
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
        public ActionResult<DocumentModel> GetDocumentDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { error = "File name is empty" });

            if (!id.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { error = "File not found or has wrong extension" });

            try
            {
                var model = DocumentModel.FillDataFromFile(_dir, id);
                return Ok(model);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning($"File not found: {id}. Error: {ex.Message}");
                return NotFound(new { error = $"File '{id}' was not found." });
            }
            catch (IOException ex)
            {
                _logger.LogWarning($"IO Error while opening file: {id}. Error: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error while reading the file." });
            }
        }
    }
}