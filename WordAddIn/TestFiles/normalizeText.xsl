<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs" version="2.0">
    <xd:doc xmlns:xd="http://www.oxygenxml.com/ns/doc/xsl" scope="stylesheet">
        <xd:desc>
            <xd:p><xd:b>Created on:</xd:b>Oct 25, 2013</xd:p>
            <xd:p><xd:b>Author:</xd:b>Mahesh Pal Singh</xd:p>
            <xd:p>This XSLT is first among series of XSLT to be applied to normalize HTML output from word</xd:p>
        </xd:desc>
    </xd:doc>
    <!-- Following is the identity Template that will replicate the original input XML except for specific templates below -->
    <xsl:template match="node()|@*">        
        <xsl:copy>
            <xsl:apply-templates select="node()|@*"></xsl:apply-templates>
        </xsl:copy>        
    </xsl:template> 
    <!--Templace to conjuct all element with name Element and type attribute text-->
    <!--Logic here is pick all contiguous Element element with class type Text and group them under a parent -->
    <xsl:template match="//Element[@Type='Text']">
        <xsl:choose>            
            <xsl:when test="not(preceding-sibling::*[1][local-name()='Element'][@Type='Text'])and following-sibling::*[1][local-name()='Element'][@Type='Text']">
                <xsl:text disable-output-escaping="yes">&lt;Element Type="Text"&gt;</xsl:text><xsl:copy-of select="*|text()"></xsl:copy-of>
            </xsl:when>
            <xsl:when test="preceding-sibling::*[1][local-name()='Element'][@Type='Text'] and following-sibling::*[1][local-name()='Element'][@Type='Text']">
                <xsl:copy-of select="*|text()"></xsl:copy-of>
            </xsl:when>
            <xsl:when test="preceding-sibling::*[1][local-name()='Element'][@Type='Text'] and not(following-sibling::*[1][local-name()='Element'][@Type='Text'])">
                <xsl:copy-of select="*|text()"></xsl:copy-of><xsl:text disable-output-escaping="yes">&lt;/Element&gt;</xsl:text><xsl:text>&#x00a;</xsl:text>
            </xsl:when>
            <xsl:when test="not(preceding-sibling::*[1][local-name()='Element'][@Type='Text']) and not(following-sibling::*[1][local-name()='Element'][@Type='Text'])">
                <xsl:copy-of select="."></xsl:copy-of>
            </xsl:when>
            <xsl:otherwise>
                <xsl:copy-of select="."></xsl:copy-of>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
</xsl:stylesheet>
