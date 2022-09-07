using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Words
{
    public class WordLists
    {
        public int Id { get; set; }
        public IList<Words> Words { get; set; } = new List<Words>();
    }
}
