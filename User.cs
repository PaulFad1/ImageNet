using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImageNet.Models
{
    [Table("Users")]
    public class User 
    {
        [Key]
        public int Id {get;set;}
        public string Login {get;set;}

        public string Password {get;set;}
        
        public IReadOnlyCollection<Image> Images => images;

        private readonly List<Image> images = new List<Image>();
        public ICollection<User> Friends {get;set;}

    }
}