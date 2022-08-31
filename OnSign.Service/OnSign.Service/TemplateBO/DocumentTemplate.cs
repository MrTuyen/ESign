using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.TemplateBO
{
    public class DocumentTemplate
    {
        public long ID { get; set; }
        public DateTime CREATEDATTIME { get; set; }
        public int CREATEDBYUSER { get; set; }
        public string CREATEDBYIP { get; set; }
        public string PATH { get; set; }
        public string NAME { get; set; }
        public string ICON { get; set; }
        public long SIZE { get; set; }
        public int PAGES { get; set; }
        public string TEMPPATH { get; set; }
        public int TOTALROW { get; set; }
        public string HTML { get; set; }
        public List<DocumentTemplatePdf> DocumentTemplatePdfs { get; set; }
        /// <summary>
        /// Thông tin bookmarks của file word
        /// </summary>
        public List<DocumentTemplateBookmark> DocumentTemplateBookmarks { get; set; }

        public DocumentTemplate()
        {
            DocumentTemplatePdfs = new List<DocumentTemplatePdf>();
            DocumentTemplateBookmarks = new List<DocumentTemplateBookmark>();
        }
    }
}
