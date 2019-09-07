namespace HelixBackupFileCore.Models
{
    public class FileFooterRecord
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public SectionType SectionType { get; set; }

        public bool IsSectionDeflated { get; set; }

        public int StartSectionOffset { get; set; }

        public int SectionLength { get; set; }

        public int InflatedLength { get; set; }
    }
}
