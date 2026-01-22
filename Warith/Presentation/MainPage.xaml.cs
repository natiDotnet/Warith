namespace Warith.Presentation;

public sealed partial class MainPage : Page
{
    private readonly List<string> _willSuggestions = new()
    {
        "2/3", "1/2", "1/3", "1/4", "1/5", "1/6", "1/7", "1/8", "1/9", "1/10"
    };

    public MainPage()
    {
        this.InitializeComponent();
        
        // Set ItemsSource for AutoSuggestBox controls
        eradyya1.ItemsSource = _willSuggestions;
        eradyya2.ItemsSource = _willSuggestions;
        eradyya3.ItemsSource = _willSuggestions;
        
        // Gender change handlers
        kj.Checked += OnMaleSelected;
        lll.Checked += OnFemaleSelected;
        
        // Checkbox mutual exclusion handlers
        grouphalat_5.Checked += OnMonaskhaChecked;
        grouphalat_1.Checked += OnWasyawagbaChecked;
        grouphalat_1.Unchecked += OnWasyawagbaUnchecked;
        grouphalat_2.Checked += OnHamlChecked;
        grouphalat_4.Checked += OnMafkoodChecked;
        
        // Initialize with male selected (default)
        ShowMaleFields();
    }

    private void OnWillTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        // Only filter suggestions if user is typing
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var query = sender.Text.ToLower();
            if (string.IsNullOrWhiteSpace(query))
            {
                sender.ItemsSource = _willSuggestions;
            }
            else
            {
                sender.ItemsSource = _willSuggestions.Where(s => s.Contains(query)).ToList();
            }
        }

        // Check will validation
        ValidateWills();
    }

    private void OnMaleSelected(object sender, RoutedEventArgs e)
    {
        ShowMaleFields();
    }

    private void OnFemaleSelected(object sender, RoutedEventArgs e)
    {
        ShowFemaleFields();
    }

    private void ShowMaleFields()
    {
        // Show wife panel
        WifePanel.Visibility = Visibility.Visible;
        
        // Hide husband panel
        HusbandPanel.Visibility = Visibility.Collapsed;
    }

    private void ShowFemaleFields()
    {
        // Hide wife panel
        WifePanel.Visibility = Visibility.Collapsed;
        
        // Show husband panel
        HusbandPanel.Visibility = Visibility.Visible;
        zawg1.Visibility = Visibility.Visible;
    }

    private void OnMonaskhaChecked(object sender, RoutedEventArgs e)
    {
        // Uncheck grouphalat_2 and grouphalat_4
        grouphalat_2.IsChecked = false;
        grouphalat_4.IsChecked = false;
    }

    private void OnWasyawagbaChecked(object sender, RoutedEventArgs e)
    {
        // Uncheck grouphalat_2 and grouphalat_4
        grouphalat_2.IsChecked = false;
        grouphalat_4.IsChecked = false;
        // Show egaza22
        egaza22.Visibility = Visibility.Visible;
    }

    private void OnWasyawagbaUnchecked(object sender, RoutedEventArgs e)
    {
        // Hide egaza22
        egaza22.Visibility = Visibility.Collapsed;
    }

    private void OnHamlChecked(object sender, RoutedEventArgs e)
    {
        // Uncheck grouphalat_1, grouphalat_4, and grouphalat_5
        grouphalat_1.IsChecked = false;
        grouphalat_4.IsChecked = false;
        grouphalat_5.IsChecked = false;
        egaza22.Visibility = Visibility.Collapsed;
    }

    private void OnMafkoodChecked(object sender, RoutedEventArgs e)
    {
        // Uncheck grouphalat_1, grouphalat_2, and grouphalat_5
        grouphalat_1.IsChecked = false;
        grouphalat_2.IsChecked = false;
        grouphalat_5.IsChecked = false;
        egaza22.Visibility = Visibility.Collapsed;
    }

    private void ValidateWills()
    {
        // Check if any will exceeds 1/3 and show egaza accordingly
        var will1 = ParseFraction(eradyya1.Text);
        var will2 = ParseFraction(eradyya2.Text);
        var will3 = ParseFraction(eradyya3.Text);

        var total = will1 + will2 + will3;

        if (total > (1m / 3m))
        {
            egaza.Visibility = Visibility.Visible;
        }
        else
        {
            egaza.Visibility = Visibility.Collapsed;
        }
    }

    private decimal ParseFraction(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return 0m;

        var parts = input.Trim().Split('/');
        if (parts.Length == 2 && 
            decimal.TryParse(parts[0], out var numerator) && 
            decimal.TryParse(parts[1], out var denominator) && 
            denominator != 0)
        {
            return numerator / denominator;
        }

        return 0m;
    }

    private void OnNumberOnlyBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        var newText = args.NewText;
        if (string.IsNullOrEmpty(newText))
            return;

        // Allow digits and only one decimal point
        if (!System.Text.RegularExpressions.Regex.IsMatch(newText, @"^\d*\.?\d*$"))
        {
            args.Cancel = true;
        }
    }
}
