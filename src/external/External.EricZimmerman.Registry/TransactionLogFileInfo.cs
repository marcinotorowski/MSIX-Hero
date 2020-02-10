namespace External.EricZimmerman.Registry
{
    public class TransactionLogFileInfo
    {
        public TransactionLogFileInfo(string fileName, byte[] fileBytes)
        {
            this.FileName = fileName;
            this.FileBytes = fileBytes;
        }

        public string FileName { get; }
        public byte[] FileBytes { get; }


    }
}
