using System;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Utilities;

/// <summary>
/// An interval that represents a range between two integers
/// </summary>
public struct Interval
{
    private int _start;
    private int _range;

    /// <summary>
    /// Creates an interval representing a single value in the range
    /// </summary>
    /// <param name="value">The value that represents the single value in the range</param>
    public Interval(int value)
    {
        _start = value;
        _range = 1;
    }
    
    public Interval(int beginInclusive, int endExclusive)
    {
        _start = beginInclusive;
        _range = endExclusive - beginInclusive;
    }

    /// <summary>
    /// The first value in the interval inclusive
    /// </summary>
    public int Begin
    {
        get => _start;
        set => _start = value;
    }

    /// <summary>
    /// The last value in the interval exclusive
    /// </summary>
    public int End
    {
        get => _start + _range;
        set => _range = value - _start;
    }

    /// <summary>
    /// The number of values contained in the range
    /// </summary>
    public int Count => _range;

    /// <summary>
    /// Returns if the value is included in the range
    /// </summary>
    /// <param name="value">The value to test</param>
    /// <returns>True if the value is included in the range</returns>
    public bool Contains(int value) => value >= Begin && value < End;

    /// <summary>
    /// Extends the interval to contain the given value
    /// </summary>
    /// <param name="value">The value to extend the interval to include</param>
    public void AddToRange(int value)
    {
        if (_range == 0)
        {
            _start = value;
            _range = 1;
        }
        else if (value < _start)
        {
            _range += _start - value;
            _start = value;
        }
        else if (value >= _start + _range)
        {
            _range = value - _start + 1;
        }
    }

    /// <summary>
    /// Add a new range to a range representing a range of contiguous temporary registers. This method is specialized
    /// for temporary register ranges and enforces constraints regarding them. Registers should be added in code
    /// generation order.
    /// </summary>
    /// <param name="range"></param>
    /// <param name="allowDuplicates">Duplicates (overlapping ranges) are permitted</param>
    public void AddToTemporaryRegisterRange(Interval range, bool allowDuplicates = false)
    {
        if (range._range == 0)
            return;
        
        // First if we are empty then we obviously need to start with the value.
        if (_range == 0)
        {
            _start = range._start;
            _range = range._range;
            return;
        }
        
        // For a valid temporary range we need to enforce the following constraints on the start value:
        // 1. If the value is the next sequentially (i.e. is equal to end) then we can add it
        if (range.Begin == End)
        {
            End = range.End;
            return;
        }
        
        // 2. If the value is lower than the start of this range it's probably a local and can be ignored
        if (range.End <= Begin)
            return;
        
        // 3. If the value is higher than the end, it breaks the continuity of the range which means that the current
        //    range is all locals.
        if (range.Begin > End)
        {
            _start = range._start;
            _range = range._range;
            return;
        }

        // 4. If the value is within the current range then it's a double use which means it's a local and the range
        //    needs to be clipped such that the start is the next value.
        var end1 = End;
        var end2 = range.End;
        if (!allowDuplicates)
            Begin = Math.Min(end1, end2);
        End = Math.Max(end1, end2);
    }

    public void MergeTemporaryRegisterRange(Interval range)
    {
        if (range.Count == 0)
            return;
        End = Math.Max(End, range.End);
    }

    /// <summary>
    /// Unions this interval with another such that the interval contains all the values of both this range and the
    /// other range
    /// </summary>
    /// <param name="interval">The interval to union with</param>
    public Interval UnionWith(Interval interval)
    {
        if (_range == 0)
            return interval;
        if (interval._range == 0)
            return this;
        
        Interval result;
        result._start = Math.Min(Begin, interval.Begin);
        result._range = Math.Max(End, interval.End) - result._start;
        return result;
    }

    public Interval IntersectWith(Interval interval)
    {
        if (_range == 0 || interval._range == 0)
            return new Interval();

        if (interval.End > Begin && End < interval.Begin)
        {
            return new Interval
            {
                Begin = Begin,
                End = Math.Min(End, interval.End)
            };
        }

        return new Interval();
    }
    
    /// <summary>
    /// Sets the beginning of the range without affecting the end. If the new beginning is bigger than the current end,
    /// then the range will be made empty.
    /// </summary>
    /// <param name="begin"></param>
    public void SetBegin(int begin)
    {
        var end = End;
        _start = begin;
        _range = Math.Max(0, end - _start);
    }

    public override string ToString()
    {
        if (_range == 0)
            return "empty";
        return $"[{_start} {_start + _range})";
    }
}