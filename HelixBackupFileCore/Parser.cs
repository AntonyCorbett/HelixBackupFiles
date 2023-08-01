namespace HelixBackupFileCore
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using HelixBackupFileCore.Exceptions;
    using HelixBackupFileCore.Models;
    using Newtonsoft.Json;

    public sealed class Parser
    {
        private const string HelixDiCode = "IDXH";
        private const string DescriptionCode = "CSED";
        private const string SetListNamesCode = "MNLS";
        private const string GlobalSettingsCode = "BOLG";

        private readonly string _backupFilePath;

        private int _anonymousPresetCounter = 1;
        
        public Parser(string backupFilePath)
        {
            _backupFilePath = backupFilePath;
        }

        public event EventHandler<SectionDataEventArgs> ParsedSectionEvent;

        public event EventHandler<PresetDataEventArgs> ParsedPresetEvent;

        public bool ParseSections { get; set; } = true;

        public bool ParsePresets { get; set; } = true;

        public FileHeader FileHeader { get; private set; }

        public FileFooter FileFooter { get; private set; }
        
        public void Execute()
        {
            CheckFileExists();

            var fileData = File
                .ReadAllBytes(_backupFilePath)
                .ToArray();
            
            ReadHeaderAndFooter(fileData);

            InflateCompressedData(fileData);
        }

        private void InflateCompressedData(byte[] fileData)
        {
            if (!ParseSections && !ParsePresets)
            {
                return;
            }

            foreach (var record in FileFooter.Records)
            {
                if (record.IsSectionDeflated)
                {
                    var (data, _) = InflateService.Inflate(fileData.Skip(record.StartSectionOffset)
                        .Take(record.SectionLength).ToArray());

                    var description = GetSectionDescription(record, data);

                    OnParsedSectionEvent(new SectionDataEventArgs
                    {
                        SectionData = data,
                        SectionTitle = record.Title,
                        SectionDescription = description,
                        SuggestedFileName = GetSectionSuggestedFileName(description, record.SectionType),
                        SectionType = record.SectionType,
                    });

                    if (record.SectionType == SectionType.SetList)
                    {
                        ParseSetList(data);
                    }
                }
            }
        }

        private string GetSectionSuggestedFileName(string description, SectionType recordSectionType)
        {
            switch (recordSectionType)
            {
                case SectionType.BackupDescription:
                    return $"{FileNamingService.CoerceValidFileName(description)}.txt";

                case SectionType.GlobalSettings:
                    return $"{FileNamingService.CoerceValidFileName(description)}.json";

                case SectionType.HelixDi:
                    return $"{FileNamingService.CoerceValidFileName(description)}.dat";

                case SectionType.ImpulseResponse:
                    return $"{FileNamingService.CoerceValidFileName(description)}.wav";

                case SectionType.SetListNames:
                    return $"{FileNamingService.CoerceValidFileName(description)}.dat";

                case SectionType.SetList:
                    return $"{FileNamingService.CoerceValidFileName(description)}.json";

                default:
                    return "unknown.dat";
            }
        }

        private void ParseSetList(byte[] setListJson)
        {
            if (!ParsePresets)
            {
                return;
            }

            var enc = new UTF8Encoding();
            dynamic? json = JsonConvert.DeserializeObject(enc.GetString(setListJson));

            if (json != null)
            {
                var setListName = json.data.meta.name;

                var presets = json.data.presets;

                foreach (var preset in presets)
                {
                    var encodedData = preset.encoded_data;
                    var isEncoded = encodedData != null;

                    string presetName;

                    if (!isEncoded)
                    {
                        if (preset.meta == null)
                        {
                            // An empty preset.
                            continue;
                        }

                        presetName = preset.meta.name;

                        if (string.IsNullOrEmpty(presetName))
                        {
                            // Unusual! It may represent corrupt preset data
                            // or simply extraneous data that should be ignored...
                            continue;
                        }
                    }
                    else
                    {
                        // when encrypted (a premium preset), we can't decrypt to get the 
                        // preset name so fabricate a name...
                        presetName = FabricatePresetName();
                    }
                    
                    OnParsedPresetEvent(new PresetDataEventArgs
                    {
                        ParentSetListName = setListName,
                        PresetName = presetName,
                        SuggestedFileName = $"{FileNamingService.CoerceValidFileName(presetName)}.hlx",
                        PresetData = preset.ToString(),
                    });
                }
            }
        }

        private string FabricatePresetName()
        {
            return $"ANON {_anonymousPresetCounter++:D3}";
        }

        private string GetSectionDescription(FileFooterRecord record, byte[] data)
        {
            if (record.SectionType == SectionType.SetList)
            {
                int? index = ExtractSetListIndex(record.Title);
                if (index == null)
                {
                    return record.Description;
                }

                return GetSetListName(index.Value) ?? record.Description;
            }

            if (record.SectionType == SectionType.ImpulseResponse)
            {
                return GetWavName(data) ?? record.Description;
            }

            return record.Description;
        }

        private string GetWavName(byte[] data)
        {
            // this may not be a reliable way of getting the 
            // name of the IR!
            if (data.Length > 2 && data.Last() == 0)
            {
                var index = data.Length - 2;
                while (data[index] != 0 && index != 0)
                {
                    --index;
                }

                var enc = new UTF8Encoding();
                return enc.GetString(data, index + 1, data.Length - index - 2);
            }

            return null;
        }


        private int? ExtractSetListIndex(string recordTitle)
        {
            if (string.IsNullOrWhiteSpace(recordTitle))
            {
                return null;
            }
            
            return (int)char.GetNumericValue(recordTitle[0]);
        }

        private string GetSetListName(int index)
        {
            if (FileHeader.SetListNames.Count > index)
            {
                return FileHeader.SetListNames[index];
            }

            return null;
        }

        private void ReadHeaderAndFooter(byte[] fileData)
        {
            FileHeader = ReadFileHeader(fileData);
            FileFooter = ReadFileFooter(fileData, FileHeader.FooterOffset);

            var enc = new UTF8Encoding();

            // backup description...
            var backupDescriptionRecord = FileFooter.Records.SingleOrDefault(x => x.Title == DescriptionCode);
            if (backupDescriptionRecord != null)
            {
                FileHeader.BackupDescription = enc.GetString(
                    fileData, 
                    backupDescriptionRecord.StartSectionOffset,
                    backupDescriptionRecord.SectionLength);
            }

            // names of set lists...
            var setListNamesRecord = FileFooter.Records.SingleOrDefault(x => x.Title == SetListNamesCode);
            if (setListNamesRecord != null)
            {
                var setListNames = enc.GetString(
                    fileData,
                    setListNamesRecord.StartSectionOffset,
                    setListNamesRecord.SectionLength);

                var tokens = setListNames.Split('\0');

                foreach (var token in tokens)
                {
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        FileHeader.SetListNames.Add(token);
                    }
                }
            }
        }

        private void CheckFileExists()
        {
            if (string.IsNullOrWhiteSpace(_backupFilePath))
            {
                throw new MissingFileException();
            }

            if (!File.Exists(_backupFilePath))
            {
                throw new FileExistsException(_backupFilePath);
            }
        }

        private FileFooter ReadFileFooter(byte[] fileData, int fileHeaderFooterOffset)
        {
            const int FooterRecordByteLength = 36;
            const int NumSetLists = 8;
            const int NumOtherRecords = 4;

            // note there will be other footer records if IRs are used.
            const int MinimumFooterLength = (NumSetLists + NumOtherRecords) * FooterRecordByteLength;

            if (fileHeaderFooterOffset > fileData.Length - MinimumFooterLength)
            {
                throw new FileFooterSmallException();
            }

            var recordCount = (fileData.Length - fileHeaderFooterOffset) / FooterRecordByteLength;
            if (fileHeaderFooterOffset + (recordCount * FooterRecordByteLength) != fileData.Length)
            {
                throw new FileFooterBadException();
            }

            var result = new FileFooter();

            var enc = new UTF8Encoding();

            for (int n = 0; n < recordCount; ++n)
            {
                var startOffset = fileHeaderFooterOffset + (n * FooterRecordByteLength);

                var title = enc.GetString(fileData, startOffset, 4);
                var sectionType = GetSectionType(title);

                result.Records.Add(new FileFooterRecord
                {
                    Title = title,
                    SectionType = sectionType,
                    Description = GetFooterRecordDescription(sectionType),
                    StartSectionOffset = BitConverter.ToInt32(fileData, startOffset + 4),
                    SectionLength = BitConverter.ToInt32(fileData, startOffset + 12),
                    IsSectionDeflated = BitConverter.ToInt32(fileData, startOffset + 20) == 1,
                    InflatedLength = BitConverter.ToInt32(fileData, startOffset + 24),
                });
            }

            return result;
        }

        private SectionType GetSectionType(string title)
        {
            if (title == HelixDiCode)
            {
                return SectionType.HelixDi;
            }

            if (title == DescriptionCode)
            {
                return SectionType.BackupDescription;
            }

            if (title == SetListNamesCode)
            {
                return SectionType.SetListNames;
            }

            if (title == GlobalSettingsCode)
            {
                return SectionType.GlobalSettings;
            }

            if (title.EndsWith("I"))
            {
                return SectionType.ImpulseResponse;
            }

            if (title.EndsWith("LS"))
            {
                return SectionType.SetList;
            }

            return SectionType.Unknown;
        }

        private string GetFooterRecordDescription(SectionType sectionType)
        {
            switch (sectionType)
            {
                default:
                    return "Unknown";

                case SectionType.ImpulseResponse:
                    return "Impulse Response";

                case SectionType.SetList:
                    return "Set List";

                case SectionType.BackupDescription:
                    return "Backup Description";

                case SectionType.GlobalSettings:
                    return "Global Settings";

                case SectionType.HelixDi:
                    return "Helix Di";

                case SectionType.SetListNames:
                    return "Set List Names";
            }
        }

        private FileHeader ReadFileHeader(byte[] fileData)
        {
            var enc = new UTF8Encoding();
            return new FileHeader
            {
                Signature = enc.GetString(fileData, 0, 4),
                FooterOffset = BitConverter.ToInt32(fileData, 0x08),
            };
        }

        private void OnParsedSectionEvent(SectionDataEventArgs e)
        {
            ParsedSectionEvent?.Invoke(this, e);
        }

        private void OnParsedPresetEvent(PresetDataEventArgs e)
        {
            ParsedPresetEvent?.Invoke(this, e);
        }
    }
}
