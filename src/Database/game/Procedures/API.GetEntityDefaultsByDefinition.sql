if object_id('API.GetEntityDefaultsByDefinition', 'P') is not null
	drop procedure API.GetEntityDefaultsByDefinition;
go

create procedure API.GetEntityDefaultsByDefinition
	@Definition int
as

select
	[definition] as [Definition],
	[definitionname] as [DefinitionName],
	[quantity] as [Quantity],
	[attributeflags] as [AttributeFlags],
	[categoryflags] as [CategoryFlags],
	[options] as [OptionsGenxy],
	[note] as [Note],
	[enabled] as [Enabled],
	[volume] as [Volume],
	[mass] as [Mass],
	[hidden] as [Hidden],
	[health] as [Health],
	[descriptiontoken] as [DescriptionToken],
	[purchasable] as [Purchasable],
	[tiertype] as [TierType],
	[tierlevel] as [TierLevel]
from entitydefaults
where [definition] = @Definition;
