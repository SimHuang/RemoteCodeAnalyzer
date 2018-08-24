using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteCodeAnalyzer.File
{
    /*
     * This class contains method:
     * grantFilePermission() - allow user to access files within a directory
     * retrieveFilesInDirectory() - return all file names for given directory
     */
    public class FileData
    {
        public void grantFilePermission() {
        }

        /*
         * Return all file names for the given directory
         */
        public ArrayList retrieveFilesInDirectory(string directory)
        {
            ArrayList fileNames = new ArrayList();

            fileNames.Add("foos.txt");
            fileNames.Add("rules.cs");
            fileNames.Add("sample_book.txt");

            return fileNames;
        }
    }
}
