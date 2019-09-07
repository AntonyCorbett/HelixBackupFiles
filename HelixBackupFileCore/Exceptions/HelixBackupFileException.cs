namespace HelixBackupFileCore.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class HelixBackupFileException : Exception
    {
        public HelixBackupFileException(string msg)
            : base(msg)
        {
        }
        
        protected HelixBackupFileException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
