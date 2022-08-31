using OnSign.BusinessObject.Output;
using System.Collections.Generic;
using System.Linq;

namespace OnSign.BusinessObject.Sign
{
    public class DocumentBO
    {
        public long ID { get; set; }
        public string UUID { get; set; }
        public string CREATEDBYIP { get; set; }
        public string TAXCODE { get; set; }
        public int CREATEDBYUSER { get; set; }
        public long IDREQUEST { get; set; }
        public string NAME { get; set; }
        public string PATH { get; set; }
        public string PDFPATH { get; set; }
        public long SIZE { get; set; }
        public int SIGNINDEX { get; set; }
        public string ICON { get; set; }
        public string STATUS { get; set; }
        public string VIEWER_PAGES_SIZE { get; set; }
        public int NUMBEROFPAGES { get; set; }

        private List<DocumentSignBO> _sign;
        public List<DocumentSignBO> SIGN
        {
            get { return _sign; }
            set
            {
                _sign = value;
                if (_sign != null)
                {
                    _sign.ForEach((x) => { x.IDDOC = ID; x.DOCPATH = PATH; });
                }
            }
        }

        private List<DocumentSignBO> _signfordisplay;
        public List<DocumentSignBO> SIGNFORDISPLAY
        {
            get { return _signfordisplay; }
            set
            {
                _signfordisplay = value;
                _signfordisplay.ForEach((sfd) =>
                {
                    _sign.ForEach((s) =>
                    {
                        if (s.ID == sfd.ID && !string.IsNullOrEmpty(sfd.SIGNATUREIMAGE) && string.IsNullOrEmpty(s.SIGNATUREIMAGE))
                        {
                            s.SIGNATUREIMAGE = sfd.SIGNATUREIMAGE;
                        }
                    });
                });
            }
        }

        public string TYPE { get; set; }
        public int PAGES { get; set; }
        public List<Thumb> thumbs { get; set; }
        public string SIZEIMAGELIST { get; set; }
        public float PDFREALWIDTH { get; set; }
        public float PDFREALHEIGHT { get; set; }

        public DocumentBO()
        {
            SIGN = new List<DocumentSignBO>();
            thumbs = new List<Thumb>();
        }
    }
}
