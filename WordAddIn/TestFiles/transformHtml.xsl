<?xml version="1.0"?>
<!--
  <ol> (C# Function)
  Test Zusammenfassung Text
  Inhalt und Gruppierung <aside>
  Figure nicht da!!
  Chapter (Level 1) Name
  Leere Paras entfernen
  Präfixe weg für zusätzliche Namespaces
  
-->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:scr="urn:scr.this"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                exclude-result-prefixes="xs">
  <xsl:output method="xml" indent="yes" />
  <xsl:strip-space  elements="span"/>
  <xsl:param name="codeCharacterTemplate">ListingTextZchn</xsl:param>
  <xsl:param name="codeListingTemplate">Listing</xsl:param>
  <xsl:param name="listingCaptionTemplate">ListingCaption</xsl:param>
  <xsl:param name="figureCaptionTemplate">FigureCaption</xsl:param>
  <xsl:param name="tableCaptionTemplate">TableCaption</xsl:param>
  <xsl:param name="tempPath">E:\Temp\</xsl:param>

  <msxsl:script language="C#" implements-prefix="scr">
    <![CDATA[
  public string GetBase64(string prefix, string pathName)
  {
    var path = System.IO.Path.Combine(prefix, pathName);
    if (!System.IO.File.Exists(path)) return String.Empty;
    return Convert.ToBase64String(System.IO.File.ReadAllBytes(path)).Substring(0, 2);
  }  
  ]]>

  </msxsl:script>
  <!--i.e., convert the attribute into element, it's value is related to p-->
  <xsl:key name="Body" match="p | table | img"
    use="generate-id((preceding-sibling::*/h1
           |preceding-sibling::*/h2
           |preceding-sibling::*/h3
           |preceding-sibling::*/h4
           |preceding-sibling::*/h5
           |preceding-sibling::*/h6
           |preceding-sibling::h1
           |preceding-sibling::h2
           |preceding-sibling::h3
           |preceding-sibling::h4
           |preceding-sibling::h5
           |preceding-sibling::h6)[last()])" />

  <xsl:key name="h2" match="h2" use="generate-id((parent::*/preceding-sibling::*/h1 | parent::*/preceding-sibling::h1 | preceding-sibling::*/h1 | preceding-sibling::h1)[1])"/>
  <xsl:key name="h3" match="h3" use="generate-id((parent::*/preceding-sibling::*/h2 | parent::*/preceding-sibling::h2 | preceding-sibling::*/h2 | preceding-sibling::h2)[1])"/>
  <xsl:key name="h4" match="h4" use="generate-id((parent::*/preceding-sibling::*/h3 | parent::*/preceding-sibling::h3 | preceding-sibling::*/h3 | preceding-sibling::h3)[1])"/>
  <xsl:key name="h5" match="h5" use="generate-id((parent::*/preceding-sibling::*/h4 | parent::*/preceding-sibling::h4 | preceding-sibling::*/h4 | preceding-sibling::h4)[1])"/>
  <xsl:key name="h6" match="h6" use="generate-id((parent::*/preceding-sibling::*/h5 | parent::*/preceding-sibling::h5 | preceding-sibling::*/h5 | preceding-sibling::h5)[1])"/>

  <!--   Wendell Piez algorithm -->
  <xsl:template match="body">
    <Content>
      <xsl:apply-templates select="key('Body',generate-id())" mode="Body"/>
      <xsl:apply-templates select="*/h1" mode="h1"/>
    </Content>
  </xsl:template>

  <xsl:template match="h1" mode="h1">
    <Element Type="Section" Level="1">
      <xsl:attribute name="Name">
        <xsl:value-of select="normalize-space(node()/following-sibling::span[last()]/text())" />
      </xsl:attribute>
      <xsl:text>&#xa;</xsl:text>
      <xsl:value-of select="normalize-space(node()/following-sibling::span[last()]/text())" />
      <xsl:apply-templates mode="Body"/>
      <xsl:apply-templates select="key('Body',generate-id())" mode="Body"/>
      <xsl:apply-templates select="key('h2',generate-id())" mode="h2"/>
    </Element>
  </xsl:template>

  <xsl:template match="h2" mode="h2">
    <xsl:text>&#xa;</xsl:text>
    <Element Type="Section" Level="2">
      <xsl:attribute name="Name">
        <xsl:value-of select="normalize-space(node()/following-sibling::span[last()]/text())" />
      </xsl:attribute>
      <xsl:text>&#xa;</xsl:text>
      <xsl:value-of select="normalize-space(node()/following-sibling::span[last()]/text())" />
      <xsl:apply-templates mode="Body"/>
      <xsl:apply-templates select="key('Body',generate-id())" mode="Body"/>
      <xsl:apply-templates select="key('h3',generate-id())" mode="h3"/>
    </Element>
  </xsl:template>

  <xsl:template match="h3" mode="h3">
    <xsl:text>&#xa;</xsl:text>
    <Element Type="Section" Level="3">
      <xsl:attribute name="Name">
        <xsl:value-of select="normalize-space(node()/following-sibling::span[last()]/text())" />
      </xsl:attribute>
      <xsl:text>&#xa;</xsl:text>
      <xsl:value-of select="normalize-space(node()/following-sibling::span[last()]/text())" />
      <xsl:apply-templates mode="Body"/>
      <xsl:apply-templates select="key('Body',generate-id())" mode="Body"/>
      <xsl:apply-templates select="key('h4',generate-id())" mode="h4"/>
    </Element>
  </xsl:template>

  <xsl:template match="h4" mode="h4">
    <xsl:text>&#xa;</xsl:text>
    <Element Type="Section" Level="4">
      <xsl:attribute name="Name">
        <xsl:value-of select="normalize-space(node()/following-sibling::span[last()]/text())" />
      </xsl:attribute>
      <xsl:text>&#xa;</xsl:text>
      <xsl:value-of select="normalize-space(node()/following-sibling::span[last()]/text())" />
      <xsl:apply-templates mode="Body"/>
      <xsl:apply-templates select="key('Body',generate-id())" mode="Body"/>
      <xsl:apply-templates select="key('h5',generate-id())" mode="h5"/>
    </Element>
  </xsl:template>

  <xsl:template match="h5" mode="h5">
    <xsl:text>&#xa;</xsl:text>
    <Element Type="Section" Level="5">
      <xsl:attribute name="Name">
        <xsl:value-of select="normalize-space(node()/following-sibling::span[last()]/text())" />
      </xsl:attribute>
      <xsl:text>&#xa;</xsl:text>
      <xsl:value-of select="normalize-space(node()/following-sibling::span[last()]/text())" />
      <xsl:apply-templates mode="Body"/>
      <xsl:apply-templates select="key('Body',generate-id())" mode="Body"/>
      <xsl:apply-templates select="key('h6',generate-id())" mode="h6"/>
    </Element>
  </xsl:template>

  <xsl:template match="h6" mode="h6">
    <xsl:text>&#xa;</xsl:text>
    <Element Type="Section" Level="6">
      <xsl:attribute name="Name">
        <xsl:value-of select="normalize-space(node()/following-sibling::span[last()]/text())" />
      </xsl:attribute>
      <xsl:text>&#xa;</xsl:text>
      <xsl:value-of select="normalize-space(node()/following-sibling::span[last()]/text())" />
      <xsl:apply-templates mode="Body"/>
      <xsl:apply-templates select="key('Body',generate-id())" mode="Body"/>
    </Element>
  </xsl:template>

  <xsl:key name="listingNodes"
           match="p"
           use="generate-id(((following-sibling::node())/*/parent::p[@class='Listing']))" />

  <xsl:key name="kFollowing"
          match="p[not(img)]"
          use="generate-id(preceding-sibling::node()[not(self::p)][1])"/>

  <xsl:template match="p" mode="Body">
    <xsl:choose>
      <xsl:when test="node()/img">
        <xsl:apply-templates />
      </xsl:when>
      <!-- Regular Listings with Caption-->
      <xsl:when test="@class=$listingCaptionTemplate">
        <xsl:text>&#xa;</xsl:text>
        <Element Type="Listing">
          <xsl:attribute name="Name">
            <xsl:value-of select="normalize-space(node()[last()]/text())" />
          </xsl:attribute>
          <pre>
            <xsl:for-each select="key('listingNodes', generate-id())">
              <xsl:value-of select="." disable-output-escaping="yes"/>
            </xsl:for-each>
          </pre>
          <!--<xsl:call-template name="listingItems">
            <xsl:with-param select="(parent::node()/following-sibling::div/p[@class=$codeListingTemplate])[1]" name="node" />
          </xsl:call-template>-->
        </Element>
      </xsl:when>
      <xsl:when test="@class='SideBarHeader'">
        <xsl:text disable-output-escaping="yes">
          &lt;Element Type="Sidebar"
        </xsl:text>
        <xsl:choose>
          <!-- Keep as Choice as we need to localize the test -->
          <xsl:when test="node()/text() = 'Note'">
            <xsl:text disable-output-escaping="yes">SidebarType="Note" &gt;</xsl:text>
            <header>
              <xsl:value-of select="node()/text()"/>
            </header>
          </xsl:when>
          <xsl:when test="node()/text() = 'Warning'">
            <xsl:text disable-output-escaping="yes">SidebarType="Warning" &gt;</xsl:text>
            <header>
              <xsl:value-of select="node()/text()"/>
            </header>
          </xsl:when>
          <xsl:when test="node()/text() = 'Tip'">
            <xsl:text disable-output-escaping="yes">SidebarType="Hint" &gt;</xsl:text>
            <header>
              <xsl:value-of select="node()/text()"/>
            </header>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text disable-output-escaping="yes">SidebarType="Box" &gt;</xsl:text>
            <header>
              <xsl:value-of select="node()/text()"/>
            </header>
          </xsl:otherwise>
        </xsl:choose>
        <aside>
          <p>
            <xsl:apply-templates select="node()/parent::*/following-sibling::p[@class='SideBarContent']" />
          </p>
        </aside>
        <xsl:text disable-output-escaping="yes">
          &lt;/Element&gt;
        </xsl:text>
      </xsl:when>
      <xsl:when test="@class='MsoListParagraphCxSpFirst'">
        <xsl:text disable-output-escaping="yes">
          &lt;Element Type="Text"&gt;
          &lt;ul&gt;
        </xsl:text>
        <li>
          <xsl:apply-templates select="node()/following-sibling::span"/>
        </li>
      </xsl:when>
      <xsl:when test="@class='MsoListParagraphCxSpMiddle'">
        <li>
          <xsl:apply-templates select="node()/following-sibling::span"/>
        </li>
      </xsl:when>
      <xsl:when test="@class='MsoListParagraphCxSpLast'">
        <li>
          <xsl:apply-templates select="node()/following-sibling::span"/>
        </li>
        <xsl:text disable-output-escaping="yes">
          &lt;/ul&gt;
          &lt;/Element&gt;
        </xsl:text>
      </xsl:when>
      <xsl:when test="@class='MsoNormal'">
        <xsl:text>&#xa;</xsl:text>
        <Element Type="Text">
          <p>
            <xsl:value-of select="normalize-space(node()/text())" />
          </p>
        </Element>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="listingItems">
    <xsl:param name="node" />
    <pre>
      <xsl:for-each select="$node/following-sibling::p[@class=$codeListingTemplate][generate-id(preceding-sibling::p) = generate-id(current())]">
        <xsl:value-of select="text()"/>
        <xsl:text>&#xa;</xsl:text>
      </xsl:for-each>
    </pre>
  </xsl:template>

  <xsl:template match="table" mode="Body">
    <Element Type="Table">
      <xsl:attribute name="Name">
        <!-- assume <p>caption</p><table> -->
        <xsl:value-of select="normalize-space(preceding-sibling::p[@class=$tableCaptionTemplate]/span[last()])"/>
      </xsl:attribute>
      <table>
        <xsl:apply-templates/>
      </table>
    </Element>
  </xsl:template>

  <xsl:template match="thead">
    <thead>
      <xsl:apply-templates/>
    </thead>
  </xsl:template>

  <xsl:template match="tbody" >
    <tbody>
      <xsl:apply-templates/>
    </tbody>
  </xsl:template>

  <xsl:template match="tr" >
    <tr>
      <xsl:apply-templates/>
    </tr>
  </xsl:template>

  <xsl:template match="td" >
    <td>
      <xsl:attribute name="width">
        <xsl:value-of select="@width"/>
      </xsl:attribute>
      <xsl:apply-templates mode="Table" />
    </td>
  </xsl:template>

  <xsl:template match="img">
    <xsl:variable select="msxsl:node-set($tempPath)/text()" name="path" />
    <Element Type="Image">
      <xsl:attribute name="Name">
        <!-- assume <p><img><p>caption</p> -->
        <xsl:value-of select="normalize-space(ancestor::p/following-sibling::p[@class='FigureCaption']/node()[last()]/text())"/>
      </xsl:attribute>
      <xsl:attribute name="ExportPath">
        <xsl:value-of select="@src"/>
      </xsl:attribute>
      <xsl:attribute name="Width">
        <xsl:value-of select="@width"/>
      </xsl:attribute>
      <xsl:attribute name="Height">
        <xsl:value-of select="@height"/>
      </xsl:attribute>
      <xsl:text disable-output-escaping="yes">&lt;![CDATA[</xsl:text>
      <xsl:value-of select="scr:GetBase64($path, @src)" disable-output-escaping="yes"/>
      <xsl:text disable-output-escaping="yes">]]&gt;</xsl:text>
    </Element>
  </xsl:template>

  <xsl:template match="b" >
    <strong>
      <xsl:apply-templates/>
    </strong>
  </xsl:template>

  <xsl:template match="i" >
    <em>
      <xsl:apply-templates/>
    </em>
  </xsl:template>

  <xsl:template match="u" >
    <u>
      <xsl:apply-templates/>
    </u>
  </xsl:template>

  <xsl:template match="code" >
    <code><xsl:value-of select="text()"/></code>
  </xsl:template>

  <xsl:template match="kbd" >
    <kbd><xsl:value-of select="text()"/></kbd>
  </xsl:template>

  <!-- Special char formatting -->
  <xsl:template match="span" mode="Body">
    <xsl:choose>
      <xsl:when test="./following-sibling::code">
        <xsl:apply-templates />
      </xsl:when>
      <xsl:when test="./following-sibling::kbd">
        <xsl:apply-templates />
      </xsl:when>
      <xsl:otherwise>
        <!--<xsl:apply-templates/>-->
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="span" mode="Table">
    <xsl:apply-templates/>
  </xsl:template>

</xsl:stylesheet>