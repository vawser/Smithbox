namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor
{
    using Hexa.NET.Utilities;
    using System.Globalization;
    using System.Numerics;
    using System.Text;

    public unsafe class TextSource
    {
        private StdWString* text;
        private NewLineType newLineType;

        public TextSource(string text)
        {
            this.text = AllocT<StdWString>();
            *this.text = new(text);
            IsBinary = !IsText(this.text);
            newLineType = GetNewLineType(this.text);
            LineCount = CountLines(this.text);
        }

        public StdWString* Text
        {
            get => text;
        }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public NewLineType NewLineType
        {
            get => newLineType;
            set
            {
                newLineType = value;
                if (value == NewLineType.Mixed)
                {
                    return;
                }
                ConvertNewLineType(text, value);
                LineCount = CountLines(text);
            }
        }

        public bool IsBinary { get; set; }

        public bool Changed { get; set; }

        public int LineCount { get; set; }

        public int CharacterCount { get; set; }

        public void SetText(StdWString* newText)
        {
            text->Resize(newText->Size);
            MemcpyT(newText->CStr(), text->CStr(), newText->Size);
        }

        public static bool IsText(StdWString* text)
        {
            for (int i = 0; i < text->Size; i++)
            {
                char c = (*text)[i];
                if (char.GetUnicodeCategory(c) == UnicodeCategory.Control)
                {
                    return false;
                }
            }

            return true;
        }

        public static NewLineType GetNewLineType(StdWString* text)
        {
            if (text->Size == 0)
            {
                return NewLineType.CRLF; 
            }

            bool crlf = false;
            bool lf = false;
            bool cr = false;

            bool foundFlag = false;

            for (int i = 0; i < text->Size; i++)
            {
                char c = (*text)[i];
                char c1 = (i + 1 < text->Size) ? (*text)[i + 1] : '\0'; 

                if (c == '\r' && c1 == '\n')
                {
                    if (foundFlag && !crlf)
                    {
                        return NewLineType.Mixed;
                    }
                    crlf = true;
                    foundFlag = true;
                    i++;
                }
                else if (c == '\n')
                {
                    if (foundFlag && !lf)
                    {
                        return NewLineType.Mixed;
                    }
                    lf = true;
                    foundFlag = true;
                }
                else if (c == '\r')
                {
                    if (foundFlag && !cr)
                    {
                        return NewLineType.Mixed;
                    }
                    cr = true;
                    foundFlag = true;
                }
            }

            if (crlf)
            {
                return NewLineType.CRLF;
            }
            if (lf)
            {
                return NewLineType.LF;
            }
            if (cr)
            {
                return NewLineType.CR;
            }

            return NewLineType.Mixed;
        }

        public List<TextSpan> Lines { get; } = [];

        public float MaxLineWidth { get; private set; }

        public Vector2 LayoutSize { get; private set; }

        public void Update(float lineHeight)
        {
            char* pText = text->Data;
            Lines.Clear();
            int lineStart = 0;
            float maxWidth = 0;

            for (int i = 0; i < text->Size; i++)
            {
                char c = pText[i];

                if (c == '\n' || c == '\r')
                {
                    // for CRLF
                    if (c == '\r' && i < text->Size - 1 && pText[i + 1] == '\n')
                    {
                        i++;
                    }

                    TextSpan span = new(text, lineStart, i - lineStart);
                    span.Size = ImGuiWChar.CalcTextSize(pText + lineStart, pText + i).X;
                    maxWidth = Math.Max(maxWidth, span.Size);
                    Lines.Add(span);
                    lineStart = i + 1;
                }
            }

            if (lineStart <= text->Size)
            {
                TextSpan span = new(text, lineStart, text->Size - lineStart);
                span.Size = ImGuiWChar.CalcTextSize(pText + lineStart, pText + text->Size).X;
                maxWidth = Math.Max(maxWidth, span.Size);
                Lines.Add(span);
            }

            MaxLineWidth = maxWidth;
            LineCount = Lines.Count;
            LayoutSize = new(maxWidth, Lines.Count * lineHeight);
        }

        public static void ConvertNewLineType(StdWString* text, NewLineType newLineType)
        {
            // don't care just return original, you can't convert it to mixed anyway.
            if (newLineType == NewLineType.Mixed)
            {
                return;
            }

            string separator = newLineType switch
            {
                NewLineType.CRLF => "\r\n",
                NewLineType.LF => "\n",
                NewLineType.CR => "\r",
                _ => throw new NotSupportedException(),
            };
            StringBuilder sb = new();
            for (int i = 0; i < text->Size; i++)
            {
                char c = (*text)[i];
                char c1 = (*text)[i + 1 == text->Size ? i : i + 1];

                if (c == '\r' && c1 == '\n' || c == '\n' || c == '\r')
                {
                    sb.Append(separator);
                    if (c1 == '\n')
                    {
                        i++;
                    }
                    continue;
                }

                sb.Append(c);
            }
            return;
        }

        public static int CountLines(StdWString* text)
        {
            int lineCount = 1;

            foreach (char c in *text)
            {
                if (c == '\n')
                {
                    lineCount++;
                }
            }

            return lineCount;
        }

        public void Dispose()
        {
            text->Release();
            Free(text);
            text = null;
        }
    }
}