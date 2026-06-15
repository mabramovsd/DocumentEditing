namespace DocumentEditing.Libs
{
    public static class Audit
    {
        private static string _auditDir = "C:\\Users\\abram\\source\\repos\\DocumentEditing\\Audit";
 
        /// <summary>
        /// Adding data to audit file
        /// </summary>
        /// <param name="fileName">File we edit</param>
        /// <param name="dataToAdd">Changed data</param>
        /// <returns></returns>
        public static bool AddData(string fileName, List<string> dataToAdd)
        {
            var filePath = Path.Combine(_auditDir, fileName);

            if (!File.Exists(filePath))
            {
                using (File.Create(filePath)) 
                {
                }
            }

            File.AppendAllText(filePath, $"Date: {DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss")}");
            File.AppendAllText(filePath, Environment.NewLine);
            File.AppendAllText(filePath, "Changes:");
            File.AppendAllText(filePath, Environment.NewLine);
            File.AppendAllText(filePath, string.Join(Environment.NewLine,dataToAdd));
            File.AppendAllText(filePath, Environment.NewLine);
            File.AppendAllText(filePath, Environment.NewLine);

            return true;
        }
    }
}
