namespace Warith.Presentation;

public partial record AboutModel
{
    public AboutModel()
    {
        Title = "About Warith";
    }

    public string Title { get; }
}
