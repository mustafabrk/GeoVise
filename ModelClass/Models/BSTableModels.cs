using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Models
{
    //TODO: Generic class olacak şekilde düzenlenecek
    public class BSTableResp<T>
    {
        public int total { get; set; }
        public int totalNotFiltered { get; set; }
        public /*List<ToDo>*/T rows { get; set; }
    }

    public class BSTableReq<T>
    {   //Search ve Searchable değerlerini ekle
        public int offset { get; set; }
        public int limit { get; set; }
        public string sort { get; set; }
        public string order { get; set; }
        public T search { get; set; }
        public T searchable { get; set; }


    }
}
