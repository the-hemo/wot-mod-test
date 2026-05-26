<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output omit-xml-declaration="yes"/>
<xsl:template match="@*|node()">
    <xsl:copy>
        <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
</xsl:template>
 
    <xsl:template match="NPCCharacters[@id='mountain_bandits_bandit']"/>
    <xsl:template match="NPCCharacters[@id='mountain_bandits_raider']"/>
    <xsl:template match="NPCCharacters[@id='mountain_bandits_chief']"/>
    <xsl:template match="NPCCharacters[@id='desert_bandits_bandit']"/>
    <xsl:template match="NPCCharacters[@id='desert_bandits_raider']"/>
    <xsl:template match="NPCCharacters[@id='desert_bandits_chief']"/>
    <xsl:template match="NPCCharacters[@id='mountain_bandits_boss']"/>
    <xsl:template match="NPCCharacters[@id='desert_bandits_boss']"/>



</xsl:stylesheet>

