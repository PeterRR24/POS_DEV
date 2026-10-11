using System;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

namespace RR.Commerce.Runtime.Printing
{
    public class StorePrinter : CommerceEntity
    {
        private const string IdColumn = "RECID";

        public StorePrinter() : base("PRStore")
        {
        }
        [SystemDataAnnotations.Key]

        [Key]
        [DataMember]
        [Column(IdColumn)]
        public long Id
        {
            get { return (long)this[IdColumn]; }
            set { this[IdColumn] = value; }
        }

        [DataMember]
        [Column("STORERECID")]
        public long StoreRecId
        {
            get { return (long)this["STORERECID"]; }
            set { this["STORERECID"] = value; }
        }

        [DataMember]
        [Column("WINDOWSPRINTERNAME1")]
        public string WindowsPrinterName1
        {
            get { return (string)this["WINDOWSPRINTERNAME1"]; }
            set { this["WINDOWSPRINTERNAME1"] = value; }
        }

        [DataMember]
        [Column("WINDOWSPRINTERNAME2")]
        public string WindowsPrinterName2
        {
            get { return (string)this["WINDOWSPRINTERNAME2"]; }
            set { this["WINDOWSPRINTERNAME2"] = value; }
        }

        [DataMember]
        [Column("WINDOWSPRINTERNAME3")]
        public string WindowsPrinterName3
        {
            get { return (string)this["WINDOWSPRINTERNAME3"]; }
            set { this["WINDOWSPRINTERNAME3"] = value; }
        }
        
    }
}
