/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/util/IterativeMergeSort.java
 * PDFBOX_SOURCE_COMMIT: 55f159e4cea6fbf47bd8720885e709ed029d7c88
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: 55f159e4cea6fbf47bd8720885e709ed029d7c88
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace PdfBox.Net.Util;

/// <summary>
/// This class provides an iterative (bottom-up) implementation of the
/// <a href="https://en.wikipedia.org/wiki/Merge_sort">MergeSort</a> algorithm for any generic
/// object which implements a <see cref="IComparer{T}"/>.
/// <para>
/// This implementation uses an iterative implementation approach over the more classical recursive
/// approach in order to save the auxiliary space required by the call stack in recursive
/// implementations.
/// </para>
/// Complexity:
/// <list type="bullet">
///   <item>Worst case time: O(n log n)</item>
///   <item>Best case time: O(n log n)</item>
///   <item>Average case time: O(n log n)</item>
///   <item>Space: O(n log n)</item>
/// </list>
/// </summary>
/// <remarks>Author: Alistair Oldfield</remarks>
public static class IterativeMergeSort
{
    /// <summary>
    /// Sorts the list according to the order induced by the specified <see cref="IComparer{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to be sorted.</param>
    /// <param name="cmp">The comparer to determine the order of the list.</param>
    public static void Sort<T>(IList<T> list, IComparer<T> cmp)
    {
        if (list.Count < 2)
        {
            return;
        }

        T[] arr = new T[list.Count];
        for (int idx = 0; idx < list.Count; idx++)
        {
            arr[idx] = list[idx];
        }

        IterativeMergeSortInternal(arr, cmp);

        for (int idx = 0; idx < arr.Length; idx++)
        {
            list[idx] = arr[idx];
        }
    }

    /// <summary>
    /// Sorts the array using iterative (bottom-up) merge sort.
    /// </summary>
    private static void IterativeMergeSortInternal<T>(T[] arr, IComparer<T> cmp)
    {
        T[] aux = (T[])arr.Clone();

        for (int blockSize = 1; blockSize < arr.Length; blockSize <<= 1)
        {
            for (int start = 0; start < arr.Length; start += blockSize << 1)
            {
                Merge(arr, aux, start, start + blockSize, start + (blockSize << 1), cmp);
            }
        }
    }

    /// <summary>
    /// Merges two sorted subarrays into the order specified by <paramref name="cmp"/> and places
    /// the ordered result back into the <paramref name="arr"/> array.
    /// </summary>
    /// <param name="arr">Array containing source data to be sorted and target for destination data.</param>
    /// <param name="aux">Array containing a copy of source data to be sorted.</param>
    /// <param name="from">Start index of left data run (left run is arr[from : mid-1]).</param>
    /// <param name="mid">End index of left data run / start index of right run (right run is arr[mid : to]).</param>
    /// <param name="to">End index of right run data.</param>
    /// <param name="cmp">The comparer to determine the order.</param>
    private static void Merge<T>(T[] arr, T[] aux, int from, int mid, int to, IComparer<T> cmp)
    {
        if (mid >= arr.Length)
        {
            return;
        }
        if (to > arr.Length)
        {
            to = arr.Length;
        }
        int i = from;
        int j = mid;
        for (int k = from; k < to; k++)
        {
            if (i == mid)
            {
                aux[k] = arr[j++];
            }
            else if (j == to)
            {
                aux[k] = arr[i++];
            }
            else if (cmp.Compare(arr[j], arr[i]) < 0)
            {
                aux[k] = arr[j++];
            }
            else
            {
                aux[k] = arr[i++];
            }
        }
        Array.Copy(aux, from, arr, from, to - from);
    }
}
