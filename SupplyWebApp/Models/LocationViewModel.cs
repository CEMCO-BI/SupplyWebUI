using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Models
{
    [Table("Location", Schema = "dbo")]
    public class Location
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LocationId { get; set; }
        //public string LocationName { get; set; }
        public string LocationCode { get; set; }
        //public ICollection<AddedFreight> AddedFreight { get; set; }


        //AddressName
        //Address1
        //Address2
        //City
        //StateId
        //Zip
        //CountryId
        //Phone1
        //Phone2
        //Fax
        //Active
        //GLAccount
        //CreatedBy
        //CreatedOn
        //ModifiedBy
        //ModifiedOn
        //CompanyInformationId
        //A_GpConnStr
        //A_DefaultNto
        //A_RemitAddressName
        //A_RemitAddress1
        //A_RemitAddress2
        //A_RemitCity
        //A_RemitState
        //A_RemitZip
        //A_RemitCountry
        //A_RemitPhone1
        //A_RemitPhone2
        //A_RemitFax
        //PrefixOrSuffixValue
        //ShippingMilesToTrack
        //TimeZoneId
        //DefaultJobEMail
        //DefaultJobEMailPassword
        //DefaultSalesOrderEMail
        //DefaultSalesOrderEMailPassword
        //DefaultInvoiceEMail
        //DefaultInvoiceEMailPassword

    }
}
