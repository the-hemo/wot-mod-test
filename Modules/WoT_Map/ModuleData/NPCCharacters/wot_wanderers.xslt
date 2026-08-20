<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output omit-xml-declaration="yes"/>
<xsl:template match="@*|node()">
    <xsl:copy>
        <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
</xsl:template>
 <!--Removing Vanilla wanderers so the Equipment Sets arent corrupted-->

	<xsl:template match="NPCCharacter[(starts-with(@id, 'spc_wanderer'))]"/>
				  
</xsl:stylesheet>

