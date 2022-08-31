using Nest;
using OnSign.BusinessObject.Forms;
using OnSign.BusinessObject.Sign;
using OnSign.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessLogic.Partners
{
    public class ElasticsearchBLL : BaseBLL
    {

        public static System.Object lockQueue = new System.Object();
        public static ElasticClient elasticClient;

        public ElasticsearchBLL()
        {
            try
            {
                if (elasticClient == null)
                {
                    Uri node = new Uri(ConfigHelper.ES_IP);
                    ConnectionSettings config = new ConnectionSettings(node);
                    elasticClient = new ElasticClient(config);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public ElasticClient IndexClient
        {
            get { return elasticClient; }
        }

        #region Singleton

        static ElasticsearchBLL instance;
        public static ElasticsearchBLL Current
        {
            get
            {
                return instance ?? (instance = new ElasticsearchBLL());
            }
        }

        #endregion

        public class xMailToBO
        {
            public int createdByUser { get; set; }
            public string name { get; set; }
            public string email { get; set; }
            public string taxCode { get; set; }
            public string address { get; set; }
            public string idNumber { get; set; }
            public string phoneNumber { get; set; }
            public string keyWord { get; set; }
        }

        public List<ReceiverBO> ES_ReceiveByKeyword(string strEmail, int createdByUser, int pagesize, int pageindex)
        {
            try
            {
                string rsl_en = $"cREATEDBYUSER:{createdByUser} AND eMAIL:({strEmail})";
                var result = Current.IndexClient.Search<ReceiverBO>(s => s
                    .Index("contacts_list")
                    .From(pageindex * pagesize)
                    .Size(pagesize)
                    .Query(q =>
                        q.QueryString(qs =>
                            qs.Query(rsl_en)
                            .Type(TextQueryType.PhrasePrefix)
                        )
                    )
                );
                return result.Documents.ToList(); ;
            } 
            catch (Exception objEx)
            {
                var msg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi gọi Elasticsearch customer");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(msg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return null;
            }
        }

    }
}
