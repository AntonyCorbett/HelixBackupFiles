namespace HelixBackupFileCore.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class FileFooterBadException : HelixBackupFileException
    {
        public FileFooterBadException() 
            : base("File footer bad!")
        {
        }

        protected FileFooterBadException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
