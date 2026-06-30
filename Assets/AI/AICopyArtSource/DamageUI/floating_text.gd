class_name FloatingText
extends Label

signal available

@onready var _icon: Sprite2D = $"%Icon"
@onready var _tween: Tween = $"%Tween"
var has_theme_icon: = false
var player_index: = - 1


func display(content: String, direction: Vector2, duration: float, spread: float, color: Color = Color.WHITE, all_caps: bool = false) -> void :
	show()
	self_modulate = color
	text = content
	uppercase = all_caps
	var movement: = direction.rotated(randf_range( - spread / 2, spread / 2))
	pivot_offset = size / 2
	scale = Vector2.ONE
	modulate.a = 1.0

	var _success = _tween.interpolate_property(
		self, 
		"position", 
		position, 
		position + movement, 
		duration, 
		Tween.TRANS_ELASTIC, 
		Tween.EASE_OUT
	)
	_success = _tween.start()
	await _tween.tween_all_completed

	_success = _tween.interpolate_property(
		self, 
		"scale", 
		scale, 
		Vector2.ZERO, 
		duration, 
		Tween.TRANS_ELASTIC, 
		Tween.EASE_IN_OUT
	)

	_success = _tween.interpolate_property(
		self, 
		"modulate:a", 
		modulate.a, 
		0.0, 
		duration, 
		Tween.TRANS_LINEAR, 
		Tween.EASE_IN_OUT
	)
	_success = _tween.start()
	await _tween.tween_all_completed

	hide()
	_icon.hide()
	emit_signal("available", self)


func set_icon(icon: Texture2D, icon_scale: Vector2) -> void :
	_icon.show()
	_icon.texture = icon
	_icon.scale = icon_scale
	_icon.position.x = get_minimum_size().x + 8
	has_theme_icon = true
