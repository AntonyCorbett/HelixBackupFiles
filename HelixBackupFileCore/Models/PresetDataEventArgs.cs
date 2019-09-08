namespace HelixBackupFileCore.Models
{
    using System;

    public class PresetDataEventArgs : EventArgs
    {
        public string ParentSetListName { get; set; }

        public string PresetName { get; set; }

        public string PresetData { get; set; }

        public string SuggestedFileName { get; set; }
    }
}
