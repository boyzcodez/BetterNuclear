shader_type canvas_item;

uniform float strength : hint_range(0.0, 1.0) = 0.35;
uniform float softness : hint_range(0.1, 2.0) = 1.1;

void fragment() {
    vec2 uv = SCREEN_UV;
    vec2 center = vec2(0.5);
    
    float dist = distance(uv, center);
    float vignette = smoothstep(0.4, softness, dist);

    COLOR = vec4(0.0, 0.0, 0.0, vignette * strength);
}
