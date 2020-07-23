using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Repositories
{
    public class RequestData
    {
        public RequestData()
        {
            Page = 1;
            Size = 10;
            Sort = "desc";
            Order = "CreationDate";
            Filter = new List<FilterData>();
            CustomFilter = new List<FilterData>();
        }
        public List<int> Ids { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
        public List<FilterData> Filter { get; set; }
        public List<FilterData> CustomFilter { get; set; }//no database

        public string Order { get; set; }
        public string Sort { get; set; }
        public string OrderBy => Order + " " + Sort;
        public bool Cache { get; set; }
    }

    public class FilterData
    {

        public FilterData()
        {
          
        }
        public FilterData(string Key, string Operator, string Value)
        {
            this.Key = Key;
            this.Operator = Operator;
            this.Value = Value;
        }

        public string Key { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public override string ToString()
        {
            return Key + Operator + Value;
        }
    }

}
