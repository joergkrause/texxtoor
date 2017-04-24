using System.Runtime.Serialization;
using Texxtoor.BusinessLayer.Finder.DTO;

namespace Texxtoor.BusinessLayer.Finder.Response
{
    [DataContract]
    internal class OpenResponse : OpenResponseBase
    {
        public OpenResponse(DTOBase currentWorkingDirectory, FullPath fullPath)
            : base(currentWorkingDirectory)
        {
            Options = new Options(fullPath);
            _files.Add(currentWorkingDirectory);
        }
    }
}