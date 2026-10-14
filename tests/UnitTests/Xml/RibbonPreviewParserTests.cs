using System;
using System.Xml;
using NUnit.Framework;
using OfficeRibbonXEditor.Common.RibbonPreview;

namespace OfficeRibbonXEditor.UnitTests.Xml;

public class RibbonPreviewParserTests
{
    [TestCase(RibbonPreviewParser.Namespace2007)]
    [TestCase(RibbonPreviewParser.Namespace2010)]
    public void ParsesTabsGroupsAndCallbacks(string xmlNamespace)
    {
        var xml = $"""
            <customUI xmlns="{xmlNamespace}">
                <ribbon><tabs>
                    <tab id="tabAsimov" label="ASIMOV">
                        <group id="grpTools" label="Tools">
                            <button id="btnImport" label="Import" size="large"
                                    onAction="AS_Import" imageMso="FileOpen"/>
                            <toggleButton id="btnToggle" label="Toggle" onAction="AS_Toggle"/>
                        </group>
                    </tab>
                </tabs></ribbon>
            </customUI>
            """;

        var document = RibbonPreviewParser.Parse(xml);
        Assert.That(document.Tabs, Has.Count.EqualTo(1));
        Assert.That(document.Tabs[0].Label, Is.EqualTo("ASIMOV"));
        Assert.That(document.Tabs[0].Groups[0].Label, Is.EqualTo("Tools"));
        Assert.That(document.Tabs[0].Groups[0].Controls, Has.Count.EqualTo(2));
        Assert.That(document.Tabs[0].Groups[0].Controls[0].Callback, Is.EqualTo("AS_Import"));
        Assert.That(document.Tabs[0].Groups[0].Controls[0].Size, Is.EqualTo("large"));
        Assert.That(document.Tabs[0].Groups[0].Controls[0].ImageMso, Is.EqualTo("FileOpen"));
    }

    [Test]
    public void ParsesPrefixedXmlAndDynamicLabel()
    {
        var xml = $"""
            <mso:customUI xmlns:mso="{RibbonPreviewParser.Namespace2010}">
                <mso:ribbon><mso:tabs><mso:tab idMso="TabHome">
                    <mso:group id="groupA" getLabel="GetGroupLabel">
                        <mso:button id="action" label="Execute"/>
                    </mso:group>
                </mso:tab></mso:tabs></mso:ribbon>
            </mso:customUI>
            """;

        var document = RibbonPreviewParser.Parse(xml);
        Assert.That(document.Tabs[0].Id, Is.EqualTo("TabHome"));
        Assert.That(document.Tabs[0].Groups[0].Label, Is.EqualTo("[dynamic: GetGroupLabel]"));
    }

    [Test]
    public void ReturnsNoTabsWhenRibbonIsAbsent()
    {
        var xml = $"<customUI xmlns=\"{RibbonPreviewParser.Namespace2010}\"/>";
        Assert.That(RibbonPreviewParser.Parse(xml).Tabs, Is.Empty);
    }

    [Test]
    public void RejectsUnsupportedNamespace()
    {
        Assert.Throws<FormatException>(() =>
            RibbonPreviewParser.Parse("<customUI xmlns=\"urn:unexpected\"/>"));
    }

    [Test]
    public void RejectsExternalEntitiesAndDtd()
    {
        var xml = $"""
            <!DOCTYPE customUI [<!ENTITY xxe SYSTEM "file:///C:/Windows/win.ini">]>
            <customUI xmlns="{RibbonPreviewParser.Namespace2010}">
                <ribbon><tabs><tab id="a" label="&xxe;"/></tabs></ribbon>
            </customUI>
            """;
        Assert.Throws<XmlException>(() => RibbonPreviewParser.Parse(xml));
    }

    [Test]
    public void RejectsMalformedXml()
    {
        Assert.Throws<XmlException>(() => RibbonPreviewParser.Parse("<customUI>"));
    }
}
