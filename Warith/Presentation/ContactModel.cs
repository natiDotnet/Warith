namespace Warith.Presentation;

public partial record ContactModel
{
    public ContactModel()
    {
        Title = "Contact Us";
    }

    public string Title { get; }
}
