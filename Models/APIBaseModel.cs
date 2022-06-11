using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Sidekick.Model
{
    public class APIBaseModel
    {
        public APIBaseModel()
        {
            IsEnabled = true;
            IsLocked = false;
            LastEditedDate = DateTime.Now;
            DateEnabled = DateTime.Now;
        }

        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        public Guid? LastEditedBy { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? LastEditedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CreatedDate { get; set; }
        public bool? IsEnabled { get; set; }
        public Guid? IsEnabledBy { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateEnabled { get; set; }
        public bool? IsLocked { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? LockedDateTime { get; set; }

        public string HashP(string access, string key)
        {
            string retStr= Convert.ToBase64String(KeyDerivation.Pbkdf2(
                                password: access.ToString(),
                                salt: Encoding.UTF8.GetBytes(key),
                                prf: KeyDerivationPrf.HMACSHA1,
                                iterationCount: 10000,
                                numBytesRequested: 256 / 8
                            ));
            return retStr;
        }

    }
}
