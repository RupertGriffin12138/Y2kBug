using System;

public static class ArrayUtil
{
    public static void Add<T>(ref T[] array, T item)
    {
        int len = array?.Length ?? 0;
        var newArr = new T[len + 1];
        if (len > 0) Array.Copy(array, newArr, len);
        newArr[len] = item;
        array = newArr;
    }

    public static void RemoveAt<T>(ref T[] array, int index)
    {
        if (array == null || index < 0 || index >= array.Length) return;

        if (array.Length == 1)
        {
            array = Array.Empty<T>();
            return;
        }

        var newArr = new T[array.Length - 1];
        if (index > 0) Array.Copy(array, 0, newArr, 0, index);
        if (index < array.Length - 1)
            Array.Copy(array, index + 1, newArr, index, array.Length - index - 1);
        array = newArr;
    }
}
