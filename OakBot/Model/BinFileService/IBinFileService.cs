namespace OakBot.Model
{
    public interface IBinFileService
    {
        bool WriteBinFile(string filename, object serializable);
        object ReadBinFile(string filename);
    }
}
