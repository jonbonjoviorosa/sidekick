using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class LegalDocument
    {
        [Key]
        public int Id { get; set; }
        public string Body { get; set; }
        public ELegalDocType DocType { get; set; }
        public Guid? LastEditedBy { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{yyyy-MM-ddTHH\\:mm\\:ss}", ApplyFormatInEditMode = true)]
        public DateTime? LastEditedDate { get; set; }
    }

    public class LegalDocumentDto
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public ELegalDocType DocType { get; set; }
    }
}
