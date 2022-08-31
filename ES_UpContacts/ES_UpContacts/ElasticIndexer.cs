using Nest;
using System;
using System.Configuration;
//using Elasticsearch.Net.ConnectionPool;
//C:\Users\huydungktv\Downloads\ESsearchTest\ESsearchTest\packages\Elasticsearch.Net.2.3.2\lib\net45\Elasticsearch.Net.dll

namespace ES_UpContacts
{
    public class ElasticIndexer
    {
        public static object lockQueue = new object();
        public ElasticClient elasticClient = null;

        public ElasticClient IndexClient
        {
            get { return elasticClient; }
        }

        static ElasticIndexer instance;
        public static ElasticIndexer Current
        {
            get
            {
                return instance ?? (instance = new ElasticIndexer());
            }
        }

        public ElasticIndexer()
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ELASTIC_MASTER_NODE"]))
            {
                var node = new Uri(ConfigurationManager.AppSettings["ELASTIC_MASTER_NODE"]);
                var config = new ConnectionSettings(node);
                elasticClient = new ElasticClient(config);
            }
        }


        public bool DeleteObject(string index, string type, string id)
        {
            lock (lockQueue)
            {

                IDeleteResponse response = elasticClient.Delete(new DeleteRequest(index, type, id));
                if (response != null)
                {
                    if (response.IsValid == true)
                    {
                        return response.IsValid;
                    }
                    else
                    {
                        throw new Exception("ElasticIndexer::DeleteObject::" + response.ServerError.Error);
                    }
                }
                return false;
            }
        }



    }
}