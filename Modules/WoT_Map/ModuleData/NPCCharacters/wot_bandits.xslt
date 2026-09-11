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

	<xsl:template match="NPCCharacters[@id='caravan_master_aserai']"/>
	<xsl:template match="NPCCharacters[@id='caravan_master_battania']"/>
	<xsl:template match="NPCCharacters[@id='caravan_master_empire']"/>
	<xsl:template match="NPCCharacters[@id='caravan_master_khuzait']"/>
	<xsl:template match="NPCCharacters[@id='caravan_master_sturgia']"/>
	<xsl:template match="NPCCharacters[@id='caravan_master_vlandia']"/>
	<xsl:template match="NPCCharacters[@id='armed_trader_aserai']"/>
	<xsl:template match="NPCCharacters[@id='armed_trader_battania']"/>
	<xsl:template match="NPCCharacters[@id='armed_trader_empire']"/>
	<xsl:template match="NPCCharacters[@id='armed_trader_khuzait']"/>
	<xsl:template match="NPCCharacters[@id='armed_trader_sturgia']"/>
	<xsl:template match="NPCCharacters[@id='armed_trader_vlandia']"/>
	<xsl:template match="NPCCharacters[@id='caravan_guard_aserai']"/>
	<xsl:template match="NPCCharacters[@id='caravan_guard_battania']"/>
	<xsl:template match="NPCCharacters[@id='caravan_guard_empire']"/>
	<xsl:template match="NPCCharacters[@id='caravan_guard_khuzait']"/>
	<xsl:template match="NPCCharacters[@id='caravan_guard_sturgia']"/>
	<xsl:template match="NPCCharacters[@id='caravan_guard_vlandia']"/>
	<xsl:template match="NPCCharacters[@id='veteran_caravan_guard_aserai']"/>
	<xsl:template match="NPCCharacters[@id='veteran_caravan_guard_battania']"/>
	<xsl:template match="NPCCharacters[@id='veteran_caravan_guard_empire']"/>
	<xsl:template match="NPCCharacters[@id='veteran_caravan_guard_khuzait']"/>
	<xsl:template match="NPCCharacters[@id='veteran_caravan_guard_sturgia']"/>
	<xsl:template match="NPCCharacters[@id='veteran_caravan_guard_vlandia']"/>	

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

