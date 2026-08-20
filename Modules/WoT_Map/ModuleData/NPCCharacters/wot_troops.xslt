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

	<xsl:template match="NPCCharacter[(starts-with(@id, 'aserai') or starts-with(@id, 'battanian') or starts-with(@id, 'imperial') or starts-with(@id, 'khuzait') 
				  or starts-with(@id, 'sturgian') or starts-with(@id, 'vlandian') or starts-with(@id, 'varyag') or starts-with(@id, 'druzhinnik') 
				  or starts-with(@id, 'mamluke') or starts-with(@id, 'bucellari') or starts-with(@id, 'eastern') or starts-with(@id, 'western')  
				  or starts-with(@id, 'sword_sisters') or starts-with(@id, 'skolderbrotva') or starts-with(@id, 'eleftheroi') or starts-with(@id, 'forest_people')
				  or starts-with(@id, 'karakhuzaits') or starts-with(@id, 'jawwal') or starts-with(@id, 'embers') or starts-with(@id, 'brotherhood_of_woods')
				  or starts-with(@id, 'lakepike') or starts-with(@id, 'hidden_hand') or starts-with(@id, 'wolfskins') or starts-with(@id, 'beni_zilal')
				  or starts-with(@id, 'company_of') or starts-with(@id, 'legion_of') or starts-with(@id, 'ghilman') )]">
		<xsl:copy>
			<xsl:attribute name="is_hidden_encyclopedia">true</xsl:attribute>
				<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>
	
</xsl:stylesheet>
