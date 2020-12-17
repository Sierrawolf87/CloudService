using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudService_API.Data
{
    public class RequestParameters 
    {
        int _PageNumber = 1;
        int _PageSize = 150;
        int _count = 1;

        public int PageCount {
            get => (int)Math.Ceiling(_count / (double)_PageSize);
            set => _count = value;
        }

        public int PageNumber
        {
            get => _PageNumber;
            set
            {
                if (value < 1)
                    _PageNumber = 1;
                if (value > PageCount)
                    _PageNumber = PageCount;
            }
        }

        public int PageSize {
            get => _PageSize;
            set
            {
                if (value < 1)
                    _PageSize = 1;
                if (value > 150)
                    _PageSize = 150;
            }
        }

        public int Skip => (PageNumber - 1) * PageSize;
        public int Take => PageSize;
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
