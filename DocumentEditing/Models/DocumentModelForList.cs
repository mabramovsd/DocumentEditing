namespace DocumentEditing.Models
{
    /// <summary>
    /// Model to view document in list
    /// </summary>
    public class DocumentModelForList
    {
        public string FileName { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastWriteTime { get; set; }

        /// <summary>
        /// Size in KB
        /// </summary>
        public double SizeKB { get; set; }
    }
}
