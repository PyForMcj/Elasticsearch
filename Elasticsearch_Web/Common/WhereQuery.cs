using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nest;

namespace Elasticsearch_Web.Common
{
    /// <summary>
    /// 多条件搜索例子
    /// </summary>
    public class WheresQuerDemo
    {

        public class WhereInfo
        {
            public int venId { get; set; }
            public string venName { get; set; }

        }

        /// <summary>
        /// 
        /// </summary>
        public static void Search()
        {
            Helper helper = new Helper();
            var result = helper.elasticClient.Search<Student>(CreateSearchRequest(new WhereInfo()));
        }
        /// <summary>
        /// searchRequest 生成
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public static Func<SearchDescriptor<Student>, ISearchRequest> CreateSearchRequest(WhereInfo where)
        {
            //querys
            var mustQuerys = new List<Func<QueryContainerDescriptor<Student>, QueryContainer>>();
            if (where.venId > 0)
            {
                mustQuerys.Add(t => t.Term(f => f.Id, where.venId));
            }

            //filters
            var mustFilters = new List<Func<QueryContainerDescriptor<Student>, QueryContainer>>();
            if (!string.IsNullOrEmpty(where.venName))
            {
                mustFilters.Add(t => t.MatchPhrase(f => f.Field(fd => fd.Description).Query(where.venName)));
            }
            Func<SearchDescriptor<Student>, ISearchRequest> searchRequest = r =>
                r.Query(q =>
                            q.Bool(b =>
                                        b.Must(mustQuerys)

                                        .Filter(f =>
                                                    f.Bool(fb =>
                                                        fb.Must(mustFilters))
                                                )
                                   )
                        );

            return searchRequest;
        }
    }
}