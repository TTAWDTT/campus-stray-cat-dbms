using System;

namespace CampusStrayCatSystem.Models.DTOs
{
    public class BlacklistStatusDto
    {
        public string UserId { get; set; }
        public bool IsBlacklisted { get; set; }
        public string BlacklistId { get; set; }
        public string ReasonType { get; set; }
        public string ReasonDetail { get; set; }
        public DateTime? BlacklistedAt { get; set; }
    }
}