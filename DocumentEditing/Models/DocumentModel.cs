namespace DocumentEditing.Models
{
    public class DocumentModel
    {
        public bool IsReadOnly { get; set; }
        public string FileName { get; set; }
        public string Content { get; set; }
        public List<string> ContentByLines { get; set; }
        public static DocumentModel FillDataFromFile(string dir, string fileName)
        {
            var model = new DocumentModel();
            model.FileName = fileName;
            model.Content = File.ReadAllText(dir + "\\" + fileName);
            model.ContentByLines = File.ReadAllLines(dir + "\\" + fileName).ToList();
            return model;
        }
    }
}
