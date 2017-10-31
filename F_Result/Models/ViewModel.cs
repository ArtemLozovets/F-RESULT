using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F_Result.Models
{

    [NotMapped]
    public class APBTableReport
    {
        public string ProjectName { get; set; }
        public decimal debitplan { get; set; }
        public decimal debitfact { get; set; }
        public decimal ddelta { get; set; }
        public decimal creditplan { get; set; }
        public decimal creditfact { get; set; }
        public decimal cdelta { get; set; }
    }

    [NotMapped]
    public class TableReport
    {
        public string ProjectName { get; set; }
        public decimal d1c { get; set; }
        public decimal d1d { get; set; }
        public decimal d2c { get; set; }
        public decimal d2d { get; set; }
        public decimal d3c { get; set; }
        public decimal d3d { get; set; }
        public decimal d4c { get; set; }
        public decimal d4d { get; set; }
        public decimal d5c { get; set; }
        public decimal d5d { get; set; }
        public decimal d6c { get; set; }
        public decimal d6d { get; set; }
        public decimal d7c { get; set; }
        public decimal d7d { get; set; }
        public decimal d8c { get; set; }
        public decimal d8d { get; set; }
        public decimal d9c { get; set; }
        public decimal d9d { get; set; }
        public decimal d10c { get; set; }
        public decimal d10d { get; set; }
        public decimal d11c { get; set; }
        public decimal d11d { get; set; }
        public decimal d12c { get; set; }
        public decimal d12d { get; set; }
        public decimal dresc { get; set; }
        public decimal dresd { get; set; }

    }

    [NotMapped]
    public class Summary
    {
        // Total row
        public string RowName { get; set; }
        public decimal d1c { get; set; }
        public decimal d1d { get; set; }
        public decimal d2c { get; set; }
        public decimal d2d { get; set; }
        public decimal d3c { get; set; }
        public decimal d3d { get; set; }
        public decimal d4c { get; set; }
        public decimal d4d { get; set; }
        public decimal d5c { get; set; }
        public decimal d5d { get; set; }
        public decimal d6c { get; set; }
        public decimal d6d { get; set; }
        public decimal d7c { get; set; }
        public decimal d7d { get; set; }
        public decimal d8c { get; set; }
        public decimal d8d { get; set; }
        public decimal d9c { get; set; }
        public decimal d9d { get; set; }
        public decimal d10c { get; set; }
        public decimal d10d { get; set; }
        public decimal d11c { get; set; }
        public decimal d11d { get; set; }
        public decimal d12c { get; set; }
        public decimal d12d { get; set; }
        public decimal dresc { get; set; }
        public decimal dresd { get; set; }
    }

    [NotMapped]
    public class Deviation
    {
        // Deviation row 
        public string DevRowName { get; set; }
        public decimal dev1 { get; set; }
        public decimal dev2 { get; set; }
        public decimal dev3 { get; set; }
        public decimal dev4 { get; set; }
        public decimal dev5 { get; set; }
        public decimal dev6 { get; set; }
        public decimal dev7 { get; set; }
        public decimal dev8 { get; set; }
        public decimal dev9 { get; set; }
        public decimal dev10 { get; set; }
        public decimal dev11 { get; set; }
        public decimal dev12 { get; set; }
        public decimal devres { get; set; }

    }
}