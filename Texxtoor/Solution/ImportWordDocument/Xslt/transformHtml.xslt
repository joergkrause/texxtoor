<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" indent="yes" />
  <xsl:strip-space  elements="span"/>
  <xsl:param name="codeCharacterTemplate">ListingTextZchn</xsl:param>
  <xsl:param name="codeListingTemplate">ListingText</xsl:param>
  <xsl:param name="listingCaptionTemplate">Listingunterschrift</xsl:param>
  <xsl:param name="figureCaptionTemplate">Bildunterschrift</xsl:param>
  <xsl:param name="tableCaptionTemplate">Tabellenberschrift</xsl:param>
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
        <xsl:value-of select="normalize-space(.)" />
      </xsl:attribute>
      <xsl:text>&#xa;</xsl:text>
      <xsl:apply-templates/>
      <xsl:apply-templates select="key('Body',generate-id())" mode="Body"/>
      <xsl:apply-templates select="key('h2',generate-id())" mode="h2"/>
    </Element>
  </xsl:template>

  <xsl:template match="h2" mode="h2">
    <Element Type="Section" Level="2">
      <xsl:attribute name="Name">
        <xsl:value-of select="normalize-space(.)" />
      </xsl:attribute>
      <xsl:text>&#xa;</xsl:text>
      <xsl:apply-templates/>
      <xsl:apply-templates select="key('Body',generate-id())" mode="Body"/>
      <xsl:apply-templates select="key('h3',generate-id())" mode="h3"/>
    </Element>
  </xsl:template>

  <xsl:template match="h3" mode="h3">
    <Element Type="Section" Level="3">
      <xsl:attribute name="Name">
        <xsl:value-of select="." />
      </xsl:attribute>
      <xsl:text>&#xa;</xsl:text>
      <xsl:apply-templates/>
      <xsl:apply-templates select="key('Body',generate-id())" mode="Body"/>
      <xsl:apply-templates select="key('h4',generate-id())" mode="h4"/>
    </Element>
  </xsl:template>

  <xsl:template match="h4" mode="h4">
    <Element Type="Section" Level="4">
      <xsl:attribute name="Name">
        <xsl:value-of select="." />
      </xsl:attribute>
      <xsl:text>&#xa;</xsl:text>
      <xsl:apply-templates/>
      <xsl:apply-templates select="key('Body',generate-id())" mode="Body"/>
      <xsl:apply-templates select="key('h5',generate-id())" mode="h5"/>
    </Element>
  </xsl:template>

  <xsl:template match="h5" mode="h5">
    <Element Type="Section" Level="5">
      <xsl:attribute name="Name">
        <xsl:value-of select="." />
      </xsl:attribute>
      <xsl:text>&#xa;</xsl:text>
      <xsl:apply-templates/>
      <xsl:apply-templates select="key('Body',generate-id())" mode="Body"/>
      <xsl:apply-templates select="key('h6',generate-id())" mode="h6"/>
    </Element>
  </xsl:template>

  <xsl:template match="h6" mode="h6">
    <Element Type="Section" Level="6">
      <xsl:attribute name="Name">
        <xsl:value-of select="." />
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
      <xsl:when test="@class=$listingCaptionTemplate">
        <xsl:text>&#xa;</xsl:text>
        <Element Type="Listing">
          <xsl:attribute name="Name">
            <xsl:value-of select="span/text()" />
          </xsl:attribute>
          <xsl:call-template name="listingItems">
            <xsl:with-param select="(parent::node()/following-sibling::div/p[@class=$codeListingTemplate])[1]" name="node" />
          </xsl:call-template>
            
          <!--<xsl:copy-of select="key('listingNodes', generate-id(p[@class=$codeListingTemplate]))/text()" />-->
        </Element>
      </xsl:when>
      <!-- Listings that appear like listings but doesn't have caption -->
      <!--
      <xsl:when test="@class=$codeListingTemplate">
        <xsl:text>&#xa;</xsl:text>
        <Element Type="Listing">
          <xsl:attribute name="Name"></xsl:attribute>
          <xsl:for-each select="following-sibling::p[@class=$codeListingTemplate]">
            <xsl:value-of select="."/>
            <xsl:text>&#xa;</xsl:text>
          </xsl:for-each>
          -->
      <!--<xsl:copy-of select="key('listingNodes', generate-id(p[@class=$codeListingTemplate]))/text()" />-->
      <!--
        </Element>
      </xsl:when>-->
      <xsl:otherwise>
        <xsl:text>&#xa;</xsl:text>
        <Element Type="Text">
          <!--trying to combine p sequences-->
          <xsl:apply-templates select="node() | key('kFollowing', generate-id(preceding-sibling::node()[1]))/node()"/>
        </Element>
      </xsl:otherwise>
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
        <xsl:value-of select="normalize-space(preceding-sibling::p[@class=$tableCaptionTemplate][1])"/>
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
    <Element Type="Image">
      <xsl:attribute name="Name">
        <!-- assume <p><img><p>caption</p> -->
        <xsl:value-of select="normalize-space(parent::*/following-sibling::p[1])"/>
      </xsl:attribute>
      <xsl:attribute name="Path">
        <xsl:value-of select="@src"/>
      </xsl:attribute>
      <xsl:attribute name="Width">
        <xsl:value-of select="@width"/>
      </xsl:attribute>
      <xsl:attribute name="Height">
        <xsl:value-of select="@height"/>
      </xsl:attribute>
      <xsl:apply-templates/>
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

  <!-- Special char formatting -->
  <xsl:template match="span">
    <xsl:choose>
      <xsl:when test="@class=$codeCharacterTemplate">
        <code>
          <xsl:value-of select="."/>
        </code>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>