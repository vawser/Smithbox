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
    private InputAction? _listeningAction;
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

            if (ImGui.Begin("Shortcuts##keybindsMenu", ref IsDisplayed, UIHelper.GetPopupWindowFlags()))
            {
                DrawSearchBar();

                ImGui.BeginChild("shortcutSection");

                DisplayRebindSection();

                ImGui.EndChild();

                ListenForCombo();

                ImGui.End();
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
            .GroupBy(kv => InputActionMetadata.Category[kv.Key])
            .OrderBy(g => g.Key);

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
                if (ImGui.BeginTable($"KeybindTable_{categoryGroup.Key}", 2,
                    ImGuiTableFlags.Borders |
                    ImGuiTableFlags.RowBg |
                    ImGuiTableFlags.Resizable))
                {
                    ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthFixed, 200);
                    ImGui.TableSetupColumn("Binding");
                    ImGui.TableHeadersRow();

                    foreach (var (action, bindings) in filteredActions)
                    {
                        ImGui.TableNextRow();

                        ImGui.TableSetColumnIndex(0);
                        ImGui.TextUnformatted(action.GetDisplayName());

                        ImGui.TableSetColumnIndex(1);

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
    private bool PassesSearch(InputAction action, List<InputManager.KeyBinding> bindings)
    {
        if (string.IsNullOrWhiteSpace(_search))
            return true;

        var term = _search.Trim().ToLowerInvariant();

        // Action name
        if (action.ToString().ToLowerInvariant().Contains(term))
            return true;

        // Category name
        if (InputActionMetadata.Category[action]
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
}