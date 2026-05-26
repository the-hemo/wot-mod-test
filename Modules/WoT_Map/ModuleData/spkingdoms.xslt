<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="xml" indent="yes"/>

    <xsl:template match="@* | node()">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()"/>
        </xsl:copy>
    </xsl:template>

	<xsl:template match="Kingdom[@id='sturgia']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_shayol</xsl:attribute>
	</xsl:template>
	<xsl:template match="Kingdom[@id='khuzait']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_maradon</xsl:attribute>
	</xsl:template>
	<xsl:template match="Kingdom[@id='aserai']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_aiel_1</xsl:attribute>
	</xsl:template>
	<xsl:template match="Kingdom[@id='battania']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_farmad</xsl:attribute>
	</xsl:template>
	<xsl:template match="Kingdom[@id='empire']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_caemlyn</xsl:attribute>
	</xsl:template>
	<xsl:template match="Kingdom[@id='empire_s']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_cairhien</xsl:attribute>
	</xsl:template>
	<xsl:template match="Kingdom[@id='empire_w']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_emonds</xsl:attribute>
	</xsl:template>
	<xsl:template match="Kingdom[@id='vlandia']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_falme</xsl:attribute>
	</xsl:template>
	
</xsl:stylesheet>
