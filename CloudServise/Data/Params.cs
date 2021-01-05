using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudService_API.Data
{
    public class PageParameters
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

        public virtual string PaginationToJson() => JsonConvert.SerializeObject(this);

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


    public class UsersParameters : PageParameters
    {
        public string Text { get; set; }
        public string Role { get; set; }
        public string Group { get; set; }
    }

    public class SolutionParameters : PageParameters
    {
        public string Text { get; set; }
        public string LaboratoryWorkId { get; set; }
        public string OwnerId { get; set; }
    }

    public class RoleParameters : PageParameters
    {
        public string Text { get; set; }
    }

    public class RequirementParameters : PageParameters
    {
        public string Text { get; set; }
        public string LaboratoryWorkId { get; set; }
    }

    public class LaboratoryWorkParameters : PageParameters
    {
        public string Text { get; set; }
        public string OwnerId { get; set; }
        public string DisciplineId { get; set; }
    }

    public class GroupParameters : PageParameters
    {
        public string Text { get; set; }
    }

    public class FileParametres : PageParameters
    {
        public string Text { get; set; }
        public string OwnerId { get; set; }
        public string RequirementId { get; set; }
        public string SolutionId { get; set; }
    }

    public class DisciplineParametres : PageParameters
    {
        public string Text { get; set; }
        public string OwnerId { get; set; }
    }

    public class DisciplineGroupTeacherParameters : PageParameters
    {
        public string Text { get; set; }
        public string DisciplineId { get; set; }
        public string GroupId { get; set; }
        public string TeacherId { get; set; }
    }
}
