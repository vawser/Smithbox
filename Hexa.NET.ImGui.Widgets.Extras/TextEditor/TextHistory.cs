namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor
{
    using Hexa.NET.Utilities;

    public unsafe class TextHistory
    {
        private readonly TextHistoryEntry[] undoHistory;
        private readonly TextHistoryEntry[] redoHistory;
        private readonly TextSource source;
        private readonly int maxCount;
        private int undoHistoryCount;
        private int redoHistoryCount;
        private bool disposedValue;

        public TextHistory(TextSource source, int maxCount)
        {
            this.source = source;
            this.maxCount = maxCount;

            undoHistory = new TextHistoryEntry[maxCount];
            redoHistory = new TextHistoryEntry[maxCount];
        }

        public int UndoCount => undoHistoryCount;

        public int RedoCount => redoHistoryCount;

        public bool CanUndo => undoHistoryCount > 0;

        public bool CanRedo => redoHistoryCount > 0;

        public void Clear()
        {
            for (int i = 0; i < undoHistory.Length; i++)
            {
                undoHistory[i].Release();
            }
            undoHistoryCount = 0;
            for (int i = 0; i < redoHistory.Length; i++)
            {
                redoHistory[i].Release();
            }
            redoHistoryCount = 0;
        }

        public void UndoPush()
        {
            UndoPushInternal();
            for (int i = 0; i < redoHistoryCount; i++)
            {
                var index = maxCount - redoHistoryCount;
                redoHistory[index].Release();
            }
            redoHistoryCount = 0;
        }

        private void UndoPushInternal()
        {
            var last = undoHistory[^1];

            // Release the last entry if it contains data
            last.Release();

            // Shift entries in undoHistory to the right
            for (int i = undoHistory.Length - 1; i > 0; i--)
            {
                undoHistory[i] = undoHistory[i - 1];
            }

            // Allocate a new StdString and clone the current text
            last.Data = AllocT<StdWString>();
            *last.Data = source.Text->Clone();

            // Place the new entry at the beginning of undoHistory
            undoHistory[0] = last;
            undoHistoryCount = Math.Min(undoHistoryCount + 1, maxCount);
        }

        private void RedoPushInternal(TextHistoryEntry entry)
        {
            var last = redoHistory[^1];

            // Release the last entry if it contains data
            last.Release();

            // Shift entries in redoHistory to the right
            for (int i = redoHistory.Length - 1; i > 0; i--)
            {
                redoHistory[i] = redoHistory[i - 1];
            }

            // Allocate a new StdString and clone the entry data
            last.Data = AllocT<StdWString>();
            *last.Data = entry.Data->Clone();

            // Place the new entry at the beginning of redoHistory
            redoHistory[0] = last;
            redoHistoryCount = Math.Min(redoHistoryCount + 1, maxCount);
        }

        public void Undo()
        {
            if (undoHistoryCount == 0)
            {
                return;
            }

            var first = undoHistory[0];

            // Shift entries in undoHistory to the left
            for (int i = 0; i < undoHistory.Length - 1; i++)
            {
                undoHistory[i] = undoHistory[i + 1];
            }

            RedoPushInternal(first);

            // Set the source text to the first entry's data
            source.SetText(first.Data);

            // Place the released entry at the end of undoHistory
            undoHistory[^1] = first;
            undoHistoryCount--;
        }

        public void Redo()
        {
            if (redoHistoryCount == 0)
            {
                return;
            }

            var first = redoHistory[0];

            // Shift entries in redoHistory to the left
            for (int i = 0; i < redoHistory.Length - 1; i++)
            {
                redoHistory[i] = redoHistory[i + 1];
            }

            UndoPushInternal();

            // Set the source text to the first entry's data
            source.SetText(first.Data);

            // Place the released entry at the end of redoHistory
            redoHistory[^1] = first;
            redoHistoryCount--;
        }

        public void Dispose()
        {
            if (disposedValue)
            {
                return;
            }

            Clear();

            disposedValue = true;
        }
    }
}