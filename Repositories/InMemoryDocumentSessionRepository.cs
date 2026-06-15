using DocumentEditing.Repositories;
using System.Collections.Concurrent;

public class InMemoryDocumentSessionRepository : IDocumentSessionRepository
{
    /// <summary>
    /// Dictionary of documents and people who edit them (docId-ListOfUserIds)
    /// </summary>
    private readonly ConcurrentDictionary<string, HashSet<string>> _sessions = new();

    public void AddEditor(string documentId, string userId)
    {
        _sessions.AddOrUpdate(
            documentId,
            _ => new HashSet<string> { userId },
            (_, users) => new HashSet<string>(users) { userId });
    }

    public void RemoveEditor(string documentId, string userId)
    {
        if (_sessions.TryGetValue(documentId, out var users))
        {
            users.Remove(userId);
            if (users.Count == 0)
            {
                _sessions.TryRemove(documentId, out _);
            }
        }
    }

    public IReadOnlyCollection<string> GetEditors(string documentId)
    {
        return _sessions.TryGetValue(documentId, out var users)
            ? users.ToList().AsReadOnly()
            : Array.Empty<string>();
    }

    public bool IsUserEditing(string documentId, string userId)
    {
        return _sessions.TryGetValue(documentId, out var users)
            && users.Contains(userId);
    }
}