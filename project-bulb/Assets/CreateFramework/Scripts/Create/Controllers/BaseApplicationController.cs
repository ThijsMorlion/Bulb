using System.ComponentModel;
using UnityEngine;

namespace Create.Controllers
{
    public class BaseApplicationController<T> : MonoBehaviour, INotifyPropertyChanged where T : struct
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public T PreviousPage;
        protected T _currentPage;
        public T CurrentPage
        {
            get
            {
                if (!Application.isPlaying)
                {
                    return InEditorVisiblePage;
                }

                return _currentPage;
            }
            set
            {
                if (value.Equals(_currentPage))
                    return;

                PreviousPage = _currentPage;
                _currentPage = value;
                RaisePropertyChanged("CurrentPage");
            }
        }

        [SerializeField]
        public T InEditorVisiblePage;

        protected virtual void RaisePropertyChanged(string v)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(v));
        }
    }
}