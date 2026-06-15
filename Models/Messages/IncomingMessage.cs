namespace DocumentEditing.Models.Messages
{
    public class IncomingMessage
    {
        public string FileName { get; set; }
        public string Content { get; set; }
        public string User { get; set; }
    }
}
