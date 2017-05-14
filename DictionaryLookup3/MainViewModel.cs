using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Windows.Input;
using DictionaryLookup3.Annotations;
using DictionaryLookup3.Web;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Reactive.Concurrency;
using System.Windows.Data;
using System.Collections;

namespace DictionaryLookup3
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _filter;
        private string[] _words;

        public BindingList<string> Debug { get; private set; }
        public ICommand SearchCommand { get; private set; }

        public MainViewModel()
        {
            Debug = new BindingList<string>();
            BindingOperations.EnableCollectionSynchronization(Debug, ((ICollection)Debug).SyncRoot);

            var command = new ObservableCommand();
            SearchCommand = command;

            Observable.FromEventPattern<PropertyChangedEventArgs>(this, nameof(INotifyPropertyChanged.PropertyChanged))
                .Where(o => o.EventArgs.PropertyName == nameof(Filter))
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Merge(command)
                .Select(_ => Filter)
                .DistinctUntilChanged()
                .SelectMany(text =>
                {
                    var retries = 0;
                    return Observable.Defer(() =>
                    {
                        if (retries>0)
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
                )
                .Subscribe(words => Words = words);
        }

        private void write(string message)
        {
            Debug.Insert(0, $"{DateTime.Now:HH:mm:ss.ffff}  {message}");
        }

        public string Filter
        {
            get { return _filter; }
            set
            {
                if (_filter == value) return;
                _filter = value;
                OnPropertyChanged(nameof(Filter));
            }
        }

        public string[] Words
        {
            get { return _words; }
            set
            {
                if (_words == value) return;
                _words = value;
                OnPropertyChanged(nameof(Words));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}