using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Sign
{
    public class RequestSignBO
    {
        public RequestSignBO()
        {

        }

        public RequestSignBO(bool HasInitChild)
        {
            LISTSIGN = new List<DocumentSignBO>();
            FILEUPLOADS = new List<DocumentBO>();
            LISTMAILTO = new List<ReceiverBO>();
        }

        public long ID { get; set; }
        public string UUID { get; set; }
        public string CATEGORY { get; set; }
        public DateTime CREATEDATTIME { get; set; }
        public string CREATEDBYIP { get; set; }
        public int CREATEDBYUSER { get; set; }
        public string STATUS { get; set; }
        public string STATUSSIGN { get; set; }
        public bool ISONLYMESIGN { get; set; }
        public bool ISDELETED { get; set; }
        public int DELETEDBYUSER { get; set; }
        public DateTime LASTACTIVITY { get; set; }
        public DateTime SIGNEDATTIME { get; set; }
        //Ten nguoi gui
        public string FULLNAME { get; set; }
        //Email nguoi gui
        public string EMAIL { get; set; }
        public string TAXCODE { get; set; }
        public string EMAILSUBJECT { get; set; }
        public string EMAILMESSAGES { get; set; }
        public string EMAILTO { get; set; }
        public string EMAILNAME { get; set; }
        public string NEXTSIGN { get; set; }
        public int DEADLINETIME { get; set; }
        public int REPEATEVERYTIME { get; set; }
        
        public List<ReceiverBO> LISTMAILTO { get; set; }

        private string _strlistmailto;
        public string STRLISTMAILTO
        {
            get { return _strlistmailto; }
            set
            {
                _strlistmailto = value;
            }
        }

        public List<DocumentBO> FILEUPLOADS { get; set; }

        private string _strfileuploads;
        public string STRFILEUPLOADS
        {
            get { return this._strfileuploads; }
            set
            {
                this._strfileuploads = value;
            }
        }

        private List<DocumentSignBO> _listsign;
        public List<DocumentSignBO> LISTSIGN
        {
            get { return _listsign; }
            set
            {
                _listsign = value;
                FILEUPLOADS?.ForEach((doc) =>
                {
                    doc.SIGNFORDISPLAY?.ForEach((sfd) =>
                    {
                        _listsign.ForEach((s) =>
                        {
                            if (s.ID == sfd.ID && !string.IsNullOrEmpty(sfd.SIGNATUREIMAGE) && s.IDDOC == sfd.IDDOC && string.IsNullOrEmpty(s.SIGNATUREIMAGE))
                            {
                                s.SIGNATUREIMAGE = sfd.SIGNATUREIMAGE;
                            }
                        });
                    });
                });
            }
        }

        public long TOTALROW { get; set; }
        public long TOTALPAGES { get; set; }
        public int NEEDMESIGN { get; set; }
        public int WAITTINGFORORTHERS { get; set; }
        public int COMPLETE { get; set; }
        public bool ISSIGNED { get; set; }
        public bool ISSELECTED { get; set; }
        public bool ISCANCELED { get; set; }
        public string CANCELREASON { get; set; }
        public string DECLINEREASON { get; set; }
        public string IDCONTRACT { get; set; }

    }

    public class ClientInfo
    {
        public string EMAIL { get; set; }
        public string NAME { get; set; }
    }
}