namespace HelixBackupFileConsole
{
    using System;
    using System.IO;
    using HelixBackupFileCore;
    using HelixBackupFileCore.Models;
    
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser(@"D:\ProjectsPersonal\HelixPatches\SamplePatches\Backup1.hxb");

            parser.ParsedSectionEvent += HandleParsedSectionEvent;
            parser.Execute();
        }

        private static void HandleParsedSectionEvent(object sender, HelixBackupFileCore.Models.SectionDataEventArgs e)
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
