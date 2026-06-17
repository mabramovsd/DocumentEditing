using DocumentEditing.Models;

namespace DocumentEditing.Services
{
    public interface IDocumentFileSystemService
    {
        /// <summary>
        /// Root directory (with documents)
        /// </summary>
        string RootDirectory { get; }

        /// <summary>
        /// List of documents in root folder
        /// </summary>
        List<DocumentModelForList> GetDocumentsList();

        /// <summary>
        /// Creating new document
        /// </summary>
        /// <param name="documentPath">File address</param>
        void CreateNewDocument(string documentPath);
    }
}
