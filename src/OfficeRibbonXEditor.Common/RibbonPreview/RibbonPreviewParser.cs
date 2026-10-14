using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace OfficeRibbonXEditor.Common.RibbonPreview;

/// <summary>Non-executing RibbonX preview model. Does not serialize or alter XML.</summary>
public sealed record RibbonPreviewControl(string Kind, string Id, string Label, string? Callback, string? Size, string? ImageMso);
public sealed record RibbonPreviewGroup(string Id, string Label, IReadOnlyList<RibbonPreviewControl> Controls);
public sealed record RibbonPreviewTab(string Id, string Label, IReadOnlyList<RibbonPreviewGroup> Groups);
public sealed record RibbonPreviewDocument(IReadOnlyList<RibbonPreviewTab> Tabs);

public static class RibbonPreviewParser
{
    public const string Namespace2007 = "http://schemas.microsoft.com/office/2006/01/customui";
    public const string Namespace2010 = "http://schemas.microsoft.com/office/2009/07/customui";

    private static readonly HashSet<string> PreviewControls = new(StringComparer.Ordinal)
    {
        "button", "toggleButton", "checkBox", "menu", "dynamicMenu",
        "gallery", "dropDown", "comboBox", "splitButton", "editBox"
    };

    public static RibbonPreviewDocument Parse(string xml)
    {
        ArgumentNullException.ThrowIfNull(xml);

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = 2_000_000
        };

        using var stringReader = new StringReader(xml);
        using var reader = XmlReader.Create(stringReader, settings);
        var root = XDocument.Load(reader, LoadOptions.None).Root;
        if (root == null || root.Name.LocalName != "customUI" ||
            root.Name.NamespaceName is not (Namespace2007 or Namespace2010))
        {
            throw new FormatException("Expected a customUI root with a supported RibbonX namespace.");
        }

        var ns = root.Name.Namespace;
        var tabsNode = root.Element(ns + "ribbon")?.Element(ns + "tabs");
        if (tabsNode == null)
        {
            return new RibbonPreviewDocument(Array.Empty<RibbonPreviewTab>());
        }

        var tabs = tabsNode.Elements(ns + "tab")
            .Select(tab => new RibbonPreviewTab(GetId(tab), GetLabel(tab),
                tab.Elements(ns + "group")
                    .Select(group => new RibbonPreviewGroup(GetId(group), GetLabel(group),
                        group.Elements()
                            .Where(control => control.Name.Namespace == ns &&
                                              PreviewControls.Contains(control.Name.LocalName))
                            .Select(control => new RibbonPreviewControl(
                                control.Name.LocalName,
                                GetId(control), GetLabel(control),
                                (string?)control.Attribute("onAction"),
                                (string?)control.Attribute("size"),
                                (string?)control.Attribute("imageMso")))
                            .ToArray()))
                    .ToArray()))
            .ToArray();

        return new RibbonPreviewDocument(tabs);
    }

    private static string GetId(XElement element) =>
        (string?)element.Attribute("id") ??
        (string?)element.Attribute("idMso") ??
        (string?)element.Attribute("idQ") ?? "(no id)";

    private static string GetLabel(XElement element) =>
        (string?)element.Attribute("label") ??
        ((string?)element.Attribute("getLabel") is { } callback
            ? $"[dynamic: {callback}]"
            : GetId(element));
}
