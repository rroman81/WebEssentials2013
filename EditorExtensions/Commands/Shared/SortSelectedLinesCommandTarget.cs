﻿using EnvDTE80;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Linq;
using System.Collections.Generic;

namespace MadsKristensen.EditorExtensions
{
    internal class SortSelectedLines : CommandTargetBase
    {
        private DTE2 _dte;

        public SortSelectedLines(IVsTextView adapter, IWpfTextView textView)
            : base(adapter, textView, GuidList.guidEditorLinesCmdSet, PkgCmdIDList.SortAsc, PkgCmdIDList.SortDesc)
        {
            _dte = EditorExtensionsPackage.DTE;
        }

        protected override bool Execute(uint commandId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            var span = GetSpan();
            var lines = span.GetText().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length == 0)
                return false;

            string result = SortLines(commandId, lines);

            _dte.UndoContext.Open("Sort Selected Lines");
            TextView.TextBuffer.Replace(span.Span, result);
            _dte.UndoContext.Close();

            return true;
        }

        private static string SortLines(uint commandId, IEnumerable<string> lines)
        {
            if (commandId == PkgCmdIDList.SortAsc)
                lines = lines.OrderBy(t => t);
            else
                lines = lines.OrderByDescending(t => t);

            return string.Join(Environment.NewLine, lines);
        }

        private SnapshotSpan GetSpan()
        {
            var sel = TextView.Selection.StreamSelectionSpan;
            var start = new SnapshotPoint(TextView.TextSnapshot, sel.Start.Position).GetContainingLine().Start;
            var end = new SnapshotPoint(TextView.TextSnapshot, sel.End.Position).GetContainingLine().End;

            return new SnapshotSpan(start, end);
        }

        protected override bool IsEnabled()
        {
            return !TextView.Selection.IsEmpty;
        }
    }
}