using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using VRC.SDKBase;
using VRC.Udon;

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class ByteArrQueue : UdonSharpBehaviour
    {
        private byte[][] _array;
        private int _head; // First valid element in the queue.
        private int _tail; // Last valid element in the queue.
        private int _size; // Number of elements.
        private int _version;

        private const int MinimumGrow = 10;

        public int Count => _size;
        public int Version => _version;

        [Obsolete("Use Count Property.")]
        public int GetCount() => _size;
        [Obsolete("Use Version Property.")]
        public int GetVersion() => _version;

        

        // Removes all Objects from the queue.
        public void Clear()
        {
            /*if (_size != 0)
            {
                if (_head < _tail)
                {
                    Array.Clear(_array, _head, _size);
                }
                else
                {
                    Array.Clear(_array, _head, _array.Length - _head);
                    Array.Clear(_array, 0, _tail);
                }

                _size = 0;
            }*/
            _size = 0;
            _head = 0;
            _tail = 0;
            _version++;
        }

        // Adds obj to the tail of the queue.
        public void Enqueue(byte[] obj)
        {
            if (obj == null)
            {
                Debug.LogError("obj is null!");
            }
            if (_array == null)
            {
                SetCapacity(MinimumGrow);
            }
            else if (_size == _array.Length)
            {
                SetCapacity(_array.Length + MinimumGrow);
            }

            _array[_tail] = obj;
            _tail = (_tail + 1) % _array.Length;
            _size++;
            _version++;
        }

        // Removes the int at the head of the queue and returns it. If the queue
        // is empty, this method returns null.
        public  byte[]  Dequeue()
        {
            if (_size == 0)
                Debug.LogError("Queue is empty!");

            byte[] removed = _array[_head];
            _array[_head] = null;
            _head = (_head + 1) % _array.Length;
            _size--;
            _version++;
            return removed;
        }

        // Returns the int at the head of the queue. The int remains in the
        // queue. If the queue is empty, this method returns null.
        public  byte[]  Peek()
        {
            if (_size == 0)
            {
                //throw new InvalidOperationException();
                return null;
            }

            return _array[_head];
        }



        // PRIVATE Grows or shrinks the buffer to hold capacity ints. Capacity
        // must be >= _size.
        public void SetCapacity(int capacity)
        {
            byte[][] newArray = new byte[capacity][];
            if (_size == 0)
            {

            }
            else if (_head < _tail)
            {
                Debug.LogWarning("SetCapacity: _head < _tail");
                Array.Copy(_array, _head, newArray, 0, _size);
            }
            else
            {
                Debug.LogWarning("SetCapacity: _head >= _tail");
                int toHead = _array.Length - _head;
                Array.Copy(_array, _head, newArray, 0, toHead);
                Array.Copy(_array, 0, newArray, toHead, _tail);
            }

            _array = newArray;
            _head = 0;
            _tail = _size == capacity ? 0 : _size;
            _version++;
        }
    }