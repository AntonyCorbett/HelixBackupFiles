namespace HelixBackupFileCore.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class FileFooterSmallException : HelixBackupFileException
    {
        public FileFooterSmallException() 
            : base("File footer is too small")
        {
        }

        protected FileFooterSmallException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
