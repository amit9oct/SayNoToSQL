
using System;
using System.Linq;

namespace SNTSBackend.Utils
{
    /// <summary>
    ///     N-Dimensional ordered tuple for clonable objects
    /// </summary>
    public class NTuples
    {
        private object[] _list;
        private int _currentIndex;

        /// <summary>
        ///     Has the deep copy of the tuples
        /// </summary>
        public object[] Tuple
        {
            get
            {
                return _list;
            }
        }

        public int MaxSize { get; private set; }

        public int TupleSize { get { return _currentIndex; } }

        public NTuples(int n)
        {
            MaxSize = n;
            _list = new object[MaxSize];
            _currentIndex = 0;
        }

        public void AddToTuple(object obj)
        {
            if (_currentIndex >= MaxSize)
            {
                var tempList = new object[MaxSize * 2];
                for(int i = 0; i < MaxSize; i++)
                {
                    tempList[i] = _list[i];
                }
                MaxSize = MaxSize * 2;
                _list = tempList;

            }
            _list[_currentIndex] = obj;
            _currentIndex++; 
        }

        public override string ToString() {
            return $"({string.Join(", ",_list.Cast<object>())})";
        }
    }
}
