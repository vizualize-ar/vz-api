using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VZ.Reviews.Services.Models.Data
{
    public class ReviewImageList : IList<ReviewImage>
    {
        private List<ReviewImage> _list = new List<ReviewImage>();

        public ReviewImage this[int index] { get => _list[index]; set => _list[index] = value; }

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public void Add(ReviewImage item)
        {
            if (!Contains(item))
            {
                _list.Add(item);
            }
        }

        public void Clear()
        {
            _list.Clear();
        }

        /// <summary>
        /// Checks if a ReviewImage exists with the same name. Matches by name only.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(ReviewImage item)
        {
            return _list.Any(i => i.name == item.name);
        }

        public void CopyTo(ReviewImage[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ReviewImage> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public int IndexOf(ReviewImage item)
        {
            return _list.FindIndex(i => i.name == item.name);
        }

        public void Insert(int index, ReviewImage item)
        {
            _list.Insert(index, item);
        }

        public bool Remove(ReviewImage item)
        {
            int index = IndexOf(item);
            if (index > -1)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}
