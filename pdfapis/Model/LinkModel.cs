using System.Runtime.Serialization;

namespace pdfapis.Model
{
    [DataContract]
    public class LinkModel
    {
        [DataMember]
        public string? link { get; set; }
    }
}
