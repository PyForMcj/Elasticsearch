using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nest;

namespace Elasticsearch_Web.Models
{

    public class SearchViewModel<T> where T : class
    {
        public IReadOnlyCollection<IHit<T>> Hits { get; set; }

        public long Total { get; set; }
    }
}