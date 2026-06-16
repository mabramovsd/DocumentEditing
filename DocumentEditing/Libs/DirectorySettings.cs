namespace DocumentEditing.Libs
{
    /// <summary>
    /// Class with directory settings
    /// </summary>
    public class DirectorySettings
    {
        /// <summary>
        /// Folder with documents
        /// </summary>
        public string Documents { get; set; } = string.Empty;

        /// <summary>
        /// Folder with audit data
        /// </summary>
        public string Audit { get; set; } = string.Empty;
    }
}
