namespace HelixBackupFileCore.Models
{
    using System.Collections.Generic;

    public class FileFooter
    {
        public List<FileFooterRecord> Records { get; } = new List<FileFooterRecord>();
    }
}
