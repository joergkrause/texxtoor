<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" indent="yes" />

  <xsl:template match="/">
    <html>
      <xsl:apply-templates />
    </html>
  </xsl:template>

  <xsl:template match="body">
    <body>
      <xsl:apply-templates />
    </body>
  </xsl:template>

  <xsl:template match="title">
    <head>
      <title>
        <xsl:value-of select="normalize-space(.)" />
      </title>
    </head>
  </xsl:template>

  <xsl:template match="div">
    <xsl:copy-of select="child::node()" />
  </xsl:template>

</xsl:stylesheet>