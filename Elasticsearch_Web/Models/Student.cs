using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;

namespace Elasticsearch_Web
{
    /*
     *[ElasticsearchType(Name = “文档的类型”,IdProperty = “文档的唯一键字段名”)]
     *[Number(NumberType.Long,Name = “Id”)] 数字类型 +名称
     *[Keyword(Name = “Name”,Index = true)]不需要分词的字符串，name=名称,index=是否建立索引
     *[Text(Name = “Dic”, Index = true,Analyzer = “ik_max_word”)]需要分词的字符串，name=名称,index=是否建立索引,Analyzer=分词器
     * ik_smart 最小分词量
     * ik_max_word 最大分词量
    */

    [ElasticsearchType(Name = "student", IdProperty = "Id")]
    public class Student
    {
        [Number(NumberType.Long, Name = "Id")]
        public long Id { get; set; }

        [Keyword(Name = "Name", Index = true)]
        public string Name { get; set; }

        [Text(Name = "Description", Index = true, Analyzer = "ik_max_word")]
        public string Description { get; set; }

        [Date(Name = "DateTime")]
        public DateTime DateTime { get; set; }
    }
}
