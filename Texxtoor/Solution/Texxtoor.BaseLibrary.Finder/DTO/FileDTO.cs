using System.Runtime.Serialization;

namespace Texxtoor.BusinessLayer.Finder.DTO
{
    [DataContract]
    internal class FileDTO : DTOBase
    {          
        /// <summary>
        ///  Hash of parent directory. Required except roots dirs.
        /// </summary>
        [DataMember(Name = "phash")]
        public string ParentHash { get; set; }

        /// <summary>
        /// In case of an image the id of the original image in database that os the basis for the thumbnail
        /// </summary>
        [DataMember(Name = "tmb")]
        public int ThumbnailId { get; set; }
    }
}