using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Henooh.Framework.Utility
{
    /// <summary>
    /// Provides set of file system related utility classes that is designed to check for all conditions that could
    /// cause an exception and return an error.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <visibility>public</visibility>
    /// <revisionhistory>
    /// YYYY-MM-DD  AS#####  v#.##.##.###  Change Description
    /// ==========  =======  ============  ============================================================================
    /// 2016-10-14  AS00749  v1.00.00.015  Initial Version
    /// 2016-11-14  AS00776  v1.00.00.028  Added ConvertHtmlToPlainText method
    /// 2016-11-15  AS00777  v1.00.00.029  Renamed method to ConvertToText method, fixed revision history
    /// 2016-11-20  AS00781  v1.00.00.031  Modified ConvertToText method to remove blank spaces in the beginning
    /// 2017-01-31  AS00829  v1.00.00.043  Add XML comment to ConvertToText method, modify summary and remarks
    /// 2017-03-03  AS00844  v1.00.00.049  Remove unused using statements
    /// 2018-03-27  AS01035  v1.01.06.007  Rename HenoohWpfBase to Henooh.Framework.Wpf
    /// 2018-11-03  AS01098  v1.01.07.007  Implement GetBitmapFromBitmapSource method
    /// 2018-12-04  AS01121  v1.01.08.007  Change the access modifier for GetBitmapFromBitmapSource to public
    /// 2019-04-17  AS01178  v1.01.11.005  Resolve CA1822 by adding static prefix to methods that doe not use instance
    /// 2019-04-18  AS01179  v1.01.11.006  Resolve CA1052 Made the entire class static
    /// 2019-04-19  AS01180  v1.01.11.007  Resolve IDE0054 by using compound assignments on ConvertToText method
    /// 2019-08-06  AS01209  v1.01.13.004  Resolve CA1062 by validating parameter is non-null before using it
    /// 2019-08-08  AS01210  v1.01.13.005  Dispose the MemoryStream after use
    /// 2019-08-09  AS01211  v1.01.13.006  Check if BitmapSource is null at the beginning of GetBitmapFromBitmapSource
    /// 2019-09-24  AS01232  v1.01.15.008  Corrected the revision history
    /// 2019-12-12  AS01260  v1.01.18.008  Add CreateDirectory and GrantAccess methods
    /// 2020-01-28  AS01269  v1.02.00.000  Port from Henooh.Framework.Wpf to Henooh.Framework.Utility
    /// 2020-02-01  AS01272  v1.02.00.001  Enable system security by adding System.IO.FileSystem.AccessControl
    /// 2020-02-05  AS01274  v1.02.00.003  Change the class static
    /// 2020-02-28  AS01288  v1.02.00.006  Add Copy method with returns while catching exceptions
    /// 2020-03-13  AS01293  v1.02.00.007  Add XML comments in summary and remarks
    /// 2020-04-28  AS01299  v1.02.01.001  Fix the logic in Copy files and grant access to files
    /// 2020-06-11  AS01300  v1.02.01.002  Add XML comments to all public methods
    /// </revisionhistory>
    public static class FileSystem
    {

        /// <summary>
        /// Provides a way to check if a directory exists, and create one if it does not exist.
        /// It is designed to handle all exceptions.
        /// </summary>
        /// <param name="aDirectoryPath"></param>
        /// <param name="aIsPublic"></param>
        /// <returns>
        /// Returns 0 if the directory already exist, and did not need to create one.
        /// Returns 1 if the directory is created, and when permissions was properly applied.
        /// Returns 2 if the directory is created, but permissions was not applied.
        /// Returns -1 if the directory was unable to be created.
        /// </returns>
        public static int CreateDirectory(string aDirectoryPath, bool aIsPublic = true)
        {
            int status = 0;

            // If a directory does not exist, create it.
            if (!Directory.Exists(aDirectoryPath))
            {
                try
                {
                    Directory.CreateDirectory(aDirectoryPath);
                    status = 1;
                }
                catch (PathTooLongException e)
                {
                    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                        "Unable to create a directory due to a PathTooLongException."));
                    Trace.WriteLine(e);
                    status = -1;
                }
                catch (DirectoryNotFoundException e)
                {
                    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                        "Unable to create a directory due to a DirectoryNotFoundException."));
                    Trace.WriteLine(e);
                    status = -1;
                }
                catch (UnauthorizedAccessException e)
                {
                    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                        "Unable to create a directory {0} due to an UnauthorizedAccesException.", aDirectoryPath));
                    Trace.WriteLine(e);
                    status = -1;
                }
                catch (ArgumentNullException e)
                {
                    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                        "Unale to create a directory due to an ArgumentNullException."));
                    Trace.WriteLine(e);
                    status = -1;
                }
                catch (NotSupportedException e)
                {
                    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                        "Unable to create a directory due to a NotSupportedException."));
                    Trace.WriteLine(e);
                    status = -1;
                }
                catch (ArgumentException e)
                {
                    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                        "Unable to create a directory due to an ArgumentException."));
                    Trace.WriteLine(e);
                    status = -1;
                }
                catch (IOException e)
                {
                    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                        "Unable to create a directory {0} due to an IOException.", aDirectoryPath));
                    Trace.WriteLine(e);
                    status = -1;
                }

                if (aIsPublic)
                {
                    if (!GrantAccess(aDirectoryPath))
                    {
                        status = 2;
                    }
                }
            }

            return status;
        }

        /// <summary>
        /// Copies an existing file to a new file.
        /// </summary>
        /// <param name="aSourceFileName"></param>
        /// <param name="aDestinationFileName"></param>
        /// <param name="aOverwriteFlag"></param>
        /// <returns>
        /// Returns 0 if the file does not exist.
        /// Returns 1 if the file does exist.
        /// Returns -1 if the file was unable to be copied.
        /// </returns>
        public static int Copy(string aSourceFileName, string aDestinationFileName, bool aOverwriteFlag = false)
        {
            int status = 0;

            // If a file does not exist, return fail.
            if (File.Exists(aSourceFileName))
            {
                try
                {
                    File.Copy(aSourceFileName, aDestinationFileName, aOverwriteFlag);
                    status = 1;
                }
                catch (FileNotFoundException e)
                {
                    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                        "Unable to copy a file due to a FileNotFoundException."));
                    Trace.WriteLine(e);
                    status = -1;
                }
                catch (PathTooLongException e)
                {
                    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                        "Unable to copy a file due to a PathTooLongException."));
                    Trace.WriteLine(e);
                    status = -1;
                }
                catch (DirectoryNotFoundException e)
                {
                    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                        "Unable to copy a file due to a DirectoryNotFoundException."));
                    Trace.WriteLine(e);
                    status = -1;
                }
                catch (UnauthorizedAccessException e)
                {
                    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                        "Unable to copy a file due to an UnauthorizedAccesException."));
                    Trace.WriteLine(e);
                    status = -1;
                }
                catch (ArgumentNullException e)
                {
                    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                        "Unale to copy a file due to an ArgumentNullException."));
                    Trace.WriteLine(e);
                    status = -1;
                }
                catch (NotSupportedException e)
                {
                    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                        "Unable to copy a file due to a NotSupportedException."));
                    Trace.WriteLine(e);
                    status = -1;
                }
                catch (ArgumentException e)
                {
                    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                        "Unable to copy a file due to an ArgumentException."));
                    Trace.WriteLine(e);
                    status = -1;
                }
                catch (IOException e)
                {
                    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                        "Unable to copy a file due to an IOException."));
                    Trace.WriteLine(e);
                    status = -1;
                }
            }

            return status;
        }

        /// <summary>
        /// Grants full permission to a file or a directory.
        /// </summary>
        /// <param name="aPath"></param>
        /// <returns></returns>
        public static bool GrantAccess(string aPath)
        {
            if (IsDirectory(aPath))
            {
                GrantAccessToDirectory(aPath);
            }
            else
            {
                GrantAccessToFile(aPath);
            }

            return true;
        }

        /// <summary>
        /// Grants full permission to a directory.
        /// </summary>
        /// <param name="aPath"></param>
        /// <returns></returns>
        public static bool GrantAccessToDirectory(string aPath)
        {
            bool status = false;
            DirectoryInfo directoryInfo;

            try
            {
                directoryInfo = new DirectoryInfo(aPath);
            }
            catch (ArgumentNullException e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Unable to obtain DirectoryInfo {0} due to an ArgumentNullException", aPath));
                Trace.WriteLine(e);
                return status;
            }
            catch (PathTooLongException e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Unable to obtain DirectoryInfo {0} due to a PathTooLongException", aPath));
                Trace.WriteLine(e);
                return status;
            }
            catch (ArgumentException e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Unable to obtain DirectoryInfo {0} due to an ArgumentException", aPath));
                Trace.WriteLine(e);
                return status;
            }
            catch (SecurityException e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Unable to obtain DirectoryInfo {0} due to a SecurityException", aPath));
                Trace.WriteLine(e);
                return status;
            }

            DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();
            directorySecurity.AddAccessRule(new FileSystemAccessRule(
                new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                FileSystemRights.FullControl,
                InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                PropagationFlags.NoPropagateInherit,
                AccessControlType.Allow));

            directoryInfo.SetAccessControl(directorySecurity);

            return true;
        }

        /// <summary>
        /// Grants full permission to a file.
        /// </summary>
        /// <param name="aPath"></param>
        /// <returns></returns>
        public static bool GrantAccessToFile(string aPath)
        {
            bool status = false;
            FileInfo fileInfo;

            try
            {
                fileInfo = new FileInfo(aPath);
            }
            catch (ArgumentNullException e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Unable to obtain DirectoryInfo {0} due to an ArgumentNullException", aPath));
                Trace.WriteLine(e);
                return status;
            }
            catch (PathTooLongException e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Unable to obtain DirectoryInfo {0} due to a PathTooLongException", aPath));
                Trace.WriteLine(e);
                return status;
            }
            catch (ArgumentException e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Unable to obtain DirectoryInfo {0} due to an ArgumentException", aPath));
                Trace.WriteLine(e);
                return status;
            }
            catch (UnauthorizedAccessException e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Unable to create a directory {0} due to an UnauthorizedAccesException.", aPath));
                Trace.WriteLine(e);
                return false;
            }
            catch (NotSupportedException e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Unable to create a directory due to a NotSupportedException."));
                Trace.WriteLine(e);
                return false;
            }
            catch (SecurityException e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Unable to obtain DirectoryInfo {0} due to a SecurityException", aPath));
                Trace.WriteLine(e);
                return status;
            }

            _ = new FileSecurity(aPath, AccessControlSections.None |
                AccessControlSections.Group | AccessControlSections.Access);

            return true;
        }

        /// <summary>
        /// Detects if the given path is a directory.
        /// </summary>
        /// <param name="aPath"></param>
        /// <returns></returns>
        public static bool IsDirectory(string aPath)
        {
            FileAttributes attributes;

            try
            {
                attributes = File.GetAttributes(aPath);
            }
            catch (PathTooLongException e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Unable to get attributes to the path due to a PathTooLongException."));
                Trace.WriteLine(e);
                return false;
            }
            catch (DirectoryNotFoundException e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Unable to get attributes to the path due to a DirectoryNotFoundException."));
                Trace.WriteLine(e);
                return false;
            }
            catch (FileNotFoundException e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Unable to get attributes to the path due to a FileNotFoundException."));
                Trace.WriteLine(e);
                return false;
            }
            catch (UnauthorizedAccessException e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Unable to create a directory {0} due to an UnauthorizedAccesException.", aPath));
                Trace.WriteLine(e);
                return false;
            }
            catch (NotSupportedException e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Unable to create a directory due to a NotSupportedException."));
                Trace.WriteLine(e);
                return false;
            }
            catch (ArgumentException e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Unable to create a directory due to an ArgumentException."));
                Trace.WriteLine(e);
                return false;
            }
            catch (IOException e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Unable to create a directory {0} due to an IOException.", aPath));
                Trace.WriteLine(e);
                return false;
            }

            if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return true;
            }
            else
                return false;
        }
    }
}
