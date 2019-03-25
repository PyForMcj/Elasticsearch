using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch_Web.Models;
using Nest;
namespace Elasticsearch_Web
{
    public class Helper
    {
        private static string elasticsearchNodeUri = System.Configuration.ConfigurationManager.AppSettings["ElasticsearchNodeUri"];
        private ElasticClient elasticClient;
        public Helper()
        {
            var node = new Uri(elasticsearchNodeUri);
            var settings = new ConnectionSettings(node).DefaultFieldNameInferrer((name) => name);
            this.elasticClient = new ElasticClient(settings);
        }

        public SearchViewModel<Student> Query(string indexName, string key)
        {

            var searchResponse = elasticClient.Search<Student>(s =>
                s.Index(indexName)//指定具体索引
                .From(0)//分页跳过多少
                .Size(50)//分页获取多少
                         //.Query(q=>q.MatchAll())//查询所有不带条件
                         //.Query(q =>q.QueryString(qs=>qs.Query("中国")))//在特殊的_all字段下并建立索引。如果没有在搜索请求中显式指定字段名，查询将默认使用_all字段。
                         //.Query(q=>q
                         //    .MultiMatch(m => m
                         //        .Fields(f => f
                         //            .Fields(
                         //                p => p.Name,
                         //                p => p.Description)
                         //            )
                         //        .Query("中国")
                         //    )
                         //)//指定查询字段
                .Query(q => q
                    .MultiMatch(m => m
                        .Fields(f => f.Fields(p => p.Name, p => p.Description))
                        //.Operator(Operator.And)//针对查询条件的全匹配，比如"北京天安门"，只有数据有这几个字才能搜索出来，比如IK会将"北京天安门"分词成"北京","天安门",那么其他包含"北京","天安门"的数据搜索不出来
                        .Operator(Operator.Or)// 针对查询条件的或匹配，"北京天安门"，"北京","天安门"都能搜出来
                        .Query(key)
                    )
                )
                .Highlight(h => h
                .PreTags("<b>")
                .PostTags("</b>")
                .Fields(
                     f => f.Field(n => n.Description)
                )
             )
            );
            var source = searchResponse.HitsMetadata.Hits.Select(h => h.Source);
            var highlights = searchResponse.HitsMetadata.Hits.Select(h => h
                                .Highlights
                            );
            foreach (var item in highlights)
            {
                var i = item.Keys;
                var j = item.Values;
                var dd = item.Where(c => c.Key == "Description");
            }
            var model = new SearchViewModel<Student>
            {
                Hits = searchResponse.Hits,
                Total = searchResponse.Total
            };
            return model;
        }


        #region 索引与Type(数据库与表)

        /// <summary>
        /// 判断Index索引是否存在
        /// </summary>
        /// <param name="indexName">Index索引名称</param>
        /// <returns>是否存在</returns>
        public bool IndexExists(string indexName)
        {
            try
            {
                return this.elasticClient.IndexExists(indexName).Exists ? true : false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="indexName">Index索引名称</param>
        /// <param name="numberOfShards">主分片(单机一般默认1，多节点默认5)</param>
        /// <param name="numberOfReplicas">副分片(单机一般默认0，多节点默认1)</param>
        /// <returns>是否创建成功</returns>
        public bool CreateIndex(string indexName, int numberOfShards, int numberOfReplicas)
        {
            if (!this.IndexExists(indexName))
            {
                var descriptor = new CreateIndexDescriptor(indexName)
                    .Settings(s =>
                    //该索引的分片数为5、副本数为1
                    s.NumberOfShards(numberOfShards).NumberOfReplicas(numberOfReplicas)
                    );
                elasticClient.CreateIndex(descriptor);
            }
            return this.IndexExists(indexName);
        }

        /// <summary>
        /// 创建索引并映射类型
        /// </summary>
        /// <typeparam name="T">映射对于实体</typeparam>
        /// <param name="indexName">Index索引名称</param>
        /// <param name="numberOfShards">主分片(单机一般默认1，多节点默认5)</param>
        /// <param name="numberOfReplicas">副分片(单机一般默认0，多节点默认1)</param>
        /// <returns>是否创建成功</returns>
        public bool CreateIndexAutoMap<T>(string indexName, int numberOfShards, int numberOfReplicas) where T : class
        {
            if (!this.IndexExists(indexName))
            {
                var descriptor = new CreateIndexDescriptor(indexName)
                    .Settings(s =>
                    //该索引的分片数为5、副本数为1
                    s.NumberOfShards(numberOfShards).NumberOfReplicas(numberOfReplicas))
                    .Mappings(ms => ms
                    .Map<T>(m => m.AutoMap())
                    );
                elasticClient.CreateIndex(descriptor);
            }
            return this.IndexExists(indexName);
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="indexName">Index索引名称</param>
        /// <returns>是否删除成功</returns>
        public bool DeleteIndex(string indexName)
        {
            if (this.IndexExists(indexName))
            {
                var descriptor = new DeleteIndexDescriptor(indexName).Index(indexName);
                elasticClient.DeleteIndex(descriptor);
            }
            return this.IndexExists(indexName);
        }

        /// <summary>
        /// 删除指定索引所在节点下的所有索引
        /// </summary>
        /// <param name="indexName">Index索引名称</param>
        /// <returns>是否删除成功</returns>
        public bool DeleteIndexAll(string indexName)
        {
            if (this.IndexExists(indexName))
            {
                var descriptor = new DeleteIndexDescriptor("db_student").AllIndices();
                elasticClient.DeleteIndex(descriptor);
            }
            return this.IndexExists(indexName);
        }

        #endregion

        #region 文档(数据)

        #region 新增

        /// <summary>
        /// 添加单条数据
        /// </summary>
        /// <typeparam name="T">映射对于实体</typeparam>
        /// <param name="t">实体实例</param>
        /// <param name="indexName">Index索引名称</param>
        public void CreateDocument<T>(T t, string indexName) where T : class
        {
            if (this.IndexExists(indexName))
            {
                this.elasticClient.Index(t, s => s.Index(indexName));
                //this.elasticClient.Index(t, s => s.Index(indexName).Type(t.GetType().Name));
            }
        }

        /// <summary>
        /// 添加多条数据
        /// </summary>
        /// <typeparam name="T">映射对于实体</typeparam>
        /// <param name="t">实体实例集合</param>
        /// <param name="indexName">Index索引名称</param>
        public void CreateDocument<T>(IEnumerable<T> t, string indexName) where T : class
        {
            if (this.IndexExists(indexName))
            {
                this.elasticClient.IndexMany(t, indexName);
                //this.elasticClient.IndexMany(t, indexName, t.GetType().Name);
            }
        }

        #endregion

        #region 删除

        /// <summary>
        /// 删除单条数据
        /// </summary>
        /// <typeparam name="T">映射对于实体</typeparam>
        /// <param name="s">指定Id的值</param>
        /// <param name="indexName">Index索引名称</param>
        public void DeleteDocument<T>(long s, string indexName) where T : class
        {
            if (this.IndexExists(indexName))
            {
                this.elasticClient.Delete<T>(s, c => c.Index(indexName));
            }
        }

        /// <summary>
        /// 删除单条数据
        /// </summary>
        /// <typeparam name="T">映射对于实体</typeparam>
        /// <param name="s">指定Id的值</param>
        /// <param name="indexName">Index索引名称</param>
        public void DeleteDocument<T>(string s, string indexName) where T : class
        {
            if (this.IndexExists(indexName))
            {
                this.elasticClient.Delete<T>(s, c => c.Index(indexName));
            }
        }

        /// <summary>
        /// 删除单条数据
        /// </summary>
        /// <typeparam name="T">映射对于实体</typeparam>
        /// <param name="s">指定Id的值</param>
        /// <param name="indexName">Index索引名称</param>
        public void DeleteDocument<T>(Guid s, string indexName) where T : class
        {
            if (this.IndexExists(indexName))
            {
                this.elasticClient.Delete<T>(s, c => c.Index(indexName));
            }
        }

        /// <summary>
        /// 删除单条数据
        /// </summary>
        /// <typeparam name="T">映射对于实体</typeparam>
        /// <param name="s">指定Id的值</param>
        /// <param name="indexName">Index索引名称</param>
        public void DeleteDocument<T>(Id s, string indexName) where T : class
        {
            if (this.IndexExists(indexName))
            {
                this.elasticClient.Delete<T>(s, c => c.Index(indexName));
            }
        }

        /// <summary>
        /// 删除单条数据
        /// </summary>
        /// <typeparam name="T">映射对于实体</typeparam>
        /// <param name="s">指定Id的值</param>
        /// <param name="indexName">Index索引名称</param>
        public void DeleteDocument<T>(T s, string indexName) where T : class
        {
            if (this.IndexExists(indexName))
            {
                this.elasticClient.Delete<T>(s, c => c.Index(indexName));
            }
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <typeparam name="T">映射对于实体</typeparam>
        /// <param name="s">指定Id的值</param>
        /// <param name="indexName">Index索引名称</param>
        public void DeleteDocument<T>(List<long> s, string indexName) where T : class
        {
            if (this.IndexExists(indexName))
            {
                var bulkQuest = new BulkRequest() { Operations = new List<IBulkOperation>() };
                foreach (var v in s)
                {
                    bulkQuest.Operations.Add(new BulkDeleteOperation<T>(v));
                }
                this.elasticClient.Bulk(bulkQuest);
            }
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <typeparam name="T">映射对于实体</typeparam>
        /// <param name="s">指定Id的值</param>
        /// <param name="indexName">Index索引名称</param>
        public void DeleteDocument<T>(List<string> s, string indexName) where T : class
        {
            if (this.IndexExists(indexName))
            {
                var bulkQuest = new BulkRequest() { Operations = new List<IBulkOperation>() };
                foreach (var v in s)
                {
                    bulkQuest.Operations.Add(new BulkDeleteOperation<T>(v));
                }
                this.elasticClient.Bulk(bulkQuest);
            }
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <typeparam name="T">映射对于实体</typeparam>
        /// <param name="s">指定Id的值</param>
        /// <param name="indexName">Index索引名称</param>
        public void DeleteDocument<T>(List<Guid> s, string indexName) where T : class
        {
            if (this.IndexExists(indexName))
            {
                var bulkQuest = new BulkRequest() { Operations = new List<IBulkOperation>() };
                foreach (var v in s)
                {
                    bulkQuest.Operations.Add(new BulkDeleteOperation<T>(v));
                }
                this.elasticClient.Bulk(bulkQuest);
            }
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <typeparam name="T">映射对于实体</typeparam>
        /// <param name="s">指定Id的值</param>
        /// <param name="indexName">Index索引名称</param>
        public void DeleteDocument<T>(List<Id> s, string indexName) where T : class
        {
            if (this.IndexExists(indexName))
            {
                var bulkQuest = new BulkRequest() { Operations = new List<IBulkOperation>() };
                foreach (var v in s)
                {
                    bulkQuest.Operations.Add(new BulkDeleteOperation<T>(v));
                }
                this.elasticClient.Bulk(bulkQuest);
            }
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <typeparam name="T">映射对于实体</typeparam>
        /// <param name="s">指定Id的值</param>
        /// <param name="indexName">Index索引名称</param>
        public void DeleteDocument<T>(List<T> s, string indexName) where T : class
        {
            if (this.IndexExists(indexName))
            {
                var bulkQuest = new BulkRequest() { Operations = new List<IBulkOperation>() };
                foreach (var v in s)
                {
                    bulkQuest.Operations.Add(new BulkDeleteOperation<T>(v));
                }
                this.elasticClient.Bulk(bulkQuest);
            }
        }

        #endregion

        #region 修改

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <typeparam name="T">映射对于实体</typeparam>
        /// <param name="t">实体实例</param>
        /// <param name="indexName">Index索引名称</param>
        public void UpdateDocument<T>(T t, long s, string indexName) where T : class
        {
            if (this.IndexExists(indexName))
            {
                this.elasticClient.Update<T, object>(s, (upt) => upt.Doc(t).Index(indexName));
            }
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <typeparam name="T">映射对于实体</typeparam>
        /// <param name="t">实体实例</param>
        /// <param name="indexName">Index索引名称</param>
        public void UpdateDocument<T>(T t, string s, string indexName) where T : class
        {
            if (this.IndexExists(indexName))
            {
                this.elasticClient.Update<T, object>(s, (upt) => upt.Doc(t).Index(indexName));
            }
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <typeparam name="T">映射对于实体</typeparam>
        /// <param name="t">实体实例</param>
        /// <param name="indexName">Index索引名称</param>
        public void UpdateDocument<T>(T t, Guid s, string indexName) where T : class
        {
            if (this.IndexExists(indexName))
            {
                this.elasticClient.Update<T, object>(s, (upt) => upt.Doc(t).Index(indexName));
            }
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <typeparam name="T">映射对于实体</typeparam>
        /// <param name="t">实体实例</param>
        /// <param name="indexName">Index索引名称</param>
        public void UpdateDocument<T>(T t, Id s, string indexName) where T : class
        {
            if (this.IndexExists(indexName))
            {
                this.elasticClient.Update<T, object>(s, (upt) => upt.Doc(t).Index(indexName));
            }
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <typeparam name="T">映射对于实体</typeparam>
        /// <param name="t">实体实例</param>
        /// <param name="indexName">Index索引名称</param>
        public void UpdateDocument<T>(T t, T s, string indexName) where T : class
        {
            if (this.IndexExists(indexName))
            {
                this.elasticClient.Update<T, object>(s, (upt) => upt.Doc(t).Index(indexName));
            }
        }

        #endregion

        #endregion
    }
}
