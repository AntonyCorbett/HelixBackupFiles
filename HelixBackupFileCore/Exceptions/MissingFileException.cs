namespace HelixBackupFileCore.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class MissingFileException : HelixBackupFileException
    {
        public MissingFileException() 
            : base("Helix backup file not specified!")
        {
        }

        protected MissingFileException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
