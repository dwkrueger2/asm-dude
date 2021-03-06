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

using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;

namespace AsmDude.Tools
{
    public struct Undefined_Label_Struct
    {
        // the file that is supposed to be included
        public string include_filename;
        // path at which the include_filename is supposed to be found
        public string path;
        // full path and name of the source file in which the include is defined
        public string source_filename;
        // the lineNumber at which the include is defined.
        public int lineNumber;
    }

    public interface ILabelGraph
    {
        SortedSet<uint> Get_Label_Def_Linenumbers(string label);
        IEnumerable<int> Get_All_Related_Linenumber();

        /// <summary>
        /// Return whether this label graph is enabled
        /// </summary>
        bool Is_Enabled { get; }

        int Get_Linenumber(uint id);
        string Get_Filename(uint id);
        bool Is_From_Main_File(uint id);


        bool Has_Label(string label);
        bool Has_Label_Clash(string label);

        /// <summary>
        /// Return file-line identifiers
        /// </summary>
        /// <param name="labelPrefix"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        SortedSet<uint> Label_Used_At_Info(string full_Qualified_Label, string label);

        /// <summary>
        /// Return dictionary of line numbers with label clash descriptions
        /// </summary>
        SortedDictionary<uint, string> Get_Label_Clashes { get; }

        /// <summary>
        /// Return dictionary of line numbers with undefined label descriptions
        /// </summary>
        SortedDictionary<uint, string> Get_Undefined_Labels { get; }

        IList<Undefined_Label_Struct> Get_Undefined_Includes { get; }

        SortedDictionary<string, string> Get_Label_Descriptions { get; }

        void Reset_Delayed();
        event EventHandler<CustomEventArgs> Reset_Done_Event;
    }
}