using Microsoft.Toolkit.Uwp.UI.Helpers;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Chronicle
{   
    public sealed partial class MainPage : Page
    {
        readonly DispatcherTimer _timer;
        StorageFile _file;
        private bool _hasChanges;
        private Color _lightColor = Color.FromArgb(255, 0, 0, 0);
        Color _darkColor = Color.FromArgb(255, 255, 213, 46);

        public MainPage()
        {
            InitializeComponent();

            ThemeListener Listener = new ThemeListener();
            Listener.ThemeChanged += Listener_ThemeChanged;

            _timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Listener_ThemeChanged(ThemeListener sender)
        {
            SetColor(sender.CurrentTheme);
        }

        private void SetColor(ApplicationTheme theme)
        {
            ITextCharacterFormat textCharacterFormat = TextContent.Document.GetDefaultCharacterFormat();
            textCharacterFormat.ForegroundColor = theme == ApplicationTheme.Dark ? _darkColor : _lightColor;
            TextContent.Document.SetDefaultCharacterFormat(textCharacterFormat);
        }

        public async Task LoadText()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            bool initText = false;
            try
            {
                StorageFile file = await storageFolder.GetFileAsync("chronicle.txt");
            }
            catch
            {
                initText = true;
            }

            _file = await storageFolder.CreateFileAsync("chronicle.txt", CreationCollisionOption.OpenIfExists);
            string text = await FileIO.ReadTextAsync(_file);

            if (initText && string.IsNullOrEmpty(text))
            {
                text += DateTime.Now.ToLongDateString();
                text += Environment.NewLine;
                text += Environment.NewLine;
                text += "🤗 Welcome to Chronicle";
                text += Environment.NewLine;
                text += Environment.NewLine;
                text += "Chronicle saves everything you type automatically, that is why you don't need any buttons or menus.";
                text += Environment.NewLine;
                text += Environment.NewLine;
                text += "This app uses \"Cascadia Code\" font which gives you awesome stuff like ->, -->, |> and =>";
                text += Environment.NewLine;
                text += Environment.NewLine;
                text += "You can also press F5 to insert the current date. Have have fun, delete this message and start writing your chronicles!";
            }

            SetColor(Application.Current.RequestedTheme);
            TextContent.Document.SetText(TextSetOptions.None, text);
            TextContent.Document.Selection.SetRange(text.Length, text.Length);
        }
        private async void Timer_Tick(object sender, object e)
        {
            if (_hasChanges)
            {
                TextContent.Document.GetText(Windows.UI.Text.TextGetOptions.UseLf, out string text);
                await FileIO.WriteTextAsync(_file, text);
                _hasChanges = false;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _ = LoadText();
        }

        private void TextContent_TextChanged(object sender, RoutedEventArgs e)
        {
            _hasChanges = true;
        }

        private void TextContent_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.F5)
            {
                TextContent.Document.Selection.SetText(Windows.UI.Text.TextSetOptions.None, DateTime.Now.ToLongDateString());                
                TextContent.Document.Selection.SetRange(TextContent.Document.Selection.EndPosition, TextContent.Document.Selection.EndPosition);
                _hasChanges = true;
            }
        }

        private void TextContent_Paste(object sender, TextControlPasteEventArgs e)
        {
            SetColor(Application.Current.RequestedTheme);
        }
    }
}
