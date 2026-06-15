namespace DocumentEditing.Models.Messages
{
    public class OutgoingMessage
    {
        public string FileName { get; set; }
        public string User { get; set; }

        public Dictionary<string, List<string>> ActiveDocuments { get; set; }
    }
}