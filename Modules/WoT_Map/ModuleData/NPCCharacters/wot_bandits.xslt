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
	<xsl:template match="NPCCharacters[@id='looter']"/>

<!--Pirates-->
	<xsl:template match="NPCCharacter[@id='sea_raiders_bandit']/@name">
		<xsl:attribute name="name">Pirate</xsl:attribute>
	</xsl:template>
	<xsl:template match="NPCCharacter[@id='sea_raiders_raider']/@name">
		<xsl:attribute name="name">Seasoned Pirate</xsl:attribute>
	</xsl:template>
	<xsl:template match="NPCCharacter[@id='sea_raiders_chief']/@name">
		<xsl:attribute name="name">Pirate Boss</xsl:attribute>
	</xsl:template>
<!--Highway Robbers-->
	<xsl:template match="NPCCharacter[@id='steppe_bandits_bandit']/@name">
		<xsl:attribute name="name">Highway Thug</xsl:attribute>
	</xsl:template>
	<xsl:template match="NPCCharacter[@id='steppe_bandits_raider']/@name">
		<xsl:attribute name="name">Highwayman</xsl:attribute>
	</xsl:template>
	<xsl:template match="NPCCharacter[@id='steppe_bandits_chief']/@name">
		<xsl:attribute name="name">Highwayman Boss</xsl:attribute>
	</xsl:template>
	<!--Outlaws-->
	<xsl:template match="NPCCharacter[@id='forest_bandits_bandit']/@name">
		<xsl:attribute name="name">Fresh Outlaw</xsl:attribute>
	</xsl:template>
	<xsl:template match="NPCCharacter[@id='forest_bandits_raider']/@name">
		<xsl:attribute name="name">Outlaw</xsl:attribute>
	</xsl:template>
	<xsl:template match="NPCCharacter[@id='forest_bandits_chief']/@name">
		<xsl:attribute name="name">Outlaw Boss</xsl:attribute>
	</xsl:template>

</xsl:stylesheet>

