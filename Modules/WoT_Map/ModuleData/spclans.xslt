<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output omit-xml-declaration="yes"/>
    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()"/>
        </xsl:copy>
    </xsl:template>
	<!--Minor Clan Clean up-->
	<xsl:template match="Faction[@id='ghilman']"/>
	<xsl:template match="Faction[@id='legion_of_the_betrayed']"/>
	<xsl:template match="Faction[@id='skolderbrotva']"/>
	<xsl:template match="Faction[@id='company_of_the_boar']"/>
	<xsl:template match="Faction[@id='beni_zilal']"/>
	<xsl:template match="Faction[@id='wolfskins']"/>
	<xsl:template match="Faction[@id='brotherhood_of_woods']"/>
	<xsl:template match="Faction[@id='hidden_hand']"/>
	<xsl:template match="Faction[@id='lakepike']"/>
	<xsl:template match="Faction[@id='embers_of_flame']"/>
	<xsl:template match="Faction[@id='jawwal']"/>
	<xsl:template match="Faction[@id='karakhuzaits']"/>
	<xsl:template match="Faction[@id='forest_people']"/>
	<xsl:template match="Faction[@id='eleftheroi']"/>
</xsl:stylesheet>