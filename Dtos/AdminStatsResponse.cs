namespace WebApi.NetCore.Dtos;

public class AdminStatsResponse
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int AdminUsers { get; set; }
    public int ManagerUsers { get; set; }
    public int StandardUsers { get; set; }
}
