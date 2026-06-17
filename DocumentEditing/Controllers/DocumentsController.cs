using DocumentEditing.Libs;
using DocumentEditing.Models;
using DocumentEditing.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace DocumentEditing.Controllers
{
    [Route("Documents")]
    [ApiController]
    public class DocumentsController : Controller
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

        [HttpGet]
        public IActionResult Index()
        {
            var documents = _documentFileSystemService.GetDocumentsList();
            var model = new DocumentsModel { Path = _dir, Documents = documents };
            return View(model);
        }

        [HttpGet("Edit/{id}")]
        public IActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("File name is empty");

            var filePath = Path.Combine(_dir, id);
            if (!id.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                return NotFound("File have wrong extension");

            bool lockAcquired = _documentLockService.TryAcquireWriteLock(id);
            try
            {
                var model = DocumentModel.FillDataFromFile(_dir, id);
                model.IsReadOnly = !lockAcquired;

                if (!lockAcquired)
                {
                    ViewBag.Message = "Sorry, document is already opened";
                }

                return View(model);
            }
            catch (IOException ex) when (
                ex is FileNotFoundException ||
                ex is DirectoryNotFoundException)
            {
                if (lockAcquired)
                {
                    _documentLockService.ReleaseWriteLock(id);
                }

                _logger.LogWarning($"Try to open file that doesn't exist: {id}. Error: {ex.Message}");
                return NotFound("File not found");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Try to open file wasn't successfull: {id}. Error: {ex.Message}");
                return BadRequest("Error");
            }
        }


        [HttpPost("Create")]
        public IActionResult Create([FromBody] SaveDocumentModel model)
        {
            if (string.IsNullOrWhiteSpace(model.FileName))
                return BadRequest("File Name is empty");

            var filePath = Path.Combine(_dir, model.FileName);

            try
            {
                _documentSessionService.CreateNewDocument(filePath);
                //_auditService.AddData(model.FileName, changes);
                _logger.LogInformation($"File {model.FileName} successfully created");

                return Ok(new { message = "File successfully created", fileName = model.FileName, redirectUrl = "Documents" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when creating file {model.FileName}.");
                return StatusCode(500, new { error = "Error when creating file" });
            }
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] SaveDocumentModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.FileName))
                return BadRequest("Некорректные данные: имя файла обязательно.");

            var filePath = Path.Combine(_dir, model.FileName);

            try
            {
                var modelOld = DocumentModel.FillDataFromFile(_dir, model.FileName);
                var changes = TextDifference.GetChangesList(modelOld?.Content ?? "", model.Content);

                if (changes.Count > 0)
                {
                    _auditService.AddData(model.FileName, changes);
                    _logger.LogInformation($"Зафиксированы изменения в файле {model.FileName}.");
                }

                System.IO.File.WriteAllText(filePath, model.Content);
                _logger.LogInformation($"File {model.FileName} successfully saved");

                return Ok(new { message = "File successfully saved", fileName = model.FileName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при сохранении файла {model.FileName}.");
                return StatusCode(500, new { error = "Ошибка при сохранении файла." });
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

            _logger.LogInformation($"Пользователь закрыл документ: {request.FileName}");
            return Ok();
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