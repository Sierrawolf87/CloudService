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
        public int PageCount
        {
            get => (int)Math.Ceiling(TotalCount / (double)Size);
        }

        public int TotalCount { get; set; }

        public int? Page { get; set; }
        public int Size { get; set; }

        public int Skip { get; set; }
        public int Take { get; set; }

        public int? Next
        {
            get
            {
                if (Page != null)
                {
                    int next = (int)(Page + 1);
                    if (next <= PageCount)
                    {
                        return next;
                    }
                }
                return null;
            }
        }

        public int? Prev
        {
            get
            {
                if (Page != null)
                {
                    int prev = (int)(Page - 1);
                    if (prev > 0)
                    {
                        return prev;
                    }
                }
                return null;
            }
        }

        public virtual string ToJson() => JsonConvert.SerializeObject(this);

        public bool Check()
        {
            if (Page == null)
                return false;

            if (Page < 1)
                Page = 1;

            if (Size < 1)
                Size = 20;

            if (Page > PageCount)
                Page = PageCount;

            Skip = (int)((Page - 1) * Size);
            Take = Size;

            return true;
        }
    }


    public class UsersParameters : RequestParameters
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string ReportCard { get; set; }
    }
}
