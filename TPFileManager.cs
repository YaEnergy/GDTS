using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD_Texture_Swapper
{
    public static class TPFileManager
    {
        public static long GetDirectorySize(DirectoryInfo d)
        {
            long size = 0;
            FileInfo[] files = d.GetFiles();
            foreach (FileInfo file in files)
                size += file.Length;

            DirectoryInfo[] ds = d.GetDirectories();
            foreach (DirectoryInfo dir in ds)
                size += GetDirectorySize(dir);

            return size;
        }
        public static bool HasEnoughAvailableSpace(string? driveLetter, long spaceRequired)
        {
            if (driveLetter == null)
                return false;

            DriveInfo[] driveInfo = DriveInfo.GetDrives();
            foreach (DriveInfo drive in driveInfo)
                if (driveInfo[0].Name == driveLetter)
                    return drive.AvailableFreeSpace > spaceRequired;

            return false;
        }
        public static Exception? OverwriteFile(string filePathToOverwrite, string overwritingFilePath)
        {
            try
            {
                FileStream fs = new(overwritingFilePath, FileMode.Open, FileAccess.ReadWrite);

                FileStream resource_fs = new(filePathToOverwrite, FileMode.Open, FileAccess.ReadWrite);

                fs.CopyTo(resource_fs);

                fs.Close();
                resource_fs.Close();

                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}
