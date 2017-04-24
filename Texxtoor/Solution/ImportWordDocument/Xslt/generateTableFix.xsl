<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs" version="1.0">
    <xd:doc xmlns:xd="http://www.oxygenxml.com/ns/doc/xsl" scope="stylesheet">
        <xd:desc>
            <xd:p><xd:b>Created on:</xd:b> Nov 6, 2013</xd:p>
            <xd:p><xd:b>Author:</xd:b> Mahesh Pal Singh</xd:p>
            <xd:p/>
        </xd:desc>
    </xd:doc>
    <xsl:output indent="yes"></xsl:output>
    <xsl:template match="/">
            <xsl:text disable-output-escaping="yes">
                &lt;xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                    xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs" version="1.0"
                    xmlns:scr="urn:scr.this" xmlns:msxsl="urn:schemas-microsoft-com:xslt"&gt;
                &lt;msxsl:script language="C#" implements-prefix="scr"&gt;
                  &lt;![CDATA[ 
                  public string Replace(string stringToModify, string pattern, string replacement) 
                  { 
                     return System.Text.RegularExpressions.Regex.Replace(stringToModify, pattern, replacement); 
                  } 
                  public bool Matches(string stringToTest, string pattern) 
                  { 
                     return System.Text.RegularExpressions.Regex.Match(stringToTest, pattern).Success; 
                  }
                  ]]&gt; 
                &lt;/msxsl:script&gt;
                &lt;xsl:template match="node()|@*"&gt;        
                &lt;xsl:copy&gt;
                &lt;xsl:apply-templates select="node()|@*"&gt;&lt;/xsl:apply-templates&gt;
                &lt;/xsl:copy&gt;        
                &lt;/xsl:template&gt;
                &lt;xsl:template match="//Element[@Type='Text'][following-sibling::*[1]/@Type='Table']"&gt;
                &lt;xsl:choose&gt;
            </xsl:text>
            <xsl:for-each select="replaces/tables/replace">
                <xsl:variable name="word">
                    <xsl:value-of select="@word"/>
                </xsl:variable>
                <xsl:text disable-output-escaping="yes">&lt;xsl:when test="starts-with(strong[1],'</xsl:text>
                <xsl:value-of select="$word"/>
                <xsl:text disable-output-escaping="yes">')"&gt;&lt;/xsl:when&gt;</xsl:text>
            </xsl:for-each>
            <xsl:text disable-output-escaping="yes">
                &lt;xsl:otherwise&gt;&lt;xsl:copy-of select="."&gt;&lt;/xsl:copy-of&gt;&lt;/xsl:otherwise&gt;
                &lt;/xsl:choose&gt;        
                &lt;/xsl:template&gt;
            </xsl:text>
            <xsl:text disable-output-escaping="yes">
                &lt;xsl:template match="//Element[@Type='Table']"&gt;
                    &lt;xsl:element name="Element"&gt;
                        &lt;xsl:attribute name="Type"&gt;Table&lt;/xsl:attribute&gt;
                        &lt;xsl:attribute name="Name"&gt;
                            &lt;xsl:choose&gt;
            </xsl:text>
            <xsl:for-each select="replaces/tables/replace">
                <xsl:variable name="pattern">
                    <xsl:value-of select="@pattern"/>
                </xsl:variable>
                <xsl:text disable-output-escaping="yes">&lt;xsl:when test="scr:Matches(@Name,'</xsl:text>
                <xsl:value-of select="$pattern"/>
                <xsl:text disable-output-escaping="yes">')"&gt;&lt;xsl:value-of select="normalize-space(scr:Replace(@Name,'</xsl:text><xsl:value-of select="$pattern"/>
                <xsl:text disable-output-escaping="yes">',''))"/&gt;&lt;/xsl:when&gt;</xsl:text>
            </xsl:for-each>
                <xsl:text disable-output-escaping="yes">
                    &lt;xsl:otherwise&gt;
                        &lt;xsl:value-of select="@Name"&gt;&lt;/xsl:value-of&gt;
                    &lt;/xsl:otherwise&gt;
                    &lt;/xsl:choose&gt;                
                    &lt;/xsl:attribute&gt;
                    &lt;xsl:apply-templates&gt;&lt;/xsl:apply-templates&gt;
                    &lt;/xsl:element&gt;
                    &lt;/xsl:template&gt;
                </xsl:text>
            <xsl:text disable-output-escaping="yes">&lt;/xsl:stylesheet&gt;</xsl:text>
    </xsl:template>
</xsl:stylesheet>
