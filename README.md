# HelixBackupFiles
Parsing Line6 Helix Backup Files

The project parses a Line 6 Helix Backup file (*.hxb).

I am using firmware 2.81 but I expect the format of the hxb file hasn't change much in recent times.

The file starts with a non-compressed header with a proprietary file signature "AF6L" which I'm guessing may stand for for "Line6 Full Archive" (little-endian byte ordering). The header also contains some text including the backup description and names of the set lists and is immediately followed by the first of several zlib sections. The first section contains the global settings. This is followed by a section for each of the IRs and a section for each of the 8 setlists. The sections use the standard zlib Deflate algorithm, and when inflated the global settings and setlist sections are in json, and the IRs are in wav (with "RIFF" signature at the head and embedded metadata for IR name, etc).

Finally there's a non-compressed "footer" section at the end of the file (straight after the last zlib section) that describes the various parts of the archive and provides offsets into the file and the lengths of each section. This info can be used if you want to inflate a particular section rather than process the whole file. The footer contains several sections of 36 bytes. The first 4 bytes of each section describe the data. The sections are "IDXH" (stands for HeliX ??), "CSED" (which stands for DESCription), "MNLS" (Set List NaMes), "BOLG" (GLOBal settings). Then there are sections for each of your IRs ("000I", "100I", "200I", etc, where "I" denotes IR), then a section for each of the 8 setlists - "10LS", "20LS", etc. After the initial 4 bytes of each section there are 8 x 32 bit integers (little-endian). The 1st is the starting offset of the section, the 3rd is the length of the data, the 5th indicates if the data is deflated (1) or not (0), and the 6th is the size of the data once inflated. Note that the address of the footer can be found at offset 0x08 in the file (32-bit unsigned).

I created this in order to extract and analyse preset data. You must be very careful if you decide to modify data and write back to the hxb file! Use at your own risk.
