using System.Net;
using Warith.Services;

namespace Warith.Presentation;

public partial record MainModel
{
    private INavigator _navigator;
    private readonly IApiCallService _apiCallService;

    public MainModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        INavigator navigator,
        IApiCallService apiCallService)
    {
        _navigator = navigator;
        _apiCallService = apiCallService;
        Title = "Main";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";
    }

    public string? Title { get; }

    public IState<string> Name => State<string>.Value(this, () => string.Empty);
    public IListFeed<string> Wills => State
        .Value(this, () => WillRules.AllowedFractions.ToImmutableList())
        .AsListFeed();

    public ICommand Calculate => Command.Create(b => b.Execute((ct) => CalculateInheritance()));
    public IState<Inheritance> Inheritance => State<Inheritance>.Value(this, () => new Inheritance());

    public async Task GoToSecond()
    {
        var name = await Name;
        await _navigator.NavigateViewModelAsync<SecondModel>(this, data: new Entity(name!));
    }

    public async ValueTask CalculateInheritance()
    {
        var testData = Warith.Services.TestData.GetSeededFormData();
        var result = await _apiCallService.CalculateInheritanceAsync(testData);
        // TODO: Handle result (e.g. show in UI or navigate)
        System.Diagnostics.Debug.WriteLine(result.ToString());
    }

}
