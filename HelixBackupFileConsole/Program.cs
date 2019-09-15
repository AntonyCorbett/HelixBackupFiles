namespace HelixBackupFileConsole
{
    using System;
    using System.IO;
    using HelixBackupFileCore;
    using HelixBackupFileCore.Models;
    
    // Sample console application that uses the "HelixBackupFileCore" assembly.
    // Here we just extract the various parts of the backup file and write
    // the corresponding files (IRs and set list files) to disk.
    internal static class Program
    {
        private static readonly string OutputFolder = "Output";
        private static readonly string SetListsFolder = "SetLists";
        private static readonly string ImpulseResponsesFolder = "ImpulseResponses";
        private static readonly string PresetsFolder = "Presets";
        private static readonly string MiscFolder = "Miscellaneous";

        private static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Specify the path to your hxb file");
                Environment.Exit(1);
            }

            var parser = new Parser(args[0]);

            parser.ParsedSectionEvent += HandleParsedSectionEvent;
            parser.ParsedPresetEvent += HandleParsedPresetEvent;

            parser.Execute();
        }

        private static void HandleParsedPresetEvent(object sender, PresetDataEventArgs e)
        {
            if (string.IsNullOrEmpty(e.ParentSetListName))
            {
                throw new ArgumentException(nameof(e.ParentSetListName));
            }

            if (string.IsNullOrEmpty(e.PresetName))
            {
                throw new ArgumentException(nameof(e.PresetName));
            }

            if (string.IsNullOrEmpty(e.PresetData))
            {
                throw new ArgumentException(nameof(e.PresetData));
            }

            var presetsFolder = Path.Combine(OutputFolder, SetListsFolder, e.ParentSetListName, PresetsFolder);
            Directory.CreateDirectory(presetsFolder);

            if (!Directory.Exists(presetsFolder))
            {
                throw new Exception($"Cannot create folder: {presetsFolder}");
            }

            var filePath = Path.Combine(presetsFolder, e.SuggestedFileName);
            File.WriteAllText(filePath, e.PresetData);
        }
        
        private static void HandleParsedSectionEvent(object sender, SectionDataEventArgs e)
        {
            Console.WriteLine($"Parsing section: {e.SectionDescription}");

            switch (e.SectionType)
            {
                case SectionType.ImpulseResponse:
                {
                    var impulseResponsesFolder = Path.Combine(OutputFolder, ImpulseResponsesFolder);
                    Directory.CreateDirectory(impulseResponsesFolder);
                    var filePath = Path.Combine(impulseResponsesFolder, e.SuggestedFileName);
                    File.WriteAllBytes(filePath, e.SectionData);
                    break;
                }

                case SectionType.GlobalSettings:
                {
                    Directory.CreateDirectory(OutputFolder);
                    var filePath = Path.Combine(OutputFolder, e.SuggestedFileName);
                    File.WriteAllBytes(filePath, e.SectionData);
                    break;
                }

                case SectionType.SetList:
                {
                    var setListsFolder = Path.Combine(OutputFolder, SetListsFolder);
                    Directory.CreateDirectory(setListsFolder);
                    var filePath = Path.Combine(setListsFolder, e.SuggestedFileName);
                    File.WriteAllBytes(filePath, e.SectionData);
                    break;
                }

                default:
                {
                    var miscFolder = Path.Combine(OutputFolder, MiscFolder);
                    Directory.CreateDirectory(miscFolder);
                    var filePath = Path.Combine(miscFolder, e.SuggestedFileName);
                    File.WriteAllBytes(filePath, e.SectionData);
                    break;
                }
            }
        }
    }
}
