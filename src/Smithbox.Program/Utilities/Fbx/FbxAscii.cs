#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace StudioCore.Utilities.Fbx;

/// <summary>
///     ASCII FBX reading (6.x and 7.x) into the same node tree as binary FBX (<see cref="FbxDocument" />).
///     <para>
///         Syntax: <c>Name: value, value, ... { children }</c>, <c>;</c> comments, quoted strings, bare words
///         (<c>Y</c>, <c>T</c>), and in 7.x arrays as <c>*count { a: v,v,v }</c>. ASCII has no types, so values are
///         typed as binary FBX would type them: whole numbers as Int32 (Int64 when out of range), others as Double,
///         one-letter words as Bool characters, other words as strings. Readers of the tree convert between number
///         types (<see cref="FbxProperty.AsLong" />, <see cref="FbxProperty.AsDoubleArray" />).
///     </para>
///     <para>
///         Object names are written "Class::Name" in ASCII and "Name\0\x01Class" in binary; they are turned into the
///         binary form, so the scene layer sees one form.
///     </para>
/// </summary>
internal sealed class FbxAscii
{
    private enum Tok
    {
        Eof,
        Name,
        String,
        Number,
        Word,
        Comma,
        LBrace,
        RBrace,
        Star,
    }

    private static readonly Regex VersionComment = new(@"FBX\s+(\d+)\.(\d+)\.(\d+)", RegexOptions.Compiled);

    private readonly string _s;
    private int _p;
    private int _line = 1;

    private Tok _tok;
    private int _start;
    private int _length;
    private string _text = "";
    private int? _commentVersion;

    private FbxAscii(string s)
    {
        _s = s;
        if (_s.Length > 0 && _s[0] == '﻿')
            _p = 1;
    }

    public static FbxDocument Read(byte[] data)
    {
        var r = new FbxAscii(Encoding.UTF8.GetString(data));
        r.Next();
        var doc = new FbxDocument { IsBinary = false };
        doc.Nodes.AddRange(r.ParseNodes(false));

        var header = doc.Find("FBXHeaderExtension")?.Child("FBXVersion")?.Prop(0);
        doc.Version = header is { IsNumber: true } ? header.AsInt() : r._commentVersion ?? 7100;
        return doc;
    }

    // ------------------------------------------------------------------ parsing

    private List<FbxNode> ParseNodes(bool inBraces)
    {
        var list = new List<FbxNode>();
        while (true)
        {
            switch (_tok)
            {
                case Tok.Eof when inBraces:
                    throw Error("file ends inside a { } block");
                case Tok.Eof:
                    return list;
                case Tok.RBrace when !inBraces:
                    throw Error("'}' without a matching '{'");
                case Tok.RBrace:
                    Next();
                    return list;
                case Tok.Name:
                    break;
                default:
                    throw Error($"expected a node name, found {_tok} '{Current}'");
            }

            var node = new FbxNode(_text);
            Next();
            ParseValues(node);
            if (_tok == Tok.LBrace)
            {
                Next();
                node.Children.AddRange(ParseNodes(true));
                node.EmptyBody = node.Children.Count == 0 && node.Properties.Count > 0;
            }

            list.Add(node);
        }
    }

    private static bool IsValue(Tok t) => t is Tok.String or Tok.Number or Tok.Word or Tok.Star;

    private void ParseValues(FbxNode node)
    {
        // Lenient on a leading comma ("Content: ,..." in some 6.x files).
        if (_tok == Tok.Comma)
            Next();

        while (IsValue(_tok))
        {
            node.Properties.Add(ReadValue());
            if (_tok != Tok.Comma)
                break;
            Next();
        }
    }

    private FbxProperty ReadValue()
    {
        switch (_tok)
        {
            case Tok.String:
            {
                var v = BinaryObjectName(_text);
                Next();
                return FbxProperty.String(v);
            }
            case Tok.Word:
            {
                var v = _text;
                Next();
                return v.Length == 1 ? FbxProperty.Char((byte)v[0]) : FbxProperty.String(v);
            }
            case Tok.Number:
            {
                var p = ParseNumber(Current);
                Next();
                return p;
            }
            case Tok.Star:
                return ReadArray();
            default:
                throw Error($"expected a value, found {_tok}");
        }
    }

    /// <summary><c>*count { a: v,v,v }</c></summary>
    private FbxProperty ReadArray()
    {
        var count = int.Parse(Current, NumberStyles.None, CultureInfo.InvariantCulture);
        Next();
        if (_tok != Tok.LBrace)
            throw Error("expected '{' after an array count");
        Next();

        var longs = new List<long>(count);
        var doubles = (List<double>?)null;
        if (_tok == Tok.Name)
        {
            if (_text != "a")
                throw Error($"expected 'a:' in an array, found '{_text}:'");
            Next();
            while (_tok == Tok.Number)
            {
                var text = Current;
                if (doubles == null && IsWhole(text) && long.TryParse(text, NumberStyles.AllowLeadingSign,
                        CultureInfo.InvariantCulture, out var l))
                {
                    longs.Add(l);
                }
                else
                {
                    doubles ??= longs.ConvertAll(v => (double)v);
                    doubles.Add(ParseDouble(text));
                }

                Next();
                if (_tok != Tok.Comma)
                    break;
                Next();
            }
        }

        if (_tok != Tok.RBrace)
            throw Error($"expected '}}' at the end of an array, found {_tok} '{Current}'");
        Next();

        var n = doubles?.Count ?? longs.Count;
        if (n != count)
            throw Error($"array says {count} values, holds {n}");

        if (doubles != null)
            return FbxProperty.DoubleArray(doubles.ToArray());
        if (longs.TrueForAll(v => v is >= int.MinValue and <= int.MaxValue))
            return FbxProperty.Int32Array(longs.ConvertAll(v => (int)v).ToArray());
        return FbxProperty.Int64Array(longs.ToArray());
    }

    private FbxProperty ParseNumber(ReadOnlySpan<char> text)
    {
        if (IsWhole(text) && long.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var l))
            return l is >= int.MinValue and <= int.MaxValue ? FbxProperty.Int32((int)l) : FbxProperty.Int64(l);
        return FbxProperty.Double(ParseDouble(text));
    }

    private static bool IsWhole(ReadOnlySpan<char> text)
    {
        foreach (var c in text)
        {
            if (!char.IsAsciiDigit(c) && c != '-' && c != '+')
                return false;
        }

        return true;
    }

    private double ParseDouble(ReadOnlySpan<char> text)
    {
        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
            return d;

        // MSVC-style specials: 1.#INF, -1.#INF, -1.#IND, 1.#QNAN.
        var negative = text.Length > 0 && text[0] == '-';
        if (text.Contains("INF", StringComparison.OrdinalIgnoreCase))
            return negative ? double.NegativeInfinity : double.PositiveInfinity;
        if (text.Contains("IND", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("NAN", StringComparison.OrdinalIgnoreCase))
            return double.NaN;
        throw Error($"bad number '{text.ToString()}'");
    }

    /// <summary>"Model::Hips" → "Hips\0\x01Model" (binary FBX's order). Other strings are kept.</summary>
    private static string BinaryObjectName(string s)
    {
        var i = s.IndexOf("::", StringComparison.Ordinal);
        if (i <= 0)
            return s;
        for (var k = 0; k < i; k++)
        {
            if (!char.IsAsciiLetterOrDigit(s[k]) && s[k] != '_')
                return s;
        }

        return s[(i + 2)..] + FbxProperty.NameClassSeparator + s[..i];
    }

    // ------------------------------------------------------------------ tokens

    private ReadOnlySpan<char> Current => _s.AsSpan(_start, _length);

    private void Next()
    {
        SkipSpaceAndComments();
        _start = _p;
        _length = 0;
        if (_p >= _s.Length)
        {
            _tok = Tok.Eof;
            return;
        }

        var c = _s[_p];
        switch (c)
        {
            case '{':
                _p++;
                _tok = Tok.LBrace;
                return;
            case '}':
                _p++;
                _tok = Tok.RBrace;
                return;
            case ',':
                _p++;
                _tok = Tok.Comma;
                return;
            case '"':
            {
                var end = _s.IndexOf('"', _p + 1);
                if (end < 0)
                    throw Error("string without a closing quote");
                var raw = _s.Substring(_p + 1, end - _p - 1);
                foreach (var ch in raw)
                {
                    if (ch == '\n')
                        _line++;
                }

                _text = raw.Contains('&') ? raw.Replace("&quot;", "\"") : raw;
                _p = end + 1;
                _tok = Tok.String;
                return;
            }
            case '*':
            {
                _p++;
                _start = _p;
                while (_p < _s.Length && char.IsAsciiDigit(_s[_p]))
                    _p++;
                _length = _p - _start;
                if (_length == 0)
                    throw Error("'*' without an array count");
                _tok = Tok.Star;
                return;
            }
        }

        if (char.IsAsciiLetter(c) || c == '_')
        {
            while (_p < _s.Length && (char.IsAsciiLetterOrDigit(_s[_p]) || _s[_p] is '_' or '|' or '-' or '.'))
                _p++;
            _length = _p - _start;
            _text = _s.Substring(_start, _length);

            var q = _p;
            while (q < _s.Length && _s[q] is ' ' or '\t')
                q++;
            if (q < _s.Length && _s[q] == ':')
            {
                _p = q + 1;
                _tok = Tok.Name;
            }
            else
            {
                _tok = Tok.Word;
            }

            return;
        }

        if (char.IsAsciiDigit(c) || c is '-' or '+' or '.')
        {
            _p++;
            while (_p < _s.Length && (char.IsAsciiLetterOrDigit(_s[_p]) || _s[_p] is '+' or '-' or '.' or '#'))
                _p++;
            _length = _p - _start;
            _tok = Tok.Number;
            return;
        }

        throw Error($"unexpected character '{c}'");
    }

    private void SkipSpaceAndComments()
    {
        while (_p < _s.Length)
        {
            var c = _s[_p];
            if (c == '\n')
            {
                _line++;
                _p++;
            }
            else if (c is ' ' or '\t' or '\r')
            {
                _p++;
            }
            else if (c == ';')
            {
                var end = _s.IndexOf('\n', _p);
                if (end < 0)
                    end = _s.Length;
                if (_commentVersion == null)
                {
                    var m = VersionComment.Match(_s, _p, end - _p);
                    if (m.Success)
                        _commentVersion = int.Parse(m.Groups[1].Value) * 1000 + int.Parse(m.Groups[2].Value) * 100 +
                                          int.Parse(m.Groups[3].Value) * 10;
                }

                _p = end;
            }
            else
            {
                return;
            }
        }
    }

    private InvalidDataException Error(string message) => new($"ASCII FBX line {_line}: {message}");
}
