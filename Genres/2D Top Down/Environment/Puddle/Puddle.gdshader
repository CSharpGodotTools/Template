shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;

// The noise of the water waves
uniform sampler2D noise : repeat_enable;

uniform float voronoi_blend : hint_range(0, 0.05);

// Puddle Mask
uniform sampler2D mask : filter_linear;

// wave_speed of waves
uniform float wave_speed : hint_range(0.05, 0.2);

// Noise strength
uniform float strength : hint_range(0, 0.1);

void fragment()
{
	float noise_value = texture(noise, UV + TIME * wave_speed).r;
	vec4 screen_tex = texture(screen_texture, SCREEN_UV + vec2(noise_value) * strength);

	COLOR = screen_tex;
	COLOR.a *= texture(mask, UV).a;
}
