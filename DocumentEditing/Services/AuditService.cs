using DocumentEditing.Libs;
using Microsoft.Extensions.Options;

namespace DocumentEditing.Services
{
    public class AuditService : IAuditService
    {
        private readonly string _auditDir;

        public AuditService(IOptions<DirectorySettings> directorySettings) 
        {
            _auditDir = directorySettings.Value.Audit;
        }

        public bool AddData(string fileName, List<string> dataToAdd)
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
            File.AppendAllText(filePath, string.Join(Environment.NewLine, dataToAdd));
            File.AppendAllText(filePath, Environment.NewLine);
            File.AppendAllText(filePath, Environment.NewLine);

            return true;
        }
    }
}
