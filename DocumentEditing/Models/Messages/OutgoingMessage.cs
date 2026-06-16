namespace DocumentEditing.Models.Messages
{
    public class OutgoingMessage
    {
        public string FileName { get; set; }
        public string User { get; set; }
        public IReadOnlyCollection<string> ActiveEditors { get; set; }
    }
}