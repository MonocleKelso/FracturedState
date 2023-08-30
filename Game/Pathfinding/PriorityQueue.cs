using System;
using System.Collections.Generic;

namespace FracturedState.Game.Nav
{
    public class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> data;

        public PriorityQueue()
        {
            data = new List<T>();
        }

        /// <summary>
        /// Adds the item to the queue if it doesn't already exist and sorts the queue.
        /// </summary>
        public void Enqueue(T item)
        {
            data.Add(item);
            int childIndex = data.Count - 1;
            while (childIndex > 0)
            {
                int parentIndex = (childIndex - 1) / 2;
                if (data[childIndex].CompareTo(data[parentIndex]) >= 0)
                    break;

                T tmp = data[childIndex];
                data[childIndex] = data[parentIndex];
                data[parentIndex] = tmp;
                childIndex = parentIndex;
            }
        }

        /// <summary>
        /// Removes and returns the item in the queue with the lowest priority.
        /// </summary>
        public T Dequeue()
        {
            int lastIndex = data.Count - 1;
            T frontItem = data[0];
            data[0] = data[lastIndex];
            data.RemoveAt(lastIndex--);

            int parentIndex = 0;
            while (true)
            {
                int childIndex = parentIndex * 2 + 1;
                if (childIndex > lastIndex)
                    break;

                int rightChild = childIndex + 1;
                if (rightChild <= lastIndex && data[rightChild].CompareTo(data[childIndex]) < 0)
                    childIndex = rightChild;

                T tmp = data[parentIndex];
                data[parentIndex] = data[childIndex];
                data[childIndex] = tmp;
                parentIndex = childIndex;
            }
            return frontItem;
        }

        /// <summary>
        /// Returns the first item in the queue without removing it.
        /// </summary>
        public T Peek()
        {
            return data[0];
        }

        /// <summary>
        /// Updates the queue based on the new priority of the item passed in.
        /// This method removes the item and requeues it.
        /// </summary>
        public void ChangePriority(T item)
        {
            data.Remove(item);
            Enqueue(item);
        }

        /// <summary>
        /// Removes the given item from the queue and re-orders the queue
        /// </summary>
        public void RemoveReorder(T item)
        {
            data.Remove(item);
            if (!IsEmpty())
            {
                T forceUpdate = Dequeue();
                Enqueue(forceUpdate);
            }
        }

        public bool IsEmpty()
        {
            return data.Count <= 0;
        }
    }
}