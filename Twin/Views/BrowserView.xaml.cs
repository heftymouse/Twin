﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.ComponentModel;
using System.Net.Mime;
using System.Threading.Tasks;
using Twin.Controls;
using Twin.Core.ViewModels;
using Twin.Gemini;
using Twin.Helpers;
using Windows.ApplicationModel.DataTransfer;

namespace Twin.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BrowserView : Page
    {
        BrowserViewModel vm;
        Getter<XamlRoot> DialogRoot => new Getter<XamlRoot>(() => this.XamlRoot);

        public BrowserView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            vm = IocHelpers.InitViewModel<BrowserViewModel>(DialogRoot);
            vm.PropertyChanged += OnViewmodelPropertyChanged;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
            {
                await vm.NavigateCommand.ExecuteAsync(null);
            }
        }

        private void OnViewmodelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(vm.CurrentPage))
            {
                var contentType = new ContentType(vm.CurrentPage.Meta);
                if (contentType.MediaType == "text/gemini")
                {
                    contentBox.Inlines.Clear();
                    // this is dumbass
                    foreach (Inline i in GemtextRenderer.Render(vm.CurrentPage.Body, vm.Uri))
                    {
                        if (i.GetType() == typeof(Hyperlink))
                        {
                            var link = i as Hyperlink;
                            if (link.NavigateUri.Scheme == "gemini")
                            {
                                link.NavigateUri = null;
                            }
                            link.Click += OnLinkClicked;
                        }

                        contentBox.Inlines.Add(i);
                    }
                }
                else
                {
                    contentBox.Text = vm.CurrentPage.Body;
                }
            }
        }

        private async void OnUriChanged(object sender, Uri args)
        {
            vm.Uri = args;
            await vm.NavigateCommand.ExecuteAsync(null);
        }

        //private async Task GoToPage(Uri uri, bool addToHistory = true, bool clearFutureHistory = false)
        //{
        //    model.IsLoading = true;
        //    Uri oldUri = model.Uri;
        //    if (uri == oldUri)
        //    {
        //        addToHistory = false;
        //    }

        //    try
        //    {
        //        model.Uri = uri;

        //        GeminiResponse response = await Task.Run(() => GeminiRequest.RequestPageAsync(uri));
        //        switch (response.Type)
        //        {
        //            case GeminiResponseType.Input:
        //                var result = await GetPageInput(response.Meta, response.Code != 0);
        //                if (result == null) return;
        //                UriBuilder builder = new(uri);
        //                builder.Query = result;
        //                await GoToPage(builder.Uri);
        //                return;

        //            case GeminiResponseType.Redirect:
        //                await GoToPage(new Uri(response.Meta));
        //                return;

        //            case GeminiResponseType.TemporaryFailure:
        //                ShowErrorDialog(uri, response, true);
        //                model.IsLoading = false;
        //                model.Uri = oldUri;
        //                return;

        //            case GeminiResponseType.PermanentFailure:
        //                ShowErrorDialog(null, response, false);
        //                model.IsLoading = false;
        //                model.Uri = oldUri;
        //                return;
        //            case GeminiResponseType.ClientCertificateRequired:
        //                // not handling for now
        //                return;
        //        }

        //        contentBox.Inlines.Clear();
        //        var contentType = new ContentType(response.Meta);
        //        if (contentType.MediaType == "text/gemini")
        //        {
        //            // this is dumbass
        //            foreach (Inline i in GemtextRenderer.Render(response.Body, uri))
        //            {
        //                if (i.GetType() == typeof(Hyperlink))
        //                {
        //                    var link = i as Hyperlink;
        //                    if (link.NavigateUri.Scheme == "gemini")
        //                    {
        //                        link.NavigateUri = null;
        //                    }
        //                    link.Click += OnLinkClicked;
        //                }

        //                contentBox.Inlines.Add(i);
        //            }
        //        }
        //        else
        //        {
        //            contentBox.Text = response.Body;
        //        }

        //        if(clearFutureHistory)
        //        {
        //            model.History.ClearNextPages();
        //        }
        //        if (addToHistory)
        //        {
        //            model.History.Add(uri);
        //        }
        //        model.CurrentPage = response;
        //    }
        //    catch (Exception e)
        //    {
        //        model.Uri = oldUri;
        //        InternalErrorText.Text = $"{e.InnerException?.Message ?? e.Message}\n\n{e.InnerException?.StackTrace ?? e.StackTrace}";
        //        await InternalErrorDialog.ShowAsync();
        //    }
        //    model.IsLoading = false;
        //}

        private async void OnLinkClicked(Hyperlink sender, HyperlinkClickEventArgs e)
        {
            var uri = new Uri(ToolTipService.GetToolTip(sender) as string);
            if (uri.Scheme == "gemini")
            {
                vm.Uri = uri;
                await vm.NavigateCommand.ExecuteAsync(null);
            }
        }

        //private async Task<string> GetPageInput(string prompt, bool isSensitive)
        //{
        //    PasswordBox textBox = new()
        //    {
        //        PlaceholderText = prompt,
        //        PasswordRevealMode = isSensitive ? PasswordRevealMode.Peek : PasswordRevealMode.Visible
        //    };

        //    ContentDialog dialog = new()
        //    {
        //        XamlRoot = this.XamlRoot,
        //        Title = "This page is requesting user input",
        //        Content = textBox,
        //        PrimaryButtonText = "Submit",
        //        CloseButtonText = "Close"
        //    };

        //    var result = await dialog.ShowAsync();
        //    if (result == ContentDialogResult.Primary)
        //    {
        //        return textBox.Password;
        //    }
        //    return null;
        //}

        //    private async void ShowErrorDialog(Uri uri, GeminiResponse response, bool retry)
        //    {
        //        PageErrorText.Text = $"Server returned code {(int)response.Type}{response.Code}\n{response.Meta}";
        //        if (retry && uri != null)
        //        {
        //            PageErrorDialog.IsSecondaryButtonEnabled = true;
        //            PageErrorDialog.SecondaryButtonClick += async (sender, e) => await GoToPage(uri);
        //        }

        //        await PageErrorDialog.ShowAsync();
        //    }

        private async void ShowInfoDialog(object sender, RoutedEventArgs e)
        {
            //await aboutDialog.ShowAsync();
            MainWindow window = new();
            RootFrame rf = (App.Current as App).m_window.RootFrame;
            (App.Current as App).m_window.Content = null;
            window.Content = rf;
            window.Activate();
        }

        //    private void OnErrorTextCopy(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        //    {
        //        DataPackage data = new();
        //        data.SetData(StandardDataFormats.Text, InternalErrorText);
        //        Clipboard.SetContent(data);
        //    }
    }
}