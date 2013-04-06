<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
  xmlns:utm="http://vortex.cis.vtc.edu/xml/UnitTestManager_0.0">

  <xsl:output method="html"/>

  <xsl:variable name="result.list" select="//utm:TestResult"/>
  <xsl:variable name="failure.list" select="$result.list/utm:Failure"/>
  <xsl:variable name="exception.list" select="$result.list/utm:Exception"/>
  <xsl:variable name="bad.return.list" select="$result.list/utm:BadReturn"/>
  <xsl:variable name="failure.count" select="count($failure.list)"/>
  <xsl:variable name="exception.count" select="count($exception.list)"/>
  <xsl:variable name="bad.return.count" select="count($bad.return.list)"/>

  <xsl:variable name="total.failure.count"
    select="$failure.count + $exception.count + $bad.return.count"/>

  <xsl:template match="/">
    <table class="section-table" cellpadding="2" cellspacing="0" border="0" width="98%">

      <!-- Unit Tests -->
      <tr>
        <td class="sectionheader" colspan="2">
          <xsl:value-of select="//utm:Title"/>, Tests run: <xsl:value-of
            select="count($result.list)"/>, Failures: <xsl:value-of select="$failure.count"/>,
          Exceptions: <xsl:value-of select="$exception.count"/>, Bad Returns: <xsl:value-of
            select="$bad.return.count"/></td>
      </tr>

      <xsl:choose>
        <xsl:when test="$total.failure.count = 0">
          <tr>
            <td colspan="2" class="section-data">All Tests Passed</td>
          </tr>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="//utm:TestResult"/>
        </xsl:otherwise>
      </xsl:choose>
    </table>
  </xsl:template>

  <!-- Format Test Result -->
  <xsl:template match="utm:TestResult">
    <xsl:variable name="local.failure.count" select="count(utm:Failure)"/>
    <xsl:variable name="local.exception.count" select="count(utm:Exception)"/>
    <xsl:variable name="local.bad.return.count" select="count(utm:BadReturn)"/>
    <xsl:variable name="local.total.failure.count"
      select="$local.failure.count + $local.exception.count + $local.bad.return.count"/>

    <xsl:if test="$local.total.failure.count > 0">
      <tr>
        <td class="section-data">TEST</td>
        <td class="section-data">
          <xsl:value-of select="@title"/>
        </td>
      </tr>
      <xsl:apply-templates select="utm:Failure"/>
      <xsl:apply-templates select="utm:Exception"/>
      <xsl:apply-templates select="utm:BadReturn"/>
      <tr>
        <td class="section-data" colspan="2">
          <xsl:text> </xsl:text>
        </td>
      </tr>
    </xsl:if>
  </xsl:template>

  <!-- Format Failure Message -->
  <xsl:template match="utm:Failure">
    <tr>
      <td class="section-data">Failure:</td>
      <td class="section-data">File: <xsl:value-of select="@file"/>, Line: <xsl:value-of
          select="@line"/>, Condition: <xsl:value-of select="."/></td>
    </tr>
  </xsl:template>

  <!-- Format Exception Message -->
  <xsl:template match="utm:Exception">
    <tr>
      <td class="section-data">Exception:</td>
      <td class="section-data">Type: <xsl:value-of select="@type"/>, Message: <xsl:value-of
          select="."/></td>
    </tr>
  </xsl:template>

  <!-- Format Bad Return Message -->
  <xsl:template match="utm:BadReturn">
    <tr>
      <td class="section-data">Bad Return:</td>
      <td class="section-data">Return Value: <xsl:value-of select="."/></td>
    </tr>
  </xsl:template>
</xsl:stylesheet>
