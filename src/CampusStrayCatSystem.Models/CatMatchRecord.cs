namespace CampusStrayCatSystem.Models {
    public class CatMatchRecord {
        public string MatchID { get; set; } = string.Empty;
        public string? SourcePhotoID { get; set; }
        public string? CandidateCatID { get; set; }
        public decimal? SimilarityScore { get; set; }
        public int? RankNo { get; set; }
        public string ConfirmStatus { get; set; } = CatMatchStatusCodes.Pending;
        public string? ConfirmUserID { get; set; }
        public string? SourcePhotoUrl { get; set; }
        public string? CandidateCatName { get; set; }
        public string? CandidateArchiveStatus { get; set; }
        public string? CandidateAreaName { get; set; }
        public string? CandidatePrimaryPhotoUrl { get; set; }
    }

    public class ConfirmCatMatchRequest {
        [Utf8ByteLength(20, ErrorMessage = "确认状态不能超过数据库允许的 20 字节。")]
        public string? ConfirmStatus { get; set; }
    }
}
