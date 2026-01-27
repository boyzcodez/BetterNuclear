shader_type canvas_item;

uniform vec2 virtual_resolution = vec2(480.0, 270.0);

void fragment() {
    vec2 vps = 1.0 / virtual_resolution;
    vec2 snapped_screen_uv = (floor(SCREEN_UV / vps) + vec2(0.5)) * vps;

    vec2 delta_viewport_pixels = (snapped_screen_uv - SCREEN_UV) / SCREEN_PIXEL_SIZE;

    vec2 uv_shift =
        delta_viewport_pixels.x * dFdx(UV) +
        delta_viewport_pixels.y * dFdy(UV);

    vec2 uvs = clamp(UV + uv_shift, vec2(0.0), vec2(1.0));

    // Force LOD 0 (no mip darkening)
    vec4 tex = textureLod(TEXTURE, uvs, 0.0);

    COLOR = tex * COLOR;
}
