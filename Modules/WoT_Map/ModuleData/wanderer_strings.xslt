<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output omit-xml-declaration="yes"/>
<xsl:template match="@*|node()">
	<xsl:copy>
		<xsl:apply-templates select="@*|node()"/>
	</xsl:copy>
</xsl:template>
	<xsl:template match="string[starts-with(@id,'prebackstory.spc_wanderer_empire')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_a.spc_wanderer_empire')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_b.spc_wanderer_empire')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_c.spc_wanderer_empire')]"/>
	<xsl:template match="string[starts-with(@id,'response_1.spc_wanderer_empire')]"/>
	<xsl:template match="string[starts-with(@id,'response_2.spc_wanderer_empire')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_d.spc_wanderer_empire')]"/>
	<xsl:template match="string[starts-with(@id,'generic_backstory.spc_wanderer_empire')]"/>
	
	<xsl:template match="string[starts-with(@id,'prebackstory.spc_wanderer_sturgia')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_a.spc_wanderer_sturgia')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_b.spc_wanderer_sturgia')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_c.spc_wanderer_sturgia')]"/>
	<xsl:template match="string[starts-with(@id,'response_1.spc_wanderer_sturgia')]"/>
	<xsl:template match="string[starts-with(@id,'response_2.spc_wanderer_sturgia')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_d.spc_wanderer_sturgia')]"/>
	<xsl:template match="string[starts-with(@id,'generic_backstory_d.spc_wanderer_sturgia')]"/>
	
	<xsl:template match="string[starts-with(@id,'prebackstory.spc_wanderer_battania')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_a.spc_wanderer_battania')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_b.spc_wanderer_battania')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_c.spc_wanderer_battania')]"/>
	<xsl:template match="string[starts-with(@id,'response_1.spc_wanderer_battania')]"/>
	<xsl:template match="string[starts-with(@id,'response_2.spc_wanderer_battania')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_d.spc_wanderer_battania')]"/>
	<xsl:template match="string[starts-with(@id,'generic_backstory_d.spc_wanderer_battania')]"/>
	
        <xsl:template match="string[starts-with(@id,'prebackstory.spc_wanderer_vlandia')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_a.spc_wanderer_vlandia')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_b.spc_wanderer_vlandia')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_c.spc_wanderer_vlandia')]"/>
	<xsl:template match="string[starts-with(@id,'response_1.spc_wanderer_vlandia')]"/>
	<xsl:template match="string[starts-with(@id,'response_2.spc_wanderer_vlandia')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_d.spc_wanderer_vlandia')]"/>
	<xsl:template match="string[starts-with(@id,'generic_backstory_d.spc_wanderer_vlandia')]"/>
	
	<xsl:template match="string[starts-with(@id,'prebackstory.spc_wanderer_aserai')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_a.spc_wanderer_aserai')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_b.spc_wanderer_aserai')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_c.spc_wanderer_aserai')]"/>
	<xsl:template match="string[starts-with(@id,'response_1.spc_wanderer_aserai')]"/>
	<xsl:template match="string[starts-with(@id,'response_2.spc_wanderer_aserai')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_d.spc_wanderer_aserai')]"/>
	<xsl:template match="string[starts-with(@id,'generic_backstory_d.spc_wanderer_aserai')]"/>
	
	<xsl:template match="string[starts-with(@id,'prebackstory.spc_wanderer_khuzait')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_a.spc_wanderer_khuzait')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_b.spc_wanderer_khuzait')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_c.spc_wanderer_khuzait')]"/>
	<xsl:template match="string[starts-with(@id,'response_1.spc_wanderer_khuzait')]"/>
	<xsl:template match="string[starts-with(@id,'response_2.spc_wanderer_khuzait')]"/>
	<xsl:template match="string[starts-with(@id,'backstory_d.spc_wanderer_khuzait')]"/>
	<xsl:template match="string[starts-with(@id,'generic_backstory_d.spc_wanderer_khuzait')]"/>
		
</xsl:stylesheet>

