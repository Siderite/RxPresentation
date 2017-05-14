using System;
using System.Reactive.Subjects;
using System.Windows.Input;

namespace DictionaryLookup3
{
    public class ObservableCommand : IObservable<object>, IDisposable, ICommand
    {
        private readonly Subject<object> _subject;

        public ObservableCommand()
        {
            _subject = new Subject<object>();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _subject.OnNext(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Dispose()
        {
            _subject?.Dispose();
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            return _subject.Subscribe(observer);
        }
    }
}