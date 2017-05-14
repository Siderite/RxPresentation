using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using DictionaryLookup2.Web;

namespace DictionaryLookup2
{
    public partial class MainWindow : Window
    {
        private readonly BindingList<string> _debug;

        public MainWindow()
        {
            InitializeComponent();

            _debug = new BindingList<string>();
            LboxDebug.ItemsSource = _debug;

            var changes = Observable.FromEventPattern(TxtFilter, nameof(TextBox.TextChanged))
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Merge(Observable.FromEventPattern(BtnSearch, nameof(Button.Click)))
                .ObserveOnDispatcher()
                .Select(_ => TxtFilter.Text) //((TextBox)_.Sender).Text
                .DistinctUntilChanged();
            var res = changes
                .SelectMany(text =>
                {
                    var retries = 0;
                    return Observable.Defer(() => {
                        if (retries > 0)
                        {
                            write($"Retrying ({retries})");
                        }
                        retries++;
                        write($"Getting words for {text}");
                        return new DictServiceSoapClient()
                            .MatchInDictAsync("wn", text, "prefix")
                            .ContinueWith(t =>
                            {
                                write($"Done for {text}");
                                return t.Result;
                            })
                        .ToObservable();
                    })
                    .Timeout(TimeSpan.FromMilliseconds(600))
                    .Retry(3)
                    .Select(wordItems => wordItems.Select(wi => wi.Word).ToArray())
                    .Catch<string[], CommunicationException>(
                        ex => Observable.Return(new[] { $"Communication error: {ex.Message}" }))
                    .Catch<string[], TimeoutException>(ex => Observable.Return(new[] { $"Timeout: {ex.Message}" }))
                    .Catch<string[], AggregateException>(
                        ex =>
                            Observable.Return(
                                ex.InnerExceptions.Select(ex_ => $"Aggregate errors: {ex_.Message}").ToArray()));
                }
                );
            res.ObserveOnDispatcher()
                .Subscribe(words => LboxWords.ItemsSource = words);
        }

        private void write(string message)
        {
            Dispatcher.Invoke(() => _debug.Insert(0, $"{DateTime.Now:HH:mm:ss.ffff}  {message}"));
        }
    }
}