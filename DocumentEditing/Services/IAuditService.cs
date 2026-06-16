namespace DocumentEditing.Services
{
    public interface IAuditService
    {
        /// <summary>
        /// Adding data to audit file
        /// </summary>
        /// <param name="fileName">File we edit</param>
        /// <param name="dataToAdd">Changed data</param>
        bool AddData(string fileName, List<string> dataToAdd);
    }
}
