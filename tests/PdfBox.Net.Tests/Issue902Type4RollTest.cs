/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Focused Type 4 roll operator parity and allocation coverage for issue #902.
 *
 * PORT_MODE: native-test
 */

using PdfBox.Net.PDModel.Common.Function.Type4;

namespace PdfBox.Net.Tests;

public class Issue902Type4RollTest
{
    [Fact]
    public void RollRotatesOnlyRequestedStackTail()
    {
        AssertStack("0 1 2 3 4 5 3 1 roll", 0, 1, 2, 5, 3, 4);
        AssertStack("0 1 2 3 4 5 3 -1 roll", 0, 1, 2, 4, 5, 3);
    }

    [Fact]
    public void RollNormalizesZeroAndOversizedRotations()
    {
        AssertStack("0 1 2 3 4 5 3 0 roll", 0, 1, 2, 3, 4, 5);
        AssertStack("0 1 2 3 4 5 3 4 roll", 0, 1, 2, 5, 3, 4);
        AssertStack("0 1 2 3 4 5 3 -4 roll", 0, 1, 2, 4, 5, 3);
        AssertStack("0 1 2 3 4 5 3 6 roll", 0, 1, 2, 3, 4, 5);
        AssertStack("0 1 1 37 roll", 0, 1);
    }

    [Fact]
    public void RollPreservesObjectIdentityAndPrefix()
    {
        object prefix = new();
        object first = new();
        object second = new();
        object third = new();
        PdfBox.Net.PDModel.Common.Function.Type4.ExecutionContext context = new(new Operators());
        ExecutionStack stack = context.GetStack();
        stack.Push(prefix);
        stack.Push(first);
        stack.Push(second);
        stack.Push(third);
        stack.Push(3);
        stack.Push(1);

        new StackOperators.Roll().Execute(context);

        Assert.Equal(4, stack.Count);
        Assert.Same(prefix, stack[0]);
        Assert.Same(third, stack[1]);
        Assert.Same(first, stack[2]);
        Assert.Same(second, stack[3]);
    }

    [Fact]
    public void RollPreservesExistingInvalidRangeBehavior()
    {
        Assert.Throws<ArgumentException>(() => Type4Tester.Create("1 2 -1 1 roll"));
        Assert.Throws<ArgumentOutOfRangeException>(() => Type4Tester.Create("1 2 3 1 roll"));
        Assert.Throws<DivideByZeroException>(() => Type4Tester.Create("1 0 1 roll"));

        AssertStack("1 0 0 roll", 1);
        AssertStack("1 -1 0 roll", 1);
        AssertStack("1 2 0 roll", 1);
    }

    [Fact]
    public void RollAllocatesNoTemporaryCollectionsInSteadyState()
    {
        object prefix = new();
        object first = new();
        object second = new();
        object third = new();
        object n = 3;
        object j = 1;
        PdfBox.Net.PDModel.Common.Function.Type4.ExecutionContext context = new(new Operators());
        ExecutionStack stack = context.GetStack();
        StackOperators.Roll roll = new();

        for (int i = 0; i < 1_000; i++)
        {
            ResetStack(stack, prefix, first, second, third, n, j);
            roll.Execute(context);
        }

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 10_000; i++)
        {
            ResetStack(stack, prefix, first, second, third, n, j);
            roll.Execute(context);
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0, allocated);
    }

    private static void AssertStack(string program, params int[] expected)
    {
        ExecutionStack stack = Type4Tester.Create(program).ToExecutionContext().GetStack();
        Assert.Equal(expected, stack.Select(Convert.ToInt32));
    }

    private static void ResetStack(
        ExecutionStack stack,
        object prefix,
        object first,
        object second,
        object third,
        object n,
        object j)
    {
        stack.Clear();
        stack.Push(prefix);
        stack.Push(first);
        stack.Push(second);
        stack.Push(third);
        stack.Push(n);
        stack.Push(j);
    }
}
