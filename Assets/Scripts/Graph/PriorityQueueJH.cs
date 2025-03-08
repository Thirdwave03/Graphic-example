using System;
using System.Collections.Generic;
using System.Diagnostics;

public class PriorityQueueJH<T, TPriority>
{
    public T[] Tvals;
    public TPriority[] Tpriorities;
    public Comparer<TPriority> mComparer { get; set; }

    int index { get; set; }
    public int Count
    {
        get
        {
            return index;
        }
    }

    int Size { get; set; }

    public PriorityQueueJH(int initialSize = 8)
    {
        Size = initialSize;
        Tvals = new T[Size];
        Tpriorities = new TPriority[Size];
        index = 0;
        mComparer = Comparer<TPriority>.Default;
    }

    public PriorityQueueJH(Comparer<TPriority> comparer)
    {
        Size = 8;
        Tvals = new T[Size];
        Tpriorities = new TPriority[Size];
        index = 0;
        mComparer = comparer;
    }

    public void Enqueue(T val, TPriority priority)
    {
        if (index + 1 == Size)
        {
            ReAllocateSize();
        }
        Tpriorities[index] = priority;
        Tvals[index] = val;

        int currentIndex = index;
        ++index;
        while (currentIndex > 0 && mComparer.Compare(Tpriorities[currentIndex], Tpriorities[(currentIndex - 1) / 2]) < 0)
        {
            (Tpriorities[currentIndex], Tpriorities[(currentIndex - 1) / 2]) = (Tpriorities[(currentIndex - 1) / 2], Tpriorities[currentIndex]);
            (Tvals[currentIndex], Tvals[(currentIndex - 1) / 2]) = (Tvals[(currentIndex - 1) / 2], Tvals[currentIndex]);
            currentIndex = (currentIndex - 1) / 2;
        }
    }

    public T Dequeue()
    {
        if(Count == 0)
        {
            throw new Exception("no queue");
        }

        --index;
        T returnval = Tvals[0];
        int currentIndex = 0;
        Tvals[0] = default(T);
        Tpriorities[0] = default(TPriority);
        (Tvals[0], Tvals[index]) = (Tvals[index], Tvals[0]);
        (Tpriorities[0], Tpriorities[index]) = (Tpriorities[index], Tpriorities[0]);

        while (currentIndex * 2 + 1 < Count)
        {
            if (mComparer.Compare(Tpriorities[currentIndex], Tpriorities[currentIndex * 2 + 1]) > 0
            || mComparer.Compare(Tpriorities[currentIndex], Tpriorities[currentIndex * 2 + 2]) > 0)
            {
                (Tpriorities[currentIndex], Tpriorities[currentIndex * 2 + 1]) = (Tpriorities[currentIndex * 2 + 1], Tpriorities[currentIndex]);
                (Tvals[currentIndex], Tvals[currentIndex * 2 + 1]) = (Tvals[currentIndex * 2 + 1], Tvals[currentIndex]);
            }
            currentIndex = currentIndex * 2 + 1;
        }
        return returnval;
    }

    public T Peek()
    {
        return Tvals[0];
    }

    public bool TryDequeue(out T val, out TPriority priority)
    {
        if (index != 0)
        {
            val = Dequeue();
            priority = default(TPriority);
            return true;
        }

        val = default(T);
        priority = default(TPriority);
        return false;
    }
    public bool TryPeek(out T val, out TPriority priority)
    {
        if (index != 0)
        {
            val = Peek();
            priority = default(TPriority);
            return true;
        }

        val = default(T);
        priority = default(TPriority);
        return false;
    }

    public void Clear()
    {        
        Array.Clear(Tvals, 0, Tvals.Length);
        index = 0;
    }

    private void ReAllocateSize()
    {
        T[] newVals = new T[Size * 2];
        TPriority[] newPriorities = new TPriority[Size * 2];

        for (int i = 0; i < Size; ++i)
        {
            newVals[i] = Tvals[i];
            newPriorities[i] = Tpriorities[i];
        }
        Size *= 2;
        Tvals = newVals;
        Tpriorities = newPriorities;
    }
}

