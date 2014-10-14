namespace StorageHandler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Transactions;

    using ChinhDo.Transactions;

    using Retry;

    using StorageHandler.CustomExections;

    /// <summary>
    ///     This class uses ChinhDo to save files and create folders in a transaction -see link
    ///     http://www.chinhdo.com/20080825/transactional-file-manager/
    /// </summary>
    public class Handler : IHandler
    {
        #region Fields

        private readonly IFileManager _fm = new TxFileManager();

        private int _maxFilesPrFolder = 100000;
        private int _MaxRetryCount = 3;

        #endregion Fields

        #region Constructors

        public Handler(string drivesFromConfig)
        {
            DrivesFromConfig = drivesFromConfig;
        }

        #endregion Constructors

        #region Properties

        public string DrivesFromConfig
        {
            get; set;
        }

        public int MaxFilesPrFolder
        {
            get { return _maxFilesPrFolder; }
            set { _maxFilesPrFolder = value; }
        }

        public int MaxRetryCount
        {
            get { return _maxFilesPrFolder; }
            set { _maxFilesPrFolder = value; }
        }

        #endregion Properties

        #region Methods

            public void RenameFile(FileInfo oldFile, string newName)
            {
                using (var scope = new TransactionScope())
                {
                    File.Move(oldFile.FullName, newName);
                    scope.Complete();
                }                
            }

        public string Savefile(FileInfo aFileInfo, string newFileName)
        {
            string localPath =
                RetryHelper.Instance.Try(() => CalculatePath())
                           .WithTimeLimit(5000)
                           .WithMaxTryCount(_MaxRetryCount)
                           .UntilNoException();
            string newFile = Path.Combine(localPath, newFileName);
            using (var scope = new TransactionScope())
            {
                _fm.Copy(aFileInfo.FullName, newFile, false);
                scope.Complete();
            }
            return newFile;
        }

            public string GetNewLocation()
            {
                return CalculatePath();
            }

            private string CalculatePath()
        {
            string localPath = string.Empty;
            string folder = GetBaseFolder();
            string s = "Y" + DateTime.Now.ToString("yyyy") + "M" + DateTime.Now.ToString("MM");
            localPath = Path.Combine(folder, s);
            if (!_fm.DirectoryExists(localPath))
            {
                _fm.CreateDirectory(localPath);
            }

            return localPath;
        }

        private IEnumerable<string> GetAllFolders()
        {
            string[] drives = DrivesFromConfig.Split(";".ToCharArray());
            foreach (string drive in drives)
            {
                var dir = new DirectoryInfo(drive);
                foreach (DirectoryInfo firstLevelDir in dir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
                    foreach (
                        DirectoryInfo secondLevelDir in
                            firstLevelDir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
                        if (secondLevelDir.EnumerateFiles("*", SearchOption.TopDirectoryOnly).Count() <=
                            MaxFilesPrFolder)
                        {
                            yield return secondLevelDir.FullName;
                        }
            }
        }

        private string GetBaseFolder()
        {
            IEnumerable<string> folders = GetAllFolders();
            IEnumerable<string> f = folders;
            IEnumerable<string> enumerable = f as IList<string> ?? f.ToList();
            int maxItems = enumerable.Count() - 1;
            if (maxItems < 0)
            {
                throw new NoFreeFoldersFoundException();
            }
            var random = new Random();
            int randomNumber = random.Next(0, maxItems);
            return enumerable.ElementAt(randomNumber);
        }

        #endregion Methods

        
    }
}