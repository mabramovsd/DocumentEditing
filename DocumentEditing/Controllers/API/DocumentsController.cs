using DocumentEditing.Libs;
using DocumentEditing.Models;
using DocumentEditing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DocumentEditing.Controllers.API
{
    [ApiController]
    [Route("Api/Documents")]
    public class DocumentsController : ControllerBase
    {
        /// <summary>
        /// Folder with documents to edit
        /// </summary>
        private readonly string _dir;

        private readonly ILogger<DocumentsController> _logger;
        private readonly IAuditService _auditService;
        private readonly DocumentLockService _documentLockService;
        private readonly IDocumentSessionService _documentSessionService;
        private readonly IDocumentFileSystemService _documentFileSystemService;

        public DocumentsController(
            ILogger<DocumentsController> logger,
            DocumentLockService documentLockService,
            IDocumentSessionService documentSessionService,
            IOptions<DirectorySettings> directorySettings,
            IAuditService auditService,
            [FromKeyedServices(DependencyKeys.DocumentsService)] IDocumentFileSystemService documentFileSystemService)
        {
            _dir = directorySettings.Value.Documents;
            _logger = logger;
            _auditService = auditService;
            _documentLockService = documentLockService;
            _documentSessionService = documentSessionService;
            _documentFileSystemService = documentFileSystemService;

            if (!Directory.Exists(_dir))
                Directory.CreateDirectory(_dir);
        }

        [HttpGet("Index")]
        public ActionResult<DocumentsModel> Index()
        {
            var documents = _documentFileSystemService.GetDocumentsList();
            var model = new DocumentsModel { Path = _dir, Documents = documents };
            return Ok(model);
        }

        [HttpGet("Edit/{id}")]
        [Authorize]
        public ActionResult<DocumentModel> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { error = "File name is empty" });

            var filePath = Path.Combine(_dir, id);
            if (!id.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { error = "File have wrong extension or not found" });

            bool lockAcquired = false;

            try
            {
                lockAcquired = _documentLockService.TryAcquireWriteLock(id);
                var user = HttpContext.User?.Identity?.Name ?? "";
                if (_documentSessionService.CanUserEdit(id, user))
                {
                    _documentSessionService.AddEditor(id, user);
                }
                else
                {
                    lockAcquired = false;
                }

                var model = DocumentModel.FillDataFromFile(_dir, id);
                model.IsReadOnly = !lockAcquired;

                return Ok(model);
            }
            catch (IOException ex) when (
                ex is FileNotFoundException ||
                ex is DirectoryNotFoundException)
            {
                _logger.LogWarning($"Try to open file that doesn't exist: {id}. Error: {ex.Message}");
                return NotFound(new { error = $"File '{id}' was not found." });
            }
            finally
            {
                if (lockAcquired)
                {
                    _documentLockService.ReleaseWriteLock(id);
                }
            }
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] SaveDocumentModel model)
        {
            if (string.IsNullOrWhiteSpace(model.FileName))
                return BadRequest(new { error = "File Name is empty" });

            var filePath = Path.Combine(_dir, model.FileName);

            try
            {
                _documentSessionService.CreateNewDocument(filePath);
                _logger.LogInformation($"File {model.FileName} successfully created");
                //Better than just OK
                return CreatedAtAction(nameof(Edit), new { id = model.FileName }, null);
            }
            catch (Exception ex) when (ex is IOException)
            {
                _logger.LogError(ex, $"IO Error when creating file {model.FileName}.");
                return Conflict(new { error = "File already exists or access denied." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error when creating file {model.FileName}.");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] SaveDocumentModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.FileName))
                return BadRequest("Incorrect Data: Empty file name");

            var filePath = Path.Combine(_dir, model.FileName);

            try
            {
                var modelOld = DocumentModel.FillDataFromFile(_dir, model.FileName);
                var changes = TextDifference.GetChangesList(modelOld?.Content ?? "", model.Content);

                if (changes.Count > 0)
                {
                    _auditService.AddData(model.FileName, changes);
                    _logger.LogInformation($"File was chaneged {model.FileName}.");
                }

                System.IO.File.WriteAllText(filePath, model.Content);
                _logger.LogInformation($"File {model.FileName} successfully saved");

                return Ok(new { message = "File successfully saved", fileName = model.FileName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when saving file {model.FileName}.");
                return StatusCode(500, new { error = "Error when saving file" });
            }
        }

        [HttpPost("Close")]
        public IActionResult OnPageClose([FromBody] CloseDocumentModel request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.FileName))
                return BadRequest("Incorrect Data: FileName is empty");

            try
            {
                string fileName = request.FileName ?? "";
                string user = request.User ?? "";

                _documentLockService.ReleaseWriteLock(fileName);
                _documentSessionService.RemoveEditor(fileName, user);
            }
            catch (JsonException ex)
            {
                Console.WriteLine("Error with JSON parsing: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            _logger.LogInformation($"User cloed document: {request.FileName}");
            return Ok();
        }
    }
}