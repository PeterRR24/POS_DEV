using System;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

namespace RR.Commerce.Runtime.Printing
{
    public enum SystemPrinterType
    {
        Default = 0,
        Printer1 = 1,
        Printer2 = 2
    }

    public enum WindowsPrinterType
    {
        None = 0,
        Terminal1 = 1,
        Terminal2 = 2,
        Terminal3 = 3,
        Custom = 4,
    }

    public enum LabelPrinterTemplate
    {
        None = 0,
        Template1 = 1,
        Template2 = 2,
        Template3 = 3,
    }

    public class PRPrinter : CommerceEntity
    {
        private const string IdColumn = "RECID";

        public PRPrinter() : base("PRPrinter")
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
        [Column("PRINTERID")]
        public string PrinterId
        {
            get { return (string)this["PRINTERID"]; }
            set { this["PRINTERID"] = value; }
        }

        [DataMember]
        [Column("NAME")]
        public string Name
        {
            get { return (string)this["NAME"]; }
            set { this["NAME"] = value; }
        }

        //[DataMember]
        //[Column("SYSTEMPRINTER")]
        //public SystemPrinterType SystemPrinter
        //{
        //    get { return (SystemPrinterType)(this["SYSTEMPRINTER"] ?? SystemPrinterType.Default); }
        //    set { this["SYSTEMPRINTER"] = value; }
        //}

        [DataMember]
        [Column("WINDOWSPRINTER")]
        public WindowsPrinterType WindowsPrinter
        {
            get { return (WindowsPrinterType)(this["WINDOWSPRINTER"] ?? WindowsPrinterType.Custom); }
            set { this["WINDOWSPRINTER"] = value; }
        }

        [DataMember]
        [Column("WINDOWSPRINTERNAME")]
        public string WindowsPrinterName
        {
            get { return (string)this["WINDOWSPRINTERNAME"]; }
            set { this["WINDOWSPRINTERNAME"] = value; }
        }

        [DataMember]
        [Column("LABELPRINTER")]
        public LabelPrinterTemplate LabelPrinter
        {
            get { return (LabelPrinterTemplate)(this["LABELPRINTER"] ?? LabelPrinterTemplate.None); }
            set { this["LABELPRINTER"] = value; }
        }
    }
}
