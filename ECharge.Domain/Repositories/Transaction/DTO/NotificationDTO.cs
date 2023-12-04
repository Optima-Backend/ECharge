using System;
using AutoMapper;
using ECharge.Domain.Entities;
using ECharge.Domain.Mappings;

namespace ECharge.Domain.Repositories.Transaction.DTO
{
    public class NotificationDTO : IMapFrom<Notification>
    {
        public int Id { get; set; }

        public string SessionId { get; set; }

        public string UserId { get; set; }

        public string Message { get; set; }

        public string Title { get; set; }

        public bool HasSeen { get; set; }

        public bool IsCableStatus { get; set; }

        public string FCMToken { get; set; }

        public DateTime CreatedDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Notification, NotificationDTO>();
        }
    }
}

