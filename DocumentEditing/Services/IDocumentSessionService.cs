namespace DocumentEditing.Services
{
    public interface IDocumentSessionService
    {
        /// <summary>
        /// Adding user to list of current document's editors
        /// </summary>
        /// <param name="documentId">Document</param>
        /// <param name="userId">User</param>
        void AddEditor(string documentId, string userId);

        /// <summary>
        /// Removing user from list of current document's editors
        /// </summary>
        /// <param name="documentId">Document</param>
        /// <param name="userId">User</param>
        void RemoveEditor(string documentId, string userId);

        /// <summary>
        /// Checks if user can edit document
        /// </summary>
        /// <param name="documentId">Document</param>
        /// <param name="userId">User</param>
        /// <returns><see langword="true"/>, if user can edit this document; else <see langword="false"/>.</returns>
        bool CanUserEdit(string documentId, string userId);

        /// <summary>
        /// Creating new document
        /// </summary>
        /// <param name="documentPath">File address</param>
        /// <returns></returns>
        bool CreateNewDocument(string documentPath);
    }
}
