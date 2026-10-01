namespace NovinApp.Server.Services
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string? NationalCode { get; }
        string? FullName { get; }
        string? Role { get; }
        int? StudentId { get; }
        int? ConsultantId { get; }
        int? SchoolId { get; }
        bool IsAuthenticated { get; }
        bool IsSystemAdmin { get; }
        bool IsSchoolManager { get; }
        bool IsConsultant { get; }
        bool IsStudent { get; }
    }
}
