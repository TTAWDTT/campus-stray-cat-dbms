namespace CampusStrayCatSystem.Data
{
    // 引用存在性校验接口，统一校验各实体引用的外键是否存在
    public interface IReferenceCheckRepository
    {
        Task<bool> VolunteerExists(string volunteerId);  // 判断志愿者 ID 在 VOL_VOLUNTEERS 表中是否存在
        Task<string?> GetVolunteerUserId(string volunteerId);
        Task<bool> ServicePointExists(string pointId);   // 判断投喂点 ID 在 MAP_SERVICEPOINTS 表中是否存在
        Task<bool> CatExists(string catId);              // 判断猫咪 ID 在 CAT_CATS 表中是否存在
        Task<bool> UserExists(string userId);            // 判断用户 ID 在 SYS_USERS 表中是否存在
    }
}
