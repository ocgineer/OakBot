using System.Collections.ObjectModel;

namespace OakBot.Model
{
    public class LimitedObservableCollection<T> : ObservableCollection<T>
    {
        #region Fields

        private int _limit;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a LimitedObserverableCollection with limited elements to use
        /// </summary>
        /// <param name="limit">Set the collection element T limit, default 1000 elements</param>
        public LimitedObservableCollection(int limit = 1000) : base()
        {
            _limit = limit;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add item T at the end of the Collection and trims the beginning if item T count is exceeding the CollectionLimit
        /// </summary>
        /// <param name="item">Item to be added to the collection</param>
        public void AddAndTrim(T item)
        {
            // Add item T at end of collection
            base.Add(item);
            
            // Remove item T at index 0 if exceeding limit
            if (base.Count > _limit)
            {
                base.RemoveAt(0);
                this.Trim();
            }

        }

        /// <summary>
        /// If limit is exceeded trim elements at first index untill equal to limit
        /// </summary>
        private void Trim()
        {
            if (base.Count > _limit)
            {
                int trimCount = base.Count - _limit;
                for (int i = 0; i < trimCount; i++)
                {
                    base.RemoveAt(0);
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get or Set Collection limit. Collection is trimmed on first <see cref="AddAndTrim(T)"/>
        /// </summary>
        public int Limit
        {
            get
            {
                return _limit;
            }
            set
            {
                if (value != _limit)
                    _limit = value;
            }
        }

        #endregion
    }
}
