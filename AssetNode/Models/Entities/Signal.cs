using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetNode.Models.Entities
{
    public class Signal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SignalId { get; set; }


        public string SignalName { get; set; }    

        public string ValueType { get; set; }

        public string Description { get; set; }

        public int AssetID { get; set; }

        public Asset asset { get; set; }
    }
}
