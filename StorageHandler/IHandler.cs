using System.IO;

namespace StorageHandler
{
    public interface IHandler
    {
        #region Methods

        string Savefile(FileInfo aFileInfo, string newFileName);
        void RenameFile(FileInfo oldFile, string newName);
        string GetNewLocation();

        #endregion Methods

        #region Other

        //void MoveFile(FileInfo aFileInfo, Path );

        #endregion Other
    }
}