namespace HelixBackupFileCore.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class FileExistsException : HelixBackupFileException
    {
        public FileExistsException(string filePath) 
            : base($"File doesn't exist: ({filePath})")
        {
        }

        protected FileExistsException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
