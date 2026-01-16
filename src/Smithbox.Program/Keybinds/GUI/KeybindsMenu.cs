using Hexa.NET.ImGui;
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
    private int _listeningIndex;

    public bool IsDisplayed = false;
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
                        InputManager.Save();
                        TaskLogs.AddLog("Shortcuts saved.");
                    }

                    if (ImGui.MenuItem("Revert All to Default"))
                    {
                        var dialog = PlatformUtils.Instance.MessageBox("Are you sure?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                        if (dialog is DialogResult.Yes)
                        {
                            RevertAllToDefault();
                        }
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();

                DrawSearchBar();

                ImGui.BeginChild("shortcutSection");

                DisplayRebindSection();

                ImGui.EndChild();

                ListenForCombo();

                ImGui.End();
            }
        }
    }

    private void RevertAllToDefault()
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

    private void DrawSearchBar()
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

    private void DisplayRebindSection()
    {
        var grouped = InputManager.Bindings
            .GroupBy(kv => KeybindMetadata.Category[kv.Key])
            .OrderBy(g => g.Key);

        float nameColumnWidth = 200;

        // Get the maximum name width from all of the bindings
        foreach (var categoryGroup in grouped)
        {
            var curWidth = GetMaxActionNameWidth(
                categoryGroup.Select(kv => (kv.Key, kv.Value)).ToList());

            if(curWidth > nameColumnWidth)
                nameColumnWidth = curWidth; 
        }

        foreach (var categoryGroup in grouped)
        {
            // Filter actions in this category
            var filteredActions = categoryGroup
                .Where(kv => PassesSearch(kv.Key, kv.Value))
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
                                : FormatBinding(b);

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

    private void ListenForCombo()
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

    private bool IsModifier(Key key)
    {
        return key == Key.LControl ||
               key == Key.RControl ||
               key == Key.LShift ||
               key == Key.RShift ||
               key == Key.LAlt ||
               key == Key.RAlt;
    }

    private string FormatBinding(InputManager.KeyBinding b)
    {
        string s = "";
        if (b.Ctrl) s += "Ctrl+";
        if (b.Shift) s += "Shift+";
        if (b.Alt) s += "Alt+";
        s += b.Key.ToString();
        return s;
    }

    private bool PassesSearch(KeybindID action, List<InputManager.KeyBinding> bindings)
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
            if (FormatBinding(b).ToLowerInvariant().Contains(term))
                return true;
        }

        return false;
    }
    private float GetMaxActionNameWidth(List<(KeybindID action, List<InputManager.KeyBinding> bindings)> actions)
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
}