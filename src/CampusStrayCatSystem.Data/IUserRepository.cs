namespace CampusStrayCatSystem.Data
{
    public interface IUserRepository
    {
        Task<bool> Exists(string userId);
    }
}
