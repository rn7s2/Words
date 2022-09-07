using System;
using System.Diagnostics;
using System.Web;

namespace Words;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void PreviousBtn_Clicked(object sender, EventArgs e)
    {
        (BindingContext as MainViewModel).SelectedDate = (BindingContext as MainViewModel).SelectedDate.AddDays(-1);
    }

    private void NextBtn_Clicked(object sender, EventArgs e)
    {
        (BindingContext as MainViewModel).SelectedDate = (BindingContext as MainViewModel).SelectedDate.AddDays(1);
    }

    private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        var word = (e.Item as Words);
        Application.Current.Dispatcher.DispatchAsync(async () =>
        {
            bool answer = await DisplayAlert(word.Word, word.Description, "Look up", "OK");
            if (answer)
                await Browser.Default.OpenAsync("https://www.collinsdictionary.com/dictionary/english/" + HttpUtility.UrlEncode(word.Word), BrowserLaunchMode.SystemPreferred);
        });
    }
}
