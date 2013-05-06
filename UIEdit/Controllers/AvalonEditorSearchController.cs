using System;
using System.Linq;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit;
using UIEdit.Utils;

namespace UIEdit.Controllers {
    public class AvalonEditorSearchController {
        public string SearchString { get; set; }
        public List<int> FoundPositions { get; private set; }
        public int LastFoundPosition { get; private set; }
        private readonly TextEditor _textEditor;

        public AvalonEditorSearchController(TextEditor textEditor) {
            _textEditor = textEditor;
            FoundPositions = new List<int>();
            LastFoundPosition = -1;
        }

        public void Reset() {
            FoundPositions.Clear();
            LastFoundPosition = -1;
            SearchString = "";
            Core.ClearMemory();
        }

        public int NextSearch(string searchFragment) {
            if (_textEditor == null || string.IsNullOrEmpty(_textEditor.Text)) return -1;
            Core.ClearMemory();
            
            var nIndex = -1;
            if (searchFragment != SearchString) {
                FoundPositions.Clear();
                var pos = -1;
                while (true) {
                    if (_textEditor.Text.Length <= pos) break;
                    pos = _textEditor.Text.IndexOf(searchFragment, pos + 1, StringComparison.OrdinalIgnoreCase);
                    if (pos == -1) break;
                    FoundPositions.Add(pos);
                }

                if (FoundPositions.Count > 0) {
                    nIndex = FoundPositions[0];
                    LastFoundPosition = nIndex;
                }
                SearchString = searchFragment;
            }
            else {
                if (FoundPositions.OrderBy(t=>t).Any(t => t > LastFoundPosition)) {
                    nIndex = FoundPositions.OrderBy(t => t).First(t => t > LastFoundPosition);
                    LastFoundPosition = nIndex;
                }
            }
            
            if (nIndex == -1) return nIndex;

            var nLineNumber = _textEditor.Document.GetLineByOffset(nIndex).LineNumber;
            var nColNumber = _textEditor.Document.GetLocation(nIndex).Column;
            _textEditor.Select(nIndex, searchFragment.Length);
            _textEditor.ScrollTo(nLineNumber, nColNumber);

            return LastFoundPosition;
        }

        public int PrevSearch(string searchFragment) {
            if (_textEditor == null || string.IsNullOrEmpty(_textEditor.Text)) return -1;
            Core.ClearMemory();

            var nIndex = -1;
            if (FoundPositions.OrderBy(t => t).Any(t => t < LastFoundPosition)) {
                nIndex = FoundPositions.OrderBy(t => t).Last(t => t < LastFoundPosition);
                LastFoundPosition = nIndex;
            }

            if (nIndex == -1) return nIndex;

            var nLineNumber = _textEditor.Document.GetLineByOffset(nIndex).LineNumber;
            var nColNumber = _textEditor.Document.GetLocation(nIndex).Column;
            _textEditor.Select(nIndex, searchFragment.Length);
            _textEditor.ScrollTo(nLineNumber, nColNumber);

            return LastFoundPosition;
        }
    }
}
