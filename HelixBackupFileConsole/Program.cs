namespace HelixBackupFileConsole
{
    using System;
    using System.IO;
    using HelixBackupFileCore;
    using HelixBackupFileCore.Models;
    
    // Sample console application that uses the "HelixBackupFileCore" assembly.
    // Here we just extract the various parts of the backup file and write
    // the corresponding files (IRs and set list files) to disk.
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Specify the path to your hxb file");
                Environment.Exit(1);
            }

            var parser = new Parser(args[0]);

            parser.ParsedSectionEvent += HandleParsedSectionEvent;
            parser.Execute();
        }

        private static void HandleParsedSectionEvent(object sender, SectionDataEventArgs e)
        {
            var fileName = GenerateFileName(e);
            Console.WriteLine($"Writing {fileName}");
            File.WriteAllBytes(fileName, e.SectionData);
        }

        private static string GenerateFileName(SectionDataEventArgs e)
        {
            switch (e.SectionType)
            {
                case SectionType.ImpulseResponse:
                    return $"{e.SectionDescription}.wav";

                case SectionType.SetList:
                case SectionType.GlobalSettings:
                    return $"{e.SectionDescription}.json";

                default:
                    return $"{e.SectionTitle}.txt";
            }
        }
    }
}
