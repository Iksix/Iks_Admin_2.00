namespace Iks_Admin;

public class Reason
{
    public string Title { get; set; }
    public int? BanTime { get; set; }

    public Reason(string Title, int? BanTime = null)
    {
        this.Title = Title;
        this.BanTime = BanTime;
    }
}