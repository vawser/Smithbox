using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats.Util;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Keybinds;


public class KeybindsMenu
{
    private KeybindID? _listeningAction;
    private MousebindID? _mouseListeningAction;
    private int _listeningIndex;
    private int _mouseListeningIndex;

    public bool IsDisplayed = false;
    private bool _wasDisplayedLastFrame;
    private bool InitialLayout = false;

    private string _search = "";

    public KeybindsMenu() { }

    public void Draw()
    {
        if (IsDisplayed)
        {
            if (!InitialLayout)
            {
                ImGui.SetKeyboardFocusHere();
                UIHelper.SetupPopupWindow();
                InitialLayout = true;
            }

            if (ImGui.Begin("Shortcuts##keybindsMenu", ref IsDisplayed, UIHelper.GetEditorPopupWindowFlags()))
            {
                ImGui.BeginMenuBar();

                if(ImGui.BeginMenu("Options"))
                {
                    if(ImGui.MenuItem("Save"))
                    {
                        InputManager.SaveKeybinds();
                        InputManager.SaveMousebinds();
                        Smithbox.Log(this, "Shortcuts saved.");
                    }

                    if (ImGui.MenuItem("Revert All to Default"))
                    {
                        var dialog = PlatformUtils.Instance.MessageBox("Are you sure?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                        if (dialog is DialogResult.Yes)
                        {
                            RevertAllKeysToDefault();
                            RevertAllMouseButtonsToDefault();
                        }
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();

                ImGui.BeginTabBar("shortcutTabs");

                if (ImGui.BeginTabItem("Keybinds"))
                {
                    DrawKeybindSearchBar();

                    ImGui.BeginChild("shortcutSection");

                    DisplayKeybindRebindSection();

                    ImGui.EndChild();

                    ListenForKeybindCombo();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Mousebinds"))
                {
                    DrawMousebindSearchBar();

                    ImGui.BeginChild("shortcutSection");

                    DisplayMousebindRebindSection();

                    ImGui.EndChild();

                    ListenForMousebindCombo();

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();

                ImGui.End();
            }
        }
        if (!IsDisplayed && _wasDisplayedLastFrame)
        {
            InputManager.SaveKeybinds();
            InputManager.SaveMousebinds();
            Smithbox.Log(this, "Shortcuts saved.");
        }

        _wasDisplayedLastFrame = IsDisplayed;
    }

    private bool IsModifier(Key key)
    {
        return key == Key.LControl ||
               key == Key.RControl ||
               key == Key.LShift ||
               key == Key.RShift ||
               key == Key.LAlt ||
               key == Key.RAlt;
    }


    #region Keybinds
    private void RevertAllKeysToDefault()
    {
        var grouped = InputManager.Bindings
            .GroupBy(kv => KeybindMetadata.Category[kv.Key])
            .OrderBy(g => g.Key);

        foreach (var categoryGroup in grouped)
        {
            foreach (var (action, bindings) in categoryGroup)
            {
                for (int i = 0; i < bindings.Count; i++)
                {
                    var defaultBinding = InputManager._defaultBindings.Entries[action][i];
                    bindings[i] = defaultBinding.Clone();
                }
            }
        }
    }

    private void DrawKeybindSearchBar()
    {
        ImGui.PushItemWidth(-1);
        ImGui.InputTextWithHint("##ShortcutSearch", "Search shortcuts...", ref _search, 128);
        ImGui.PopItemWidth();

        ImGui.SameLine();

        if (ImGui.SmallButton("X"))
        {
            _search = "";
        }
    }

    private void DisplayKeybindRebindSection()
    {
        var grouped = InputManager.Bindings
            .GroupBy(kv => KeybindMetadata.Category[kv.Key])
            .OrderBy(g => g.Key);

        float nameColumnWidth = 200;

        // Get the maximum name width from all of the bindings
        foreach (var categoryGroup in grouped)
        {
            var curWidth = GetMaxKeybindActionNameWidth(
                categoryGroup.Select(kv => (kv.Key, kv.Value)).ToList());

            if(curWidth > nameColumnWidth)
                nameColumnWidth = curWidth; 
        }

        foreach (var categoryGroup in grouped)
        {
            // Filter actions in this category
            var filteredActions = categoryGroup
                .Where(kv => PassesKeybindSearch(kv.Key, kv.Value))
                .ToList();

            // Skip empty categories when searching
            if (filteredActions.Count == 0)
                continue;

            bool searching = !string.IsNullOrWhiteSpace(_search);

            var flags = ImGuiTreeNodeFlags.DefaultOpen;

            if (ImGui.CollapsingHeader(categoryGroup.Key.GetDisplayName(), flags))
            {
                if (ImGui.BeginTable($"KeybindTable_{categoryGroup.Key}", 4,
                    ImGuiTableFlags.Borders |
                    ImGuiTableFlags.RowBg |
                    ImGuiTableFlags.Resizable))
                {
                    ImGui.TableSetupColumn("##defaultAction", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthFixed, nameColumnWidth);
                    ImGui.TableSetupColumn("Binding", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Description");
                    ImGui.TableHeadersRow();

                    foreach (var (action, bindings) in filteredActions)
                    {
                        var name = "Unassigned";
                        var desc = "";

                        var presentation = KeybindMetadata.Presentation[action];

                        ImGui.TableNextRow();

                        // Default
                        ImGui.TableSetColumnIndex(0);

                        if (ImGui.Button($"{Icons.Refresh}##revert_{action}", DPI.IconButtonSize))
                        {
                            for (int i = 0; i < bindings.Count; i++)
                            {
                                var defaultBinding = InputManager._defaultBindings.Entries[action][i];
                                bindings[i] = defaultBinding.Clone();
                            }
                        }

                        if (presentation.Item1 != null)
                        {
                            name = presentation.Item1;
                        }

                        if (presentation.Item2 != null)
                        {
                            desc = presentation.Item2;
                        }

                        ImGui.TableSetColumnIndex(1);

                        // Name
                        ImGui.TextUnformatted(name);

                        ImGui.TableSetColumnIndex(2);

                        // Keybind
                        for (int i = 0; i < bindings.Count; i++)
                        {
                            var b = bindings[i];

                            bool listening =
                                _listeningAction == action &&
                                _listeningIndex == i;

                            string label = listening
                                ? "Press combo..."
                                : FormatKeyBinding(b);

                            if (ImGui.Button($"{label}##{action}_{i}"))
                            {
                                _listeningAction = action;
                                _listeningIndex = i;
                            }

                            ImGui.SameLine();
                        }

                        // Description
                        ImGui.TableSetColumnIndex(3);

                        ImGui.TextUnformatted(desc);
                    }

                    ImGui.EndTable();
                }
            }
        }
    }

    private void ListenForKeybindCombo()
    {
        if (_listeningAction == null)
            return;

        bool ctrl = InputManager.HasCtrlDown();

        bool shift = InputManager.HasShiftDown();

        bool alt = InputManager.HasAltDown();

        foreach (var key in InputManager.Current.GetDownKeys())
        {
            if (IsModifier(key))
                continue;

            var binding = InputManager.Bindings[_listeningAction.Value][_listeningIndex];
            binding.Key = key;
            binding.Ctrl = ctrl;
            binding.Shift = shift;
            binding.Alt = alt;

            _listeningAction = null;
            return;
        }
    }

    private string FormatKeyBinding(InputManager.KeyBinding b)
    {
        string s = "";
        if (b.Ctrl) s += "Ctrl+";
        if (b.Shift) s += "Shift+";
        if (b.Alt) s += "Alt+";
        s += b.Key.ToString();
        return s;
    }

    private bool PassesKeybindSearch(KeybindID action, List<InputManager.KeyBinding> bindings)
    {
        if (string.IsNullOrWhiteSpace(_search))
            return true;

        var term = _search.Trim().ToLowerInvariant();

        // Action name
        if (action.ToString().ToLowerInvariant().Contains(term))
            return true;

        // Category name
        if (KeybindMetadata.Category[action]
            .ToString()
            .ToLowerInvariant()
            .Contains(term))
            return true;

        // Keybind Name
        if (KeybindMetadata.Presentation[action].Item1
            .ToString()
            .ToLowerInvariant()
            .Contains(term))
            return true;

        // Keybind Description
        if (KeybindMetadata.Presentation[action].Item2
            .ToString()
            .ToLowerInvariant()
            .Contains(term))
            return true;

        // Binding text (Ctrl+S etc.)
        foreach (var b in bindings)
        {
            if (FormatKeyBinding(b).ToLowerInvariant().Contains(term))
                return true;
        }

        return false;
    }
    private float GetMaxKeybindActionNameWidth(List<(KeybindID action, List<InputManager.KeyBinding> bindings)> actions)
    {
        float maxWidth = 0f;

        foreach (var (action, _) in actions)
        {
            var presentation = KeybindMetadata.Presentation[action];
            string name = presentation.Item1 ?? action.ToString();

            float width = ImGui.CalcTextSize(name).X;
            if (width > maxWidth)
                maxWidth = width;
        }

        return maxWidth + ImGui.GetStyle().CellPadding.X * 2 + 6f;
    }

    #endregion

    #region Mousebinds
    private void RevertAllMouseButtonsToDefault()
    {
        var grouped = InputManager.MouseBindings
            .GroupBy(kv => MousebindMetadata.Category[kv.Key])
            .OrderBy(g => g.Key);

        foreach (var categoryGroup in grouped)
        {
            foreach (var (action, bindings) in categoryGroup)
            {
                for (int i = 0; i < bindings.Count; i++)
                {
                    var defaultBinding = InputManager._defaultMouseBindings.Entries[action][i];
                    bindings[i] = defaultBinding.Clone();
                }
            }
        }
    }

    private void DrawMousebindSearchBar()
    {
        ImGui.PushItemWidth(-1);
        ImGui.InputTextWithHint("##MouseShortcutSearch", "Search shortcuts...", ref _search, 128);
        ImGui.PopItemWidth();

        ImGui.SameLine();

        if (ImGui.SmallButton("X##mouseX"))
        {
            _search = "";
        }
    }

    private void DisplayMousebindRebindSection()
    {
        var grouped = InputManager.MouseBindings
            .GroupBy(kv => MousebindMetadata.Category[kv.Key])
            .OrderBy(g => g.Key);

        float nameColumnWidth = 200;

        // Get the maximum name width from all of the bindings
        foreach (var categoryGroup in grouped)
        {
            var curWidth = GetMaxMousebindActionNameWidth(
                categoryGroup.Select(kv => (kv.Key, kv.Value)).ToList());

            if (curWidth > nameColumnWidth)
                nameColumnWidth = curWidth;
        }

        foreach (var categoryGroup in grouped)
        {
            // Filter actions in this category
            var filteredActions = categoryGroup
                .Where(kv => PassesMousebindSearch(kv.Key, kv.Value))
                .ToList();

            // Skip empty categories when searching
            if (filteredActions.Count == 0)
                continue;

            bool searching = !string.IsNullOrWhiteSpace(_search);

            var flags = ImGuiTreeNodeFlags.DefaultOpen;

            if (ImGui.CollapsingHeader(categoryGroup.Key.GetDisplayName(), flags))
            {
                if (ImGui.BeginTable($"KeybindTable_{categoryGroup.Key}", 4,
                    ImGuiTableFlags.Borders |
                    ImGuiTableFlags.RowBg |
                    ImGuiTableFlags.Resizable))
                {
                    ImGui.TableSetupColumn("##defaultAction", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthFixed, nameColumnWidth);
                    ImGui.TableSetupColumn("Binding", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Description");
                    ImGui.TableHeadersRow();

                    foreach (var (action, bindings) in filteredActions)
                    {
                        var name = "Unassigned";
                        var desc = "";

                        var presentation = MousebindMetadata.Presentation[action];

                        ImGui.TableNextRow();

                        // Default
                        ImGui.TableSetColumnIndex(0);

                        if (ImGui.Button($"{Icons.Refresh}##revert_{action}", DPI.IconButtonSize))
                        {
                            for (int i = 0; i < bindings.Count; i++)
                            {
                                var defaultBinding = InputManager._defaultMouseBindings.Entries[action][i];
                                bindings[i] = defaultBinding.Clone();
                            }
                        }

                        if (presentation.Item1 != null)
                        {
                            name = presentation.Item1;
                        }

                        if (presentation.Item2 != null)
                        {
                            desc = presentation.Item2;
                        }

                        ImGui.TableSetColumnIndex(1);

                        // Name
                        ImGui.TextUnformatted(name);

                        ImGui.TableSetColumnIndex(2);

                        // Keybind
                        for (int i = 0; i < bindings.Count; i++)
                        {
                            var b = bindings[i];

                            bool listening =
                                _mouseListeningAction == action &&
                                _mouseListeningIndex == i;

                            string label = listening
                                ? "Press combo..."
                                : FormatMouseBinding(b);

                            if (ImGui.Button($"{label}##{action}_{i}"))
                            {
                                _mouseListeningAction = action;
                                _mouseListeningIndex = i;
                            }

                            ImGui.SameLine();
                        }

                        // Description
                        ImGui.TableSetColumnIndex(3);

                        ImGui.TextUnformatted(desc);
                    }

                    ImGui.EndTable();
                }
            }
        }
    }

    private void ListenForMousebindCombo()
    {
        if (_mouseListeningAction == null)
            return;

        bool ctrl = InputManager.HasCtrlDown();

        bool shift = InputManager.HasShiftDown();

        bool alt = InputManager.HasAltDown();

        foreach (var button in InputManager.MouseCurrent.GetDownButtons())
        {
            var binding = InputManager.MouseBindings[_mouseListeningAction.Value][_listeningIndex];
            binding.Key = button;
            binding.Ctrl = ctrl;
            binding.Shift = shift;
            binding.Alt = alt;

            _mouseListeningAction = null;
            return;
        }
    }

    private string FormatMouseBinding(InputManager.MouseBinding b)
    {
        string s = "";
        if (b.Ctrl) s += "Ctrl+";
        if (b.Shift) s += "Shift+";
        if (b.Alt) s += "Alt+";
        s += b.Key.ToString();
        return s;
    }

    private bool PassesMousebindSearch(MousebindID action, List<InputManager.MouseBinding> bindings)
    {
        if (string.IsNullOrWhiteSpace(_search))
            return true;

        var term = _search.Trim().ToLowerInvariant();

        // Action name
        if (action.ToString().ToLowerInvariant().Contains(term))
            return true;

        // Category name
        if (MousebindMetadata.Category[action]
            .ToString()
            .ToLowerInvariant()
            .Contains(term))
            return true;

        // Mousebind Name
        if (MousebindMetadata.Presentation[action].Item1
            .ToString()
            .ToLowerInvariant()
            .Contains(term))
            return true;

        // Mousebind Description
        if (MousebindMetadata.Presentation[action].Item2
            .ToString()
            .ToLowerInvariant()
            .Contains(term))
            return true;

        // Binding text (Ctrl+S etc.)
        foreach (var b in bindings)
        {
            if (FormatMouseBinding(b).ToLowerInvariant().Contains(term))
                return true;
        }

        return false;
    }
    private float GetMaxMousebindActionNameWidth(List<(MousebindID action, List<InputManager.MouseBinding> bindings)> actions)
    {
        float maxWidth = 0f;

        foreach (var (action, _) in actions)
        {
            var presentation = MousebindMetadata.Presentation[action];
            string name = presentation.Item1 ?? action.ToString();

            float width = ImGui.CalcTextSize(name).X;
            if (width > maxWidth)
                maxWidth = width;
        }

        return maxWidth + ImGui.GetStyle().CellPadding.X * 2 + 6f;
    }
    #endregion
}