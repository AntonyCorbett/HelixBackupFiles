namespace HelixBackupFileCore
{
    using System.IO;
    using ICSharpCode.SharpZipLib.Zip.Compression;

    internal static class InflateService
    {
        public static (byte[] Data, long InputLength) Inflate(byte[] inputData)
        {
            var inflater = new Inflater();

            using (var inputStream = new MemoryStream(inputData))
            using (var ms = new MemoryStream())
            {
                var inputBuffer = new byte[4096];
                var outputBuffer = new byte[4096];

                while (inputStream.Position < inputData.Length)
                {
                    var read = inputStream.Read(inputBuffer, 0, inputBuffer.Length);

                    inflater.SetInput(inputBuffer, 0, read);

                    while (!inflater.IsNeedingInput)
                    {
                        var written = inflater.Inflate(outputBuffer, 0, outputBuffer.Length);
                        if (written == 0)
                        {
                            break;
                        }

                        ms.Write(outputBuffer, 0, written);
                    }

                    if (inflater.IsFinished)
                    {
                        break;
                    }
                }

                var inputLength = inflater.TotalIn;

                inflater.Reset();

                return (ms.ToArray(), inputLength);
            }
        }
    }
}
