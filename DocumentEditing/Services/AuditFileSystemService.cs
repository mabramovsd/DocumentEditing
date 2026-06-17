using DocumentEditing.Libs;
using DocumentEditing.Models;
using Microsoft.Extensions.Options;

namespace DocumentEditing.Services
{
    public class AuditFileSystemService : IDocumentFileSystemService
    {
        private readonly string _rootDirectory;

        public AuditFileSystemService(IOptions<DirectorySettings> directorySettings)
        {
            _rootDirectory = directorySettings.Value.Audit;
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
            throw new NotImplementedException();
        }
    }
}