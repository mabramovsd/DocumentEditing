namespace DocumentEditing.Libs
{
    public static class TextDifference
    {
        public static List<string> GetChangesList(string textOld, string textNew)
        {
            // Split by lines
            string[] oldLines = textOld.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            string[] newLines = textNew.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            int oldIndex = 0, newIndex = 0;
            var changes = new List<string>();

            while (oldIndex < oldLines.Length && newIndex < newLines.Length)
            {
                if (oldLines[oldIndex] == newLines[newIndex])
                {
                    oldIndex++;
                    newIndex++;
                }
                else
                {
                    // Looks like new line
                    if (newIndex + 1 < newLines.Length && oldLines[oldIndex] == newLines[newIndex + 1])
                    {
                        changes.Add($"PositionOld: '{oldIndex}', PositionNew: '{newIndex}', Added: '{newLines[newIndex]}'");
                        newIndex++;
                    }
                    //Looks like line was removed
                    else if (oldIndex + 1 < oldLines.Length && oldLines[oldIndex + 1] == newLines[newIndex])
                    {
                        changes.Add($"PositionOld: '{oldIndex}', PositionNew: '{newIndex}', Deleted: '{oldLines[oldIndex]}'");
                        oldIndex++;
                    }
                    //Line was changed
                    else
                    {
                        changes.Add($"PositionOld: '{oldIndex}', PositionNew: '{newIndex}', Changed: '{oldLines[oldIndex]}' -> '{newLines[newIndex]}'");
                        oldIndex++;
                        newIndex++;
                    }
                }
            }

            // Adding/removing from the end
            while (newIndex < newLines.Length)
            {
                changes.Add($"PositionOld: '{oldIndex}', PositionNew: '{newIndex}', Added: '{newLines[newIndex]}'");
                newIndex++;
            }
            while (oldIndex < oldLines.Length)
            {
                changes.Add($"PositionOld: '{oldIndex}', PositionNew: '{newIndex}', Deleted: '{oldLines[oldIndex]}'");
                oldIndex++;
            }

            return changes;
        }
    }
}
