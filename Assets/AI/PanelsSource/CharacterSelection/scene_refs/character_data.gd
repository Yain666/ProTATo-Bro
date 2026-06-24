class_name CharacterData
extends ItemData

@export var wanted_tags # (Array, String)
@export var banned_item_groups # (Array, String)
@export var banned_items # (Array, String)
@export var starting_weapons # (Array, Resource)
@export var starting_items # (Array, Resource)


func get_category() -> int:
	return Category.CHARACTER
