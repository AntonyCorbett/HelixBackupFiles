namespace HelixBackupFileCore.Models
{
    using System.Collections.Generic;

    public class FileHeader
    {
        public string Signature { get; set; }

        public int FooterOffset { get; set; }

        public string BackupDescription { get; set; }

        public List<string> SetListNames { get; } = new List<string>();
    }
}
