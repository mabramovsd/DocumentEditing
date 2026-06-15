namespace DocumentEditing.Libs
{
    public static class TextDifference
    {
        public static List<string> GetChangesList(string textOld, string textNew)
        {
            // Разбиваем тексты на строки
            string[] oldLines = textOld.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            string[] newLines = textNew.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            int i = 0, j = 0;
            var changes = new List<string>();

            while (i < oldLines.Length && j < newLines.Length)
            {
                if (oldLines[i] == newLines[j])
                {
                    // Строки совпадают, двигаем оба указателя
                    i++;
                    j++;
                }
                else
                {
                    // Строки различаются. Проверяем, что произошло.
                    // Это упрощенная логика. В реальном diff нужно смотреть вперед.
                    if (j + 1 < newLines.Length && oldLines[i] == newLines[j + 1])
                    {
                        // Похоже на добавление строки в newLines
                        changes.Add($"PositionOld: '{i}', PositionNew: '{j}', Added: '{newLines[j]}'");
                        j++; // Двигаем только указатель нового текста
                    }
                    else if (i + 1 < oldLines.Length && oldLines[i + 1] == newLines[j])
                    {
                        // Похоже на удаление строки из oldLines
                        changes.Add($"PositionOld: '{i}', PositionNew: '{j}', Deleted: '{oldLines[i]}'");
                        i++; // Двигаем только указатель старого текста
                    }
                    else
                    {
                        // Скорее всего, замена
                        changes.Add($"PositionOld: '{i}', PositionNew: '{j}', Changed: '{oldLines[i]}' -> '{newLines[j]}'");
                        i++;
                        j++;
                    }
                }
            }
            // Обрабатываем оставшиеся строки (добавления в конце или удаления в конце)
            while (j < newLines.Length)
            {
                changes.Add($"PositionOld: '{i}', PositionNew: '{j}', Added: '{newLines[j]}'");
                j++;
            }
            while (i < oldLines.Length)
            {
                changes.Add($"PositionOld: '{i}', PositionNew: '{j}', Deleted: '{oldLines[i]}'");
                i++;
            }

            return changes;
        }
    }
}
