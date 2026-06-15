using DocumentEditing.Repositories;
using Microsoft.AspNetCore.Http;

namespace DocumentEditing.Services
{
    public class DocumentSessionService : IDocumentSessionService
    {
        private readonly IDocumentSessionRepository _repository;

        public DocumentSessionService(IDocumentSessionRepository repository)
        {
            _repository = repository;
        }

        public void AddEditor(string documentId, string userId)
        {
            _repository.AddEditor(documentId, userId);
        }

        public void RemoveEditor(string documentId, string userId)
        {
            _repository.RemoveEditor(documentId, userId);
        }

        public bool CanUserEdit(string documentId, string userId)
        {
            return
                _repository.IsUserEditing(documentId, userId) || 
                _repository.GetEditors(documentId).Count == 0;
        }
    }
}
