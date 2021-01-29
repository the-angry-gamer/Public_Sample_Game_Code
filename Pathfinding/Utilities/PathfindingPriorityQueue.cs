using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PathFindingAsteria
{

    public class PathfindingPriorityQueue<T> : IEnumerator, IEnumerable where T : IComparable<T> 
    {
        // List of items in our queue
        List<T> data;

        /// <summary>
        ///     The number of items currently in the queue
        /// </summary>
        public int Count { get { return data.Count; } }

        // constructor
        public PathfindingPriorityQueue()
        {
            this.data = new List<T>();
        }

        /// <summary>
        ///     Add an item to the queue and sort using a min binary heap
        /// </summary>
        /// <param name="item"> The item to add to the queue </param>
        public void Enqueue(T item)
        {
            // add the item to the end of the data List
            data.Add(item);

            // start at the last position in the heap
            int childindex = data.Count - 1;

            // sort using a min binary heap
            while (childindex > 0)
            {
                // find the parent position in the heap
                int parentindex = (childindex - 1) / 2;

                // if parent and child are already sorted, stop sorting
                if (data[childindex].CompareTo(data[parentindex]) >= 0)
                {
                    break;
                }

                // ... otherwise, swap parent and child
                T tmp = data[childindex];
                data[childindex] = data[parentindex];
                data[parentindex] = tmp;

                // move up one level in the heap and repeat until sorted
                childindex = parentindex;

            }
        }

        /// <summary>
        ///     Remove an item from queue and keep it sorted using a min binary heap
        /// </summary>
        /// <returns>  
        ///     The first item from the list.
        /// </returns>
        public T Dequeue()
        {
            // get the index for the last item
            int lastindex = data.Count - 1;

            // store the first item in the List in a variable
            T frontItem = data[0];

            // replace the first item with the last item
            data[0] = data[lastindex];

            // shorten the queue and remove the last position 
            data.RemoveAt(lastindex);

            // decrement our item count
            lastindex--;

            // start at the beginning of the queue to sort the binary heap
            int parentindex = 0;

            // sort using min binary heap
            while (true)
            {
                // choose the left child
                int childindex = parentindex * 2 + 1;

                // if there is no left child, stop sorting
                if (childindex > lastindex)
                {
                    break;
                }

                // the right child
                int rightchild = childindex + 1;

                // if the value of the right child is less than the left child, switch to the right branch of the heap
                if (rightchild <= lastindex && data[rightchild].CompareTo(data[childindex]) < 0)
                {
                    childindex = rightchild;
                }

                // if the parent and child are already sorted, then stop sorting
                if (data[parentindex].CompareTo(data[childindex]) <= 0)
                {
                    break;
                }

                // if not, then swap the parent and child
                T tmp = data[parentindex];
                data[parentindex] = data[childindex];
                data[childindex] = tmp;

                // move down the heap onto the child's level and repeat until sorted
                parentindex = childindex;

            }

            // return the original first item
            return frontItem;
        }

        /// <summary>
        ///     Look at the first item without dequeuing it
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            T frontItem = data[0];
            return frontItem;
        }

        /// <summary>
        ///     Peek at the specified item. 
        /// </summary>
        /// <param name="i"> The item number to peek it </param>
        /// <returns>
        ///     The specified item if i is within the array count.
        ///     If i is out of the count, it returns the first item
        /// </returns>
        public T Peek(int i)
        {
            if ( i > data.Count )
            {
                i = 0;
            }
            T frontItem = data[i];
            return frontItem;
        }

        /// <summary>
        ///     Determine if the specified item is in the list.
        /// </summary>
        /// <param name="item"> The item to check   </param>
        /// <returns>
        ///     A bool of inclusion
        /// </returns>
        public bool Contains(T item)
        {
            return data.Contains(item);
        }

        /// <summary>
        ///     Turn the queue into a list type
        /// </summary>
        /// <returns>
        ///     A list type of all the queue items
        /// </returns>
        public List<T> ToList()
        {
            return data;
        }


        #region Enumeration
        //https://docs.microsoft.com/en-us/troubleshoot/dotnet/csharp/make-class-foreach-statement


        int position = -1;

        //IEnumerator and IEnumerable require these methods.
        public IEnumerator GetEnumerator()
        {
            return (IEnumerator)this;
        }
        //IEnumerator
        public bool MoveNext()
        {
            position++;
            return (position < data.Count);
        }
        //IEnumerable
        public void Reset()
        {
            position = 0;
        }
        //IEnumerable
        public object Current
        {
            get { return data[position]; }
        }
        #endregion

    }

}