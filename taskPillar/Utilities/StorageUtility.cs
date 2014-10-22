using System;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;

namespace PillarAPI.Utilities
{
    public static class StorageUtility
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool IsWritable()
        {
            try
            {
                //Connect
                var root = new DirectoryInfo(Pillar.GlobalPillarApiSettings.DRIVES4STORAGE);
                // Create temp file
                var tmpDir = new FileInfo(Path.GetTempFileName());
                DirectoryInfo subdirectory = root.CreateSubdirectory(tmpDir.Name);
                // Delete temp file
                subdirectory.Delete();
                return true;
            }
            catch (Exception e)
            {
                Log.Fatal(e);
            }
            return false;
        }

        public static bool IsAccesable()
        {
            try
            {
                //Connect
                var root = new DirectoryInfo(Pillar.GlobalPillarApiSettings.DRIVES4STORAGE);
                // return any dirs
                return root.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly).Any();
            }
            catch (Exception e)
            {
                Log.Fatal(e);
            }
            return false;
        }

        public static long FileCount()
        {
            try
            {
                //Connect
                var root = new DirectoryInfo(Pillar.GlobalPillarApiSettings.DRIVES4STORAGE);
                // return nomber of files
                return root.EnumerateFiles("*", SearchOption.AllDirectories).AsParallel().LongCount();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return -1;
        }

        public static long DirCount()
        {
            try
            {
                //Connect
                var root = new DirectoryInfo(Pillar.GlobalPillarApiSettings.DRIVES4STORAGE);
                // return nomber of Dirs
                return root.EnumerateDirectories("*", SearchOption.AllDirectories).AsParallel().LongCount();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return -1;
        }
    }
}