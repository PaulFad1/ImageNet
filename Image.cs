using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImageNet.Models
{
    [Table("Images")]
    public class Image
    {
        [Key]
        
        public int Id {get;set;}
        public string Path{get;set;}
        public User User {get;set;} = null!;
    }
}