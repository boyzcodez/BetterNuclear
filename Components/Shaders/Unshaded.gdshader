shader_type canvas_item;
render_mode unshaded;

void fragment() {
    vec2 uv = UV;
    // snap UV to nearest texel center
    vec2 texel = TEXTURE_PIXEL_SIZE;
    uv = (floor(uv / texel) + 0.5) * texel;
    COLOR = texture(TEXTURE, uv);
}