using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Models
{
    //TODO: Generic class olacak şekilde düzenlenecek
    public class User
    {
        public decimal Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime SessionDuration { get; set; }
    }
}
