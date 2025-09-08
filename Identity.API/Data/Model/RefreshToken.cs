namespace Identity.API.Data.Model;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public string UserId { get; set; }
    public DateTime ExpiryTime { get; set; }
    public AppUser AppUser { get; set; }
    
    public bool IsValid() => ExpiryTime > DateTime.Now;
}