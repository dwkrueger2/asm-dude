﻿// The MIT License (MIT)
//
// Copyright (c) 2017 Henk-Jan Lebbink
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using AsmDude.Tools;
using AsmTools;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Runtime.InteropServices;

namespace AsmDude.SignatureHelp {

    internal sealed class AsmSignatureHelpCommandFilter : IOleCommandTarget {
        private readonly ITextView _textView;
        private readonly ISignatureHelpBroker _broker;

        private ISignatureHelpSession _session;
        private IOleCommandTarget _nextCommandHandler;

        internal AsmSignatureHelpCommandFilter(IVsTextView textViewAdapter, ITextView textView, ISignatureHelpBroker broker) {
            this._textView = textView;
            this._broker = broker;

            //add this to the filter chain
            textViewAdapter.AddCommandFilter(this, out this._nextCommandHandler);
        }

        private char GetTypeChar(IntPtr pvaIn) {
            return (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            try {
                SnapshotPoint currentPoint = this._textView.Caret.Position.BufferPosition;
                if ((currentPoint != null) && (currentPoint > 0)) {
                    SnapshotPoint point = currentPoint - 1;
                    if (point.Position > 1) {
                        ITextSnapshotLine line = point.Snapshot.GetLineFromPosition(point.Position);
                        string lineStr = line.GetText();

                        int pos = point.Position - line.Start;
                        if (!AsmSourceTools.IsInRemark(pos, lineStr)) { //check if current position is in a remark; if we are in a remark, no signature help

                            if ((pguidCmdGroup == VSConstants.VSStd2K) && (nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)) {
                                char typedChar = GetTypeChar(pvaIn);
                                if (char.IsWhiteSpace(typedChar) || typedChar.Equals(',')) {
                                    var t = AsmSourceTools.ParseLine(lineStr);
                                    if (this._session != null) this._session.Dismiss(); // cleanup previous session
                                    if (t.mnemonic != Mnemonic.UNKNOWN) {
                                        this._session = this._broker.TriggerSignatureHelp(this._textView);
                                    }
                                } else if (AsmSourceTools.IsRemarkChar(typedChar) && (this._session != null)) {
                                    this._session.Dismiss();
                                    this._session = null;
                                }
                            } else {
                                bool enterPressed = (nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN);
                                if (enterPressed && (this._session != null)) {
                                    this._session.Dismiss();
                                    this._session = null;
                                }
                            }
                        }
                    }
                }
            } catch (Exception e) {
                AsmDudeToolsStatic.Output(string.Format("ERROR: {0}:Exec; e={1}", ToString(), e.ToString()));
            }
            return this._nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
            return this._nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
    }
}
