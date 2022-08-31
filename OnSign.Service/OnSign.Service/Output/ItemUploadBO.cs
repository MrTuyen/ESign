using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Output
{
    //public class ItemUploadBO
    //{
    //    public string name { get; set; }
    //    public string path { get; set; }
    //    public string type { get; set; }
    //    public string icon { get; set; }
    //    public int size { get; set; }
    //    public int pages { get; set; }
    //    public List<Thumb> thumbs { get; set; }
    //}

    public class Thumb
    {
        public string name { get; set; }
        public int page { get; set; }
        public string small { get; set; }
        public string big { get; set; }
        public float width { get; set; }
        public float height { get; set; }
    }
}
