/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused xUnit coverage for the C# port of Apache PDFBox IterativeMergeSort behavior.
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

using PdfBox.Net.Util;

namespace PdfBox.Net.Tests;

/// <summary>
/// Tests for <see cref="IterativeMergeSort"/>.
/// </summary>
public class IterativeMergeSortTest
{
    [Fact]
    public void SortEmptyListDoesNothing()
    {
        List<int> list = new();
        IterativeMergeSort.Sort(list, Comparer<int>.Default);
        Assert.Empty(list);
    }

    [Fact]
    public void SortSingleElementDoesNothing()
    {
        List<int> list = new() { 42 };
        IterativeMergeSort.Sort(list, Comparer<int>.Default);
        Assert.Equal(new[] { 42 }, list);
    }

    [Fact]
    public void SortAlreadySortedList()
    {
        List<int> list = new() { 1, 2, 3, 4, 5 };
        IterativeMergeSort.Sort(list, Comparer<int>.Default);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, list);
    }

    [Fact]
    public void SortReversedList()
    {
        List<int> list = new() { 5, 4, 3, 2, 1 };
        IterativeMergeSort.Sort(list, Comparer<int>.Default);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, list);
    }

    [Fact]
    public void SortUnsortedList()
    {
        List<int> list = new() { 3, 1, 4, 1, 5, 9, 2, 6 };
        IterativeMergeSort.Sort(list, Comparer<int>.Default);
        Assert.Equal(new[] { 1, 1, 2, 3, 4, 5, 6, 9 }, list);
    }

    [Fact]
    public void SortWithCustomComparerDescending()
    {
        List<int> list = new() { 3, 1, 4, 1, 5 };
        IterativeMergeSort.Sort(list, Comparer<int>.Create((a, b) => b.CompareTo(a)));
        Assert.Equal(new[] { 5, 4, 3, 1, 1 }, list);
    }

    [Fact]
    public void SortTwoElements()
    {
        List<int> list = new() { 2, 1 };
        IterativeMergeSort.Sort(list, Comparer<int>.Default);
        Assert.Equal(new[] { 1, 2 }, list);
    }

    [Fact]
    public void SortStrings()
    {
        List<string> list = new() { "banana", "apple", "cherry", "apricot" };
        IterativeMergeSort.Sort(list, StringComparer.Ordinal);
        Assert.Equal(new[] { "apple", "apricot", "banana", "cherry" }, list);
    }

    [Fact]
    public void SortIsStableForEqualElements()
    {
        // Use a tuple list with a secondary ordering that must remain stable
        var list = new List<(int Key, int Order)>
        {
            (1, 0), (2, 1), (1, 2), (2, 3), (1, 4)
        };
        IterativeMergeSort.Sort(list, Comparer<(int Key, int Order)>.Create((a, b) => a.Key.CompareTo(b.Key)));
        // All key=1 items should retain their original relative order (0, 2, 4)
        var ones = list.Where(t => t.Key == 1).Select(t => t.Order).ToList();
        Assert.Equal(new[] { 0, 2, 4 }, ones);
    }
}
