namespace DocumentEditing.Repositories
{
    /// <summary>
    /// Repo for document editing sessions
    /// </summary>
    public interface IDocumentSessionRepository
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
        /// List of people who edit document
        /// </summary>
        /// <param name="documentId">Document</param>
        IReadOnlyCollection<string> GetEditors(string documentId);

        /// <summary>
        /// Checks if user edited this doc
        /// </summary>
        /// <param name="documentId">Document</param>
        /// <param name="userId">User</param>
        /// <returns><see langword="true"/>, if user edit this document; else <see langword="false"/>.</returns>
        bool IsUserEditing(string documentId, string userId);
    }
}
