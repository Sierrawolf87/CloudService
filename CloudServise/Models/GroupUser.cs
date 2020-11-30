using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CloudService_API.Models
{
    public class GroupUser
    {

        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public Guid GroupId { get; set; }
        [ForeignKey("GroupId")]
        public Group Group { get; set; }

        public GroupUser() {}
        public GroupUser(Guid userId, Guid groupId)
        {
            UserId = userId;
            groupId = groupId;
        }
        public GroupUserDTO ToGroupUserDto()
        {
            return new GroupUserDTO(Id, UserId, GroupId);
        }
    }

    public class GroupUserDTO
    {
        public GroupUserDTO(Guid id, Guid userId, Guid groupId)
        {
            Id = id;
            UserId = userId;
            GroupId = groupId;
        }

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }
    }
}
