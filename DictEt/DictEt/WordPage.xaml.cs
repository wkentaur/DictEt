using DictEtLogic;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DictEt
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WordPage : ContentPage
    {
        private string CurrentWord;
        //fire TextChanged event
        private bool FireTextChanged = true;

        public WordPage(string sWord = "")
        {           
            InitializeComponent();

            WordList.ItemsSource = Core.myDict.Words;
            BottomLabel.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => {
                    Device.OpenUri(new Uri("https://et.m.wiktionary.org/"));
                })
            });

            if (sWord.Length > 0)
                ShowWordDesc(sWord);
        }

        //FIXME can I avoid    async void?
        public async void ShowWordDesc(string sWord)
        {           
            try
            {
                CurrentWord = sWord;
                WordBrowser.IsVisible = true;
                WebProgress.IsVisible = true;
                WordList.IsVisible = false;
                BottomLabel.IsVisible = false;
                var htmlSource = new HtmlWebViewSource();

                htmlSource.BaseUrl = "https://et.wiktionary.org";
                Core.wordDescCancelTS.Cancel();
                Core.wordDescCancelTS = new CancellationTokenSource();
                Dict resDict = await Core.GetDescAsync(sWord, Core.wordDescCancelTS.Token);
                
                #pragma warning disable 4014
                //fire and forget
                Task.Run(async () =>
                {
                    await WebProgress.ProgressTo(0.9, 900, Easing.CubicInOut);
                }).ConfigureAwait(false);
                #pragma warning restore 4014

                htmlSource.Html = Core.AddHtmlHeaderFooter( resDict.WordDescs[sWord] );
                FireTextChanged = false;
                    SearchWord.Text = CurrentWord;
                FireTextChanged = true;
                WordBrowser.Source = htmlSource;
                WebProgress.IsVisible = false;
            }
            catch (OperationCanceledException)
            {  }
            catch (Exception ex)
            {
                await DisplayAlert("msg", ex.Message, "cancel");
            }
        }


        private void SearchWord_Focused(object sender, FocusEventArgs e)
        {
            WordList.IsVisible = true;
            WordBrowser.IsVisible = false;
        }

        private async void SearchWord_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FireTextChanged)
            {
                if (e.NewTextValue == "")
                {
                    Core.EmptyWordLookup();
                }
                else if (SearchWord.Text.Length > 0)
                {
                    try
                    {
                        Core.lookupCancelTS.Cancel();
                        Core.lookupCancelTS = new CancellationTokenSource();
                        Dict resDict = await Core.GetWordsAsync(SearchWord.Text, Core.lookupCancelTS.Token);

                    }
                    catch (OperationCanceledException)
                    { }
                    catch (Exception ex)
                    {
                        await DisplayAlert("msg", ex.Message, "cancel");
                    }

                }

            }
        }

        private async void SearchWord_SearchButtonPressed(object sender, EventArgs e)
        {
            try
            {
                Core.lookupCancelTS.Cancel();
                Core.lookupCancelTS = new CancellationTokenSource();
                Dict resDict = await Core.GetWordsAsync(SearchWord.Text, Core.lookupCancelTS.Token);

            }
            catch (OperationCanceledException)
            { }
            catch (Exception ex)
            {
                await DisplayAlert("msg", ex.Message, "cancel");
            }
        }

        private async void WordList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (WordList.SelectedItem != null)
            {
                await OpenNewWordPage(WordList.SelectedItem.ToString());
            }
        }

        private async void WordBrowser_Navigating(object sender, WebNavigatingEventArgs e)
        {
            if (e.Url.StartsWith("dict:"))
            {
                string wordUrl = e.Url;
                wordUrl = wordUrl.Replace("dict:", "");
                wordUrl = wordUrl.Replace("_", " ");
                e.Cancel = true;
                await OpenNewWordPage(wordUrl);
            }

            WebProgress.IsVisible = true;
        }
        private void WordBrowser_Navigated(object sender, WebNavigatedEventArgs e)
        {
            WebProgress.IsVisible = false;
        }
        private async Task OpenNewWordPage(string newWord)
        {
            //old page
            FireTextChanged = false;
                SearchWord.Text = CurrentWord;
            FireTextChanged = true;

            WordBrowser.IsVisible = true;
            WordList.IsVisible = false;
            WordList.SelectedItem = null;
            Core.myDict.Words.Clear();

            var newWordPage = new WordPage(newWord);
            await Navigation.PushModalAsync(newWordPage, false);
        }

        protected override bool OnBackButtonPressed()
        {
            if (Navigation.NavigationStack.Count == 1 && Device.RuntimePlatform == Device.Android)
                DependencyService.Get<IAndroidMethods>().CloseApp();

            return base.OnBackButtonPressed();
        }

    }
}
