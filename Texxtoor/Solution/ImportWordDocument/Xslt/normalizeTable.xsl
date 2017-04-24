<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs" version="1.0"
    xmlns:scr="urn:scr.this" xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <msxsl:script language="C#" implements-prefix="scr">
    <![CDATA[ 
    public string Replace(string stringToModify, string pattern, string replacement) 
    { 
        return System.Text.RegularExpressions.Regex.Replace(stringToModify, pattern, replacement); 
    } 
    public bool Matches(string stringToTest, string pattern) 
    { 
        return System.Text.RegularExpressions.Regex.Match(stringToTest, pattern).Success; 
    }
    ]]>
  </msxsl:script>
  <xsl:template match="node()|@*">
    <xsl:copy>
      <xsl:apply-templates select="node()|@*"></xsl:apply-templates>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="//Element[@Type='Text'][following-sibling::*[1]/@Type='Table']">
    <xsl:choose>
      <xsl:when test="starts-with(strong[1],'Tabelle')"></xsl:when>
      <xsl:when test="starts-with(strong[1],'Table')"></xsl:when>
      <xsl:when test="starts-with(strong[1],'Tab')"></xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select="."></xsl:copy-of>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="//Element[@Type='Table']">
    <xsl:element name="Element">
      <xsl:attribute name="Type">Table</xsl:attribute>
      <xsl:attribute name="Name">
        <xsl:choose>
          <xsl:when test="scr:Matches(@Name,'Tabelle\s*[0-9]+\.[0-9]+\s*')">
            <xsl:value-of select="normalize-space(scr:Replace(@Name,'Tabelle\s*[0-9]+\.[0-9]+\s*',''))"/>
          </xsl:when>
          <xsl:when test="scr:Matches(@Name,'Table\s*[0-9]+\.[0-9]+\s*')">
            <xsl:value-of select="normalize-space(scr:Replace(@Name,'Table\s*[0-9]+\.[0-9]+\s*',''))"/>
          </xsl:when>
          <xsl:when test="scr:Matches(@Name,'Tab\s*[0-9]+\.[0-9]+\s*')">
            <xsl:value-of select="normalize-space(scr:Replace(@Name,'Tab\s*[0-9]+\.[0-9]+\s*',''))"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="@Name"></xsl:value-of>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <xsl:apply-templates></xsl:apply-templates>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>