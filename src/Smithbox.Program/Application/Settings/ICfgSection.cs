namespace StudioCore.Application;

/// <summary>
///     A group of settings held by <see cref="CFG"/> (as <c>CFG.Current.Graphics</c> and so on). A section's members
///     are saved in Configuration.json alongside CFG's own, in the one flat object, under their own names, so its
///     members keep the names (and prefix) they had on CFG.
/// </summary>
/// <remarks>
///     Settings stay fields (or properties) so ImGui can bind them by ref. To add a section: implement this, add a
///     <c>[JsonIgnore] public readonly</c> field for it to CFG and a row to <c>CFG.Sections</c>, and register the type in
///     ProjectJsonSerializerContext.
/// </remarks>
public interface ICfgSection
{
}
