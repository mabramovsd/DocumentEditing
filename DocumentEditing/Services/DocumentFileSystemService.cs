using DocumentEditing.Libs;
using DocumentEditing.Models;
using Microsoft.Extensions.Options;

namespace DocumentEditing.Services
{
    public class DocumentFileSystemService : IDocumentFileSystemService
    {
        private readonly string _rootDirectory;

        public DocumentFileSystemService(IOptions<DirectorySettings> directorySettings)
        {
            _rootDirectory = directorySettings.Value.Documents;
        }

        public string RootDirectory => _rootDirectory;

        public List<DocumentModelForList> GetDocumentsList()
        {
            return Directory.GetFiles(_rootDirectory)
                .Select(f => new FileInfo(f))
                .Where(fi => fi.Extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
                .Select(fi => new DocumentModelForList
                {
                    FileName = fi.Name,
                    CreationTime = fi.CreationTime,
                    LastWriteTime = fi.LastWriteTime,
                    SizeKB = Math.Round((double)fi.Length / 1024, 2)
                })
                .ToList();
        }

        public void CreateNewDocument(string fileName)
        {
            var fullPath = Path.Combine(_rootDirectory, fileName);
            if (!File.Exists(fullPath))
            {
                using (File.Create(fullPath)) { }
            }
        }
    }
}