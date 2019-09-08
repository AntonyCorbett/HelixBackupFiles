using System.ComponentModel;

namespace HelixBackupFileCore.Models
{
    using System;

    public class SectionDataEventArgs : EventArgs
    {
        public string SectionTitle { get; set; }

        public string SectionDescription { get; set; }

        public byte[] SectionData { get; set; }

        public SectionType SectionType { get; set; }

        public string SuggestedFileName { get; set; }
    }
}
