using System.IO;
using System;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.BaseLibrary.Finder
{
    public class FullPath
    {
        public Root Root
        {
            get { return _root; }
        }
        public bool IsDirectory
        {
            get { return _isDirectory; }
        }
        public string RelativePath
        {
            get
            {
                return _relativePath;
            }
        }
        public ResourceFolder Directory
        {
            get
            {
              return _isDirectory ? (ResourceFolder)_fileSystemObject : null;
            }
        }
        public ResourceFile File
        {
            get
            {
              return !_isDirectory ? (ResourceFile)_fileSystemObject : null;
            }
        }
        public FullPath(Root root, Resource fileSystemObject)
        {
            if (root == null)
                throw new ArgumentNullException("root", "Root can not be null");
            if (fileSystemObject == null)
                throw new ArgumentNullException("root", "Filesystem object can not be null");
            _root = root;
            _fileSystemObject = fileSystemObject;
            _isDirectory = _fileSystemObject is ResourceFolder;
            if (fileSystemObject.FullName.StartsWith(root.Directory.FullName))
            {
                if (fileSystemObject.FullName.Length == root.Directory.FullName.Length)
                {
                    _relativePath = string.Empty;
                }
                else
                {
                    _relativePath = fileSystemObject.FullName.Substring(root.Directory.FullName.Length + 1);
                }
            }
            else
                throw new InvalidOperationException("Filesystem object must be in it root directory or in root subdirectory");

        }


        private Root _root;
        private Resource _fileSystemObject;
        private bool _isDirectory;
        private string _relativePath;

        
        

    }
}