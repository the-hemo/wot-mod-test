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

	<xsl:template match="Faction[@id='player_faction']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_ludgard</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='looters']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.hideout_mountain_2</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='deserters']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.hideout_mountain_2</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='sea_raiders']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.hideout_seaside_6</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='mountain_bandits']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.hideout_mountain_15</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='forest_bandits']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.hideout_forest_19</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='desert_bandits']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.hideout_desert_12</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='steppe_bandits']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.hideout_steppe_5</xsl:attribute>
	</xsl:template>

	<xsl:template match="Faction[@id='ghilman']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_aiel_7</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='legion_of_the_betrayed']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_village_illian_3_2</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='skolderbrotva']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.village_canluum_3</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='company_of_the_boar']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.village_darluna_3</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='beni_zilal']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.village_aiel_2_3</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='wolfskins']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.village_minde_3</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='brotherhood_of_woods']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_village_murandy_2_1</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='hidden_hand']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_shadow_11</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='lakepike']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.village_jumaras_2</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='embers_of_flame']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.village_gholams_2</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='jawwal']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_aiel_5</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='karakhuzaits']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_village_malkier_1_1</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='forest_people']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_village_farmad_2_1</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='eleftheroi']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_village_illian_1_1</xsl:attribute>
	</xsl:template>

	<xsl:template match="Faction[@id='clan_empire_north_1']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_caemlyn</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_north_2']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_braem</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_north_3']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_aringill</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_north_4']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_andor_1</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_north_5']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_andor_2</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_north_6']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_andor_3</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_north_7']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_andor_4</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_north_8']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_tarvalon</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_north_9']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_sedai_1</xsl:attribute>
	</xsl:template>

	<xsl:template match="Faction[@id='clan_empire_west_1']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_whitebridge</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_west_2']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_baerlon</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_west_3']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_emonds</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_west_4']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_dragon_1</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_west_5']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_dragon_2</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_west_6']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_dragon_3</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_west_7']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_dragon_4</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_west_8']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_ashaman</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_west_9']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_sedai_2</xsl:attribute>
	</xsl:template>

	<xsl:template match="Faction[@id='clan_empire_south_1']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_cairhien</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_south_2']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_tremon</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_south_3']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_cairhien_1</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_south_4']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_cairhien_2</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_south_5']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_cairhien_3</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_south_6']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_mayene</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_south_7']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_mayene_1</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_south_8']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_mayene_2</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_empire_south_9']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_godan</xsl:attribute>
	</xsl:template>

	<xsl:template match="Faction[@id='clan_aserai_1']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_aiel_1</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_aserai_2']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_aiel_2</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_aserai_3']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_aiel_3</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_aserai_4']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_aiel_4</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_aserai_5']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_aiel_5</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_aserai_6']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_aiel_6</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_aserai_7']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_aiel_1</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_aserai_8']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_aiel_2</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_aserai_9']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_aiel_3</xsl:attribute>
	</xsl:template>

	<xsl:template match="Faction[@id='clan_khuzait_1']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_maradon</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_khuzait_2']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_bashere</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_khuzait_3']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_chachin</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_khuzait_4']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_canluum</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_khuzait_5']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_arbela</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_khuzait_6']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_jakanda</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_khuzait_7']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_falmoran</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_khuzait_8']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_faldara</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_khuzait_9']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_seven</xsl:attribute>
	</xsl:template>

	<xsl:template match="Faction[@id='clan_sturgia_1']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_shayol</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_sturgia_2']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_sherandu</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_sturgia_3']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_fortress</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_sturgia_4']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_thetown</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_sturgia_5']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_thakandar</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_sturgia_6']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_doom</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_sturgia_7']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_jumaras</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_sturgia_8']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_gholams</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_sturgia_9']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_draghkar</xsl:attribute>
	</xsl:template>

	<xsl:template match="Faction[@id='clan_vlandia_1']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_falme</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_vlandia_2']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_darluna</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_vlandia_3']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_bandar</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_vlandia_4']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_katar</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_vlandia_5']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_tanchico</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_vlandia_6']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_elmora</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_vlandia_7']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_amador</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_vlandia_8']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_seachan_1</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_vlandia_9']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_seachan_2</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_vlandia_10']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_seachan_3</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_vlandia_11']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_doman_2</xsl:attribute>
	</xsl:template>

	<xsl:template match="Faction[@id='clan_battania_1']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_farmad</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_battania_2']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_illian</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_battania_3']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_eboudar</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_battania_4']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.town_salidar</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_battania_5']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_farmad_1</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_battania_6']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_farmad_2</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_battania_7']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_illian_1</xsl:attribute>
	</xsl:template>
	<xsl:template match="Faction[@id='clan_battania_8']/@initial_home_settlement">
		<xsl:attribute name="{name()}">Settlement.castle_illian_2</xsl:attribute>
	</xsl:template>
	
</xsl:stylesheet>
