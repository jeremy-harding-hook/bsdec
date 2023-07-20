//-----------------------------------------------------------------------
//
// Copyright 2023 Jeremy Harding Hook
//
// This file is part of BsdecGui.
//
// BsdecGui is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option)
// any later version.
//
// BsdecGui is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
// details.
//
// You should have received a copy of the GNU General Public License along with
// BsdecGui. If not, see <https://www.gnu.org/licenses/>.
//
//-----------------------------------------------------------------------

using Avalonia.Controls;
using BsdecGui.ViewModels;
using System.Threading.Tasks;

namespace BsdecGui.Views
{
    public partial class FormatEditorView : UserControl
    {
        public FormatEditorView()
        {
            InitializeComponent();
        }

        string cachedText = string.Empty;

        public void OnTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (DataContext is not FormatEditor editor)
                return;
            Task.Run(() =>
            {
                if (editor.TextChangePending)
                {
                    editor.TextChangePending = false;
                    editor.Sync();
                }
            });
        }
    }
}
