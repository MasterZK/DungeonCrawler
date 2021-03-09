using System;
using Unity.Collections;

/// <summary>
/// Implementation of a heap in native code
/// </summary>
/// <typeparam name="T">Type of elements in heap</typeparam>
public struct NativeBinaryHeap<T> : IDisposable where T : unmanaged, IComparable<T>, IEquatable<T>
{
    /// <summary>
    /// Current amount of elements in heap
    /// </summary>
    public int Count { get; internal set; }
    /// <summary>
    /// Maximum amount of storable elements in heap
    /// </summary>
    public int Capacity { get; internal set; }

    public NativeArray<T> elements;
    public NativeHashMap<T, int> elementIndices;

    public NativeBinaryHeap(int maxHeapSize, Allocator allocator)
    {
        Count = 0;
        Capacity = maxHeapSize;

        elements = new NativeArray<T>(maxHeapSize, allocator, NativeArrayOptions.UninitializedMemory);
        elementIndices = new NativeHashMap<T, int>(128, allocator);
    }

    public T this[int i]
    {
        get { return elements[i]; }
    }

    public void Add(T item)
    {
        UpdateHeapItem(item, Count);
        SortUp(item);
        Count++;
    }

    public T RemoveFirst()
    {
        T firstItem = elements[0];
        Count--;

        var item = elements[Count];
        UpdateHeapItem(item, 0);
        SortDown(item);

        return firstItem;
    }

    public T RemoveAt(int index)
    {
        T firstItem = elements[index];
        Count--;

        if (index == Count)
            return firstItem;

        var item = elements[Count];
        UpdateHeapItem(item, index);
        SortDown(item);

        return firstItem;
    }

    public int IndexOf(T item) => GetHeapIndex(item);

    void SortDown(T item)
    {
        while (true)
        {
            int itemIndex = GetHeapIndex(item);
            int childIndexLeft = itemIndex * 2 + 1;
            int childIndexRight = itemIndex * 2 + 2;
            int swapIndex = 0;

            if (childIndexLeft < Count)
            {
                swapIndex = childIndexLeft;

                if (childIndexRight < Count)
                    if (elements[childIndexLeft].CompareTo(elements[childIndexRight]) < 0)
                        swapIndex = childIndexRight;

                if (item.CompareTo(elements[swapIndex]) < 0)
                    Swap(item, elements[swapIndex]);
                else
                    return;

            }
            else
            {
                return;
            }

        }
    }

    void SortUp(T item)
    {
        int parentIndex = (GetHeapIndex(item) - 1) / 2;

        while (true)
        {
            T parentItem = elements[parentIndex];

            if (item.CompareTo(parentItem) > 0)
               Swap(item, parentItem);
            else
                break;

            parentIndex = (GetHeapIndex(item) - 1) / 2;
        }
    }

    void Swap(T itemA, T itemB)
    {
        int itemAIndex = GetHeapIndex(itemA);
        int itemBIndex = GetHeapIndex(itemB);

        UpdateHeapItem(itemB, itemAIndex);
        UpdateHeapItem(itemA, itemBIndex);
    }

    void UpdateHeapItem(T item, int newIndex)
    {
        elementIndices.Remove(item);
        bool success = elementIndices.TryAdd(item, newIndex);
        elements[newIndex] = item;
    }

    int GetHeapIndex(T item)
    {
        if (elementIndices.TryGetValue(item, out int result))
            return result;

        return -1;
    }

    public void Dispose()
    {
        elements.Dispose();
        elementIndices.Dispose();
    }
}

