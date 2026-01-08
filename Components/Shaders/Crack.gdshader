shader_type canvas_item;

uniform vec2 refraction_offset = vec2(25.0, 25.0);
uniform vec2 reflection_offset = vec2(1.0, 1.0);

uniform vec4 reflection_color : source_color = vec4(1.0, 1.0, 1.0, 0.8);

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_nearest;

void fragment() {
    vec4 crack = texture(TEXTURE, UV);

    // Treat alpha as crack strength
    float crack_mask = crack.a;

    // Sample nearby pixel to detect edges
    vec4 crack_adjacent = texture(
        TEXTURE,
        UV + TEXTURE_PIXEL_SIZE * reflection_offset
    );

    float edge_strength = abs(crack_adjacent.a - crack_mask);

    // Base screen refraction
    vec2 refract_uv = SCREEN_UV +
        crack_mask * SCREEN_PIXEL_SIZE * refraction_offset;

    vec4 refracted = texture(SCREEN_TEXTURE, refract_uv);

    // Reflection highlight on edges
    vec4 reflected = reflection_color;

    // Mix reflection only on edges
    vec4 final_color = mix(
        refracted,
        reflected,
        smoothstep(0.05, 0.2, edge_strength)
    );

    // Fade effect where there's no crack
    COLOR = mix(
        texture(SCREEN_TEXTURE, SCREEN_UV),
        final_color,
        crack_mask
    );
}