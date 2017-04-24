<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:user="urn:my-scripts">
  <xsl:output method="xml" indent="yes" />
  <xsl:strip-space  elements="span"/>
  <xsl:param name="codeCharacter">ListingTextZchn</xsl:param>
  <xsl:param name="codePara">ListingText</xsl:param>
  <xsl:param name="listingCaption">Listingunterschrift</xsl:param>
  <xsl:param name="imageCaption">Bildunterschrift</xsl:param>
  <xsl:param name="tableCaption">Tabellenberschrift</xsl:param>
  <xsl:param name="sidebarHint">IconHinweisText</xsl:param>
  <xsl:param name="sidebarWarning">IconWarnungText</xsl:param>
  <xsl:param name="textPara">StandardAbsatz|AufzhlungEinrckung</xsl:param>
  <xsl:param name="bulletPara">Aufzhl1</xsl:param>
  <xsl:param name="numberPara">AufzhlNumber</xsl:param>
  <!--i.e., convert the attribute into element, it's value is related to p-->
  <xsl:key name="Body" match="p | table | img"
    use="generate-id((
            preceding-sibling::*/h1
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

  <xsl:key name="h2" match="h2" use="generate-id((parent::*/preceding-sibling::*/h1 | parent::*/preceding-sibling::h1 | preceding-sibling::*/h1 | preceding-sibling::h1)[last()])"/>
  <xsl:key name="h3" match="h3" use="generate-id((parent::*/preceding-sibling::*/h2 | parent::*/preceding-sibling::h2 | preceding-sibling::*/h2 | preceding-sibling::h2)[last()])"/>
  <xsl:key name="h4" match="h4" use="generate-id((parent::*/preceding-sibling::*/h3 | parent::*/preceding-sibling::h3 | preceding-sibling::*/h3 | preceding-sibling::h3)[last()])"/>
  <xsl:key name="h5" match="h5" use="generate-id((parent::*/preceding-sibling::*/h4 | parent::*/preceding-sibling::h4 | preceding-sibling::*/h4 | preceding-sibling::h4)[last()])"/>
  <xsl:key name="h6" match="h6" use="generate-id((parent::*/preceding-sibling::*/h5 | parent::*/preceding-sibling::h5 | preceding-sibling::*/h5 | preceding-sibling::h5)[last()])"/>

  <!-- head not needed -->
  <xsl:template match="head">    
  </xsl:template>

  <!--   Wendell Piez algorithm -->
  <xsl:template match="body">
    <Content Type="Opus">
      <xsl:attribute name="Name">
        <xsl:value-of select="//head/title"/>
      </xsl:attribute>      
      <xsl:apply-templates select="key('Body',generate-id())" mode="Body"/>
      <xsl:apply-templates select="h1|*/h1" mode="h1"/>
    </Content>
  </xsl:template>

  <xsl:template match="h1" mode="h1">
    <Element Type="Section" Level="1">
      <xsl:attribute name="Name">
        <xsl:value-of select="normalize-space(.)" />
      </xsl:attribute>
      <xsl:text>&#xa;</xsl:text>
      <xsl:value-of select="normalize-space(.)" />
      <xsl:apply-templates select="key('Body',generate-id())" mode="Body"/>
      <xsl:apply-templates select="key('h2',generate-id())" mode="h2"/>
    </Element>
  </xsl:template>

  <xsl:template match="h2" mode="h2">
    <xsl:text>&#xa;</xsl:text>
    <Element Type="Section" Level="2">
      <xsl:attribute name="Name">
        <xsl:value-of select="normalize-space(.)" />
      </xsl:attribute>
      <xsl:text>&#xa;</xsl:text>
      <xsl:value-of select="normalize-space(.)" />
      <xsl:apply-templates select="key('Body',generate-id())" mode="Body"/>
      <xsl:apply-templates select="key('h3',generate-id())" mode="h3"/>
    </Element>
  </xsl:template>

  <xsl:template match="h3" mode="h3">
    <xsl:text>&#xa;</xsl:text>
    <Element Type="Section" Level="3">
      <xsl:attribute name="Name">
        <xsl:value-of select="normalize-space(.)" />
      </xsl:attribute>
      <xsl:text>&#xa;</xsl:text>
      <xsl:value-of select="normalize-space(.)" />
      <xsl:apply-templates select="key('Body',generate-id())" mode="Body"/>
      <xsl:apply-templates select="key('h4',generate-id())" mode="h4"/>
    </Element>
  </xsl:template>

  <xsl:template match="h4" mode="h4">
    <xsl:text>&#xa;</xsl:text>
    <Element Type="Section" Level="4">
      <xsl:attribute name="Name">
        <xsl:value-of select="normalize-space(.)" />
      </xsl:attribute>
      <xsl:text>&#xa;</xsl:text>
      <xsl:value-of select="normalize-space(.)" />
      <xsl:apply-templates select="key('Body',generate-id())" mode="Body"/>
      <xsl:apply-templates select="key('h5',generate-id())" mode="h5"/>
    </Element>
  </xsl:template>

  <xsl:template match="h5" mode="h5">
    <Element Type="Section" Level="5">
      <xsl:attribute name="Name">
        <xsl:value-of select="normalize-space(.)" />
      </xsl:attribute>
      <xsl:text>&#xa;</xsl:text>
      <xsl:value-of select="normalize-space(.)" />
      <xsl:apply-templates select="key('Body',generate-id())" mode="Body"/>
      <xsl:apply-templates select="key('h6',generate-id())" mode="h6"/>
    </Element>
  </xsl:template>

  <xsl:template match="h6" mode="h6">
    <Element Type="Section" Level="6">
      <xsl:attribute name="Name">
        <xsl:value-of select="normalize-space(.)" />
      </xsl:attribute>
      <xsl:text>&#xa;</xsl:text>
      <xsl:apply-templates/>
      <xsl:apply-templates select="key('Body',generate-id())" mode="Body"/>
    </Element>
  </xsl:template>

  <xsl:key name="listingNodes"
           match="p"
           use="generate-id(preceding-sibling::node[@id][1])" />

  <xsl:key name="kFollowing"
          match="p[not(img)]"
          use="generate-id(preceding-sibling::node()[not(self::p)][1])"/>

  <xsl:template match="p[not(img)]" mode="Body">
    <xsl:choose>
      <!-- Regular Listings with Caption-->
      <xsl:when test="@class=$listingCaption">
        <xsl:text>&#xa;</xsl:text>
        <Element Type="Listing">
          <xsl:attribute name="Name">
            <xsl:value-of select="span/text()" />
          </xsl:attribute>
          <xsl:call-template name="listingItems">
            <xsl:with-param select="(parent::node()/following-sibling::div/p[@class=$codePara])[1]" name="node" />
          </xsl:call-template>
        </Element>
      </xsl:when>
      <xsl:when test="@class=$codePara">
        <xsl:text>&#xa;</xsl:text>
        <Element Type="Listing">
          <xsl:attribute name="Name"></xsl:attribute>
          <xsl:call-template name="listingItems">
            <xsl:with-param name="node" select="following-sibling::p[@class=$codePara]" />
          </xsl:call-template>
        </Element>
      </xsl:when>
      <!-- Sidebars -->
      <xsl:when test="@class=$sidebarHint">
        <Element Type="Sidebar" SidebarType="Note">
          <header>Note</header>
          <aside>
            <xsl:apply-templates />
          </aside>
        </Element>
      </xsl:when>
      <xsl:when test="@class=$sidebarWarning">
        <Element Type="Sidebar" SidebarType="Warning">
          <header>Note</header>
          <aside>
            <xsl:apply-templates />
          </aside>
        </Element>
      </xsl:when>
      <xsl:when test="contains($textPara, @class)">
            <xsl:text>&#xa;</xsl:text>
        <Element Type="Text">
          <p><xsl:apply-templates /></p>
        </Element>
      </xsl:when>
      <xsl:when test="@class=$bulletPara">
        <xsl:text>&#xa;</xsl:text>
        <Element Type="Text">
          <ul>
            <li>
              <xsl:choose>
                <xsl:when test="starts-with(normalize-space(.), 'n  ')">
                  <xsl:value-of select="substring(normalize-space(.), 2)"/>
                </xsl:when>
      <xsl:otherwise>
                  <xsl:value-of select="normalize-space(.)"/>                  
                </xsl:otherwise>
              </xsl:choose>
            </li>
          </ul>
        </Element>
      </xsl:when>
      <xsl:when test="@class=$numberPara">
        <xsl:text>&#xa;</xsl:text>
        <Element Type="Text">
          <ol>
            <li>
              <xsl:apply-templates />
            </li>
          </ol>
        </Element>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="listingItems">
    <xsl:param name="node" />
    <xsl:value-of select="normalize-space($node)"/>        
  </xsl:template>

  <!-- Table Handling-->

  <xsl:template match="table" mode="Body">
    <xsl:text>&#xa;</xsl:text>
    <Element Type="Table">
      <xsl:attribute name="Name">
        <!-- assume <p>caption</p><table> -->
        <xsl:value-of select="normalize-space(preceding-sibling::p[@class=$tableCaption][1])"/>
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
      <xsl:apply-templates/>
    </td>
  </xsl:template>

  <xsl:template match="img" mode="Body">
    <xsl:variable name="Base64">data:image/png;base64,</xsl:variable>
    <Element Type="Image">
      <xsl:attribute name="Name">
        <!-- assume <p><img><p>caption</p> -->
        <xsl:value-of select="normalize-space(parent::node()/following-sibling::p[@class=$imageCaption])"/>
      </xsl:attribute>
      <xsl:attribute name="Width">
        <xsl:value-of select="@width"/>
      </xsl:attribute>
      <xsl:attribute name="Height">
        <xsl:value-of select="@height"/>
      </xsl:attribute>
      <xsl:apply-templates/>
      <xsl:choose>
        <xsl:when test="starts-with(@src, $Base64)">
          <xsl:attribute name="Method">
            <xsl:text>Base64</xsl:text>
          </xsl:attribute>
          <xsl:text disable-output-escaping="yes">&lt;![CDATA[</xsl:text>
          <xsl:value-of select="substring-after(@src, ',')" disable-output-escaping="yes"/>
          <xsl:text disable-output-escaping="yes">]]&gt;</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="Path">
            <xsl:value-of select="@src"/>
          </xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
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

  <xsl:template match="ul" >
    <ul>
      <xsl:apply-templates/>
    </ul>
  </xsl:template>

  <xsl:template match="ol" >
    <ol>
      <xsl:apply-templates/>
    </ol>
  </xsl:template>

  <xsl:template match="li" >
    <li>
      <xsl:apply-templates/>
    </li>
  </xsl:template>

  <!-- Bullet points and num lists based on word styles  -->

  <xsl:template name="bullet">
    <li>
      <xsl:apply-templates/>
    </li>
  </xsl:template>

  <!-- Special char formatting -->
  <xsl:template match="span">
    <xsl:choose>
      <xsl:when test="@class=$codeCharacter">
        <code><xsl:value-of select="normalize-space(.)"/></code>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="normalize-space(.)"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>