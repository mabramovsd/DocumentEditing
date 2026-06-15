namespace DocumentEditing.Libs
{
    /// <summary>
    /// Static class with global variables (yes, antipattern)
    /// </summary>
    public static class Global
    {
        /// <summary>
        /// Dictionary of opened documents (key - document, value - list of users who edit (in previous version it can be more than one person))
        /// </summary>
        public static Dictionary<string, List<string>> ActiveDocuments = new Dictionary<string, List<string>>();
    }
}
