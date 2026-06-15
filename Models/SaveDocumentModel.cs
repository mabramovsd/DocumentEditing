namespace DocumentEditing.Models
{
    /// <summary>
    /// Model to save edited content
    /// </summary>
    public class SaveDocumentModel
    {
        public string FileName { get; set; }
        public string Content { get; set; }
    }
}
