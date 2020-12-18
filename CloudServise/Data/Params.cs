using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudService_API.Data
{
    public class RequestParameters 
    {
        public int PageCount {
            get => (int)Math.Ceiling(TotalCount / (double)PageSize);
        }

        public int TotalCount { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public int Skip => (PageNumber - 1) * PageSize;
        public int Take => PageSize;

        public virtual string ToJson() => JsonConvert.SerializeObject(this);

        public void Check()
        {
            if (PageNumber < 1)
                PageNumber = 1;

            if (PageSize < 1)
                PageSize = 1;

            if (PageNumber > PageCount)
                PageNumber = PageCount;
        }
    }


    public class UsersParameters: RequestParameters
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string ReportCard { get; set; }
    }
}
