using DocumentEditing.Libs;
using DocumentEditing.Models;
using DocumentEditing.Services;
using Microsoft.AspNetCore.Mvc;
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
        /// <summary>
        /// List of documents (refreshed every time we opened /Documents)
        /// </summary>
        private List<string> _documents;

        private readonly ILogger<DocumentsController> _logger;
        private readonly DocumentLockService _documentLockService;
        private static readonly object _syncRoot = new object();

        public DocumentsController(ILogger<DocumentsController> logger, DocumentLockService documentLockService)
        {
            _documents = new List<string>();
            _dir = "C:\\Users\\abram\\source\\repos\\DocumentEditing\\Documents";
            _logger = logger;
            _documentLockService = documentLockService;

            if (!Directory.Exists(_dir))
                Directory.CreateDirectory(_dir);
        }

        // GET: /Documents
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

        // GET: /Documents/Edit/{id}
        [HttpGet("Edit/{id}")]
        public IActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Имя файла не указано.");

            var filePath = Path.Combine(_dir, id);
            if (!System.IO.File.Exists(filePath) || !id.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                return NotFound("Файл не найден.");


            bool canEdit = false;

            // Блокируем доступ к словарю ActiveDocuments
            lock (_syncRoot)
            {
                if (!Global.ActiveDocuments.ContainsKey(id))
                {
                    canEdit = true;
                }
            }
            bool lockAcquired = _documentLockService.TryAcquireWriteLock(id);

            var model = DocumentModel.FillDataFromFile(_dir, id);
            model.IsReadOnly = !canEdit;
            model.IsReadOnly = !lockAcquired;

            if (!canEdit)
            {
                ViewBag.Message = "Сорри, документ уже открыт на редактирование.";
            }

            return View(model);
        }

        // POST: /Documents/Save
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
                    Audit.AddData(model.FileName, changes);
                    _logger.LogInformation($"Зафиксированы изменения в файле {model.FileName}.");
                }

                System.IO.File.WriteAllText(filePath, model.Content);
                _logger.LogInformation($"Файл {model.FileName} успешно сохранён.");

                return Ok(new { message = "Файл сохранён", fileName = model.FileName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при сохранении файла {model.FileName}.");
                return StatusCode(500, new { error = "Ошибка при сохранении файла." });
            }
        }

        // POST: /Documents/Close
        [HttpPost("Close")]
        public IActionResult OnPageClose([FromBody] CloseDocumentModel request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.FileName))
                return BadRequest("Некорректные данные: имя файла обязательно.");

            try
            {
                string fileName = request.FileName ?? "";
                string user = request.User ?? "";

                _documentLockService.ReleaseWriteLock(fileName);

                lock (_syncRoot)
                {
                    if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(user))
                    {
                        if (Global.ActiveDocuments.ContainsKey(fileName) &&
                            Global.ActiveDocuments[fileName].Contains(user))
                        {
                            Global.ActiveDocuments[fileName].Remove(user);
                        }

                        if (Global.ActiveDocuments.ContainsKey(fileName) &&
                            Global.ActiveDocuments[fileName].Count == 0)
                        {
                            Global.ActiveDocuments.Remove(fileName);
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine("Ошибка при разборе JSON: " + ex.Message);
            }

            _logger.LogInformation($"Пользователь закрыл документ: {request.FileName}");
            return Ok();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("Error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}