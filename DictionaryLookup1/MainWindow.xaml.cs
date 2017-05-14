using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using DictionaryLookup1.Web;

namespace DictionaryLookup1
{
    public partial class MainWindow : Window
    {
        private readonly BindingList<string> _debug;
        private readonly object _filterLock = new object();
        private readonly Timer _timer;
        private string _lastText;
        private string _text;

        public MainWindow()
        {
            InitializeComponent();
            TxtFilter.TextChanged += TxtFilter_TextChanged;
            BtnSearch.Click += BtnSearch_OnClick;
            _timer = new Timer
            {
                AutoReset = false,
                Enabled = false,
                Interval = 500
            };
            _timer.Elapsed += _timer_Elapsed;
            _debug = new BindingList<string>();
            LboxDebug.ItemsSource = _debug;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            _timer?.Dispose();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string text;
            lock (_filterLock)
            {
                text = _text;
            }
            if (text == _lastText) return;
            _lastText = text;
            searchFor(text);
        }

        private void searchFor(string text, int retries = 0)
        {
            string[] words;
            try
            {
                var serv = new DictServiceSoapClient();
                write($"Getting words for {text}");
                var task = serv.MatchInDictAsync("wn", text, "prefix");
                Task.WaitAny(task, Task.Delay(TimeSpan.FromMilliseconds(600)));
                if (task.IsCompleted)
                {
                    var wordItems = task.Result;
                    words = wordItems.Select(wi => wi.Word).ToArray();
                }
                else
                {
                    words = new[] {"Timeout getting words"};
                }
                write($"Done for {text}");
            }
            catch (CommunicationException ex) when(ex.InnerException==null)
            {
                words = new[] {$"Communication error: {ex.Message}"};
            }
            catch (AggregateException ex)
            {
                words = ex.InnerExceptions.Select(ex_ => $"Aggregate errors: {ex_.Message}").ToArray();
            }
            catch (CommunicationException ex) when (ex.InnerException != null)
            {
                if (retries < 3)
                {
                    searchFor(text, retries + 1);
                    return;
                }
                words = new[] {$"Communication error w{ex.InnerException.Message}"};
            }
            catch (Exception ex)
            {
                words = new[] {$"Unexpected exception: {ex.Message}"};
            }
            Dispatcher.Invoke(() => LboxWords.ItemsSource = words);
        }

        private void write(string message)
        {
            Dispatcher.Invoke(() => _debug.Insert(0, $"{DateTime.Now:HH:mm:ss.ffff}  {message}"));
        }

        private void TxtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            _timer.Stop();
            lock (_filterLock)
            {
                _text = TxtFilter.Text;
            }
            _timer.Start();
        }

        private void BtnSearch_OnClick(object sender, RoutedEventArgs e)
        {
            searchFor(TxtFilter.Text);
        }
    }
}