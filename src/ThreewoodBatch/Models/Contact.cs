using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreewoodBatch.Models
{
    [AttributeUsage(AttributeTargets.All)]
    public class Column : System.Attribute
    {
        public int ColumnIndex { get; set; }
        public string ColumnName { get; set; }

        public Column(int column)
        {
            ColumnIndex = column;
        }
        public Column(string column)
        {
            ColumnName = column;
        }
    }

    public class ContactDto
    {
        public enum RowAction
        {
            [Description("Add")]
            Add,
            [Description("Edit")]
            Edit,
            [Description("Delete")]
            Delete            
        };

        [Column("A")]
        [Required]
        public string Action { get; set; }

        [Column("B")]
        [Required]
        public string Id { get; set; }

        [Column("C")]
        [Required]
        public string EmpolyeeId { get; set; }

        [Column("D")]
        [Required]
        public string Department_EN { get; set; }

        [Column("E")]
        [Required]
        public string Department_CHT { get; set; }

        [Column("F")]
        [Required]
        public string Name_EN { get; set; }

        [Column("G")]
        [Required]
        public string Name_CHT { get; set; }

        [Column("H")]
        [Required]
        public string Position_EN { get; set; }

        [Column("I")]
        [Required]
        public string Position_CHT { get; set; }

        [Column("J")]
        [Required]
        public string WorkLocation_EN { get; set; }

        [Column("K")]
        [Required]
        public string WorkLocation_CHT { get; set; }

        [Column("L")]
        [Required]
        public string Floor { get; set; }

        [Column("M")]
        [Required]
        public string Trade_EN { get; set; }

        [Column("N")]
        [Required]
        public string Trade_CHT { get; set; }

        [Column("O")]
        [Required]
        public string Telephone { get; set; }

        [Column("P")]
        [Required]
        public string Fax { get; set; }

        [Column("Q")]
        [Required]
        public string Email { get; set; }
    }    
}
