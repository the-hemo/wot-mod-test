<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output omit-xml-declaration="yes"/>
<xsl:template match="@*|node()">
    <xsl:copy>
        <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
</xsl:template>
 
    <xsl:template match="string[@id='str_culture_description.vlandia']"/>
    <xsl:template match="string[@id='str_culture_description.battania']"/>
    <xsl:template match="string[@id='str_culture_description.khuzait']"/>
    <xsl:template match="string[@id='str_culture_description.aserai']"/>
    <xsl:template match="string[@id='str_culture_description.sturgia']"/>
    <xsl:template match="string[@id='str_culture_description.empire']"/>

    <xsl:template match="string[@id='str_culture_rich_name.vlandia']"/>
    <xsl:template match="string[@id='str_culture_rich_name.battania']"/>
    <xsl:template match="string[@id='str_culture_rich_name.khuzait']"/>
    <xsl:template match="string[@id='str_culture_rich_name.aserai']"/>
    <xsl:template match="string[@id='str_culture_rich_name.sturgia']"/>
    <xsl:template match="string[@id='str_culture_rich_name.empire']"/>

    <xsl:template match="string[@id='str_political_philosophy_lord_1_15_for_lord_1_14']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_15_for_lord_1_14_b']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_15_for_lord_1_14_c']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_14_for_lord_1_14']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_14_for_lord_1_14_b']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_14_for_lord_1_14_c']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_17_for_lord_1_14']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_37_for_lord_1_14']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_1_for_lord_1_1']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_1_for_lord_1_1_b']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_1_for_lord_1_1_c']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_5_for_lord_1_1']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_5_for_lord_1_1_b']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_5_for_lord_1_1_c']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_20_for_lord_1_1']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_7_for_lord_1_7']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_7_for_lord_1_7_b']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_7_for_lord_1_7_c']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_11_for_lord_1_7']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_21_for_lord_1_7']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_46_for_lord_1_7']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_46_for_lord_1_7_b']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_1_46_for_lord_1_7_c']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_2_1_for_lord_2_1']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_2_1_for_lord_2_1_b']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_2_1_for_lord_2_1_c']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_2_3_for_lord_2_1']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_2_5_for_lord_2_1']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_5_1_for_lord_5_1']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_5_1_for_lord_5_1_b']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_5_1_for_lord_5_1_c']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord 5_3_for_lord_5_1']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord 5_5_for_lord_5_1']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_3_1_for_lord_3_1']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_3_1_for_lord_3_1_b']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_3_1_for_lord_3_1_c']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_3_3_for_lord_3_1']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_3_3_for_lord_3_1_b']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_3_3_for_lord_3_1_c']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_3_5_for_lord_3_1']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_4_1_for_lord_4_1']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_4_1_for_lord_4_1_b']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_4_1_for_lord_4_1_c']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_6_1_for_lord_6_1']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_6_1_for_lord_6_1_b']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_6_1_for_lord_6_1_c']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_6_4_for_lord_6_1']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_6_4_for_lord_6_1_b']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_6_4_for_lord_6_1_c']"/>
    <xsl:template match="string[@id='str_political_philosophy_lord_6_5_for_lord_6_1']"/>
</xsl:stylesheet>


