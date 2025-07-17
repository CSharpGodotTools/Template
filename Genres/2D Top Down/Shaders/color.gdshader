shader_type canvas_item;

uniform vec4 color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float blend_intensity : hint_range(0.0, 1.0) = 0.0f;

void fragment()
{
    vec4 original_color = texture(TEXTURE, UV);
    vec4 neutral_color = vec4(1.0, 1.0, 1.0, original_color.a); // Neutral color (white) with the same alpha as the original

    // Blend the original color with the neutral color based on the blend intensity
    COLOR = mix(original_color, neutral_color, blend_intensity) * color;
}
