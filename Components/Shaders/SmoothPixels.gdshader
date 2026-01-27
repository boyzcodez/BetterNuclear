shader_type canvas_item;
render_mode world_vertex_coords, blend_mix;

uniform float sharpness : hint_range(0.5, 3.0) = 1.6; // higher = crisper, too high = shimmer
uniform float min_softness : hint_range(0.5, 2.0) = 1.0; // higher = more stable when minifying/rotating

vec4 sampleTextureCrisp(sampler2D tex, vec2 uv, vec2 texel_size)
{
	// Convert UV to "texel space" (pixels in the source texture).
	vec2 tex_size = vec2(1.0) / texel_size;
	vec2 p = uv * tex_size;

	// Nearest texel center in texel space.
	vec2 c = floor(p - 0.5) + 0.5;

	// Offset from center in [-0.5, 0.5).
	vec2 o = p - c;

	// Footprint of a screen pixel in texels (bigger => more minification).
	vec2 fw = fwidth(p);
	fw = max(fw, vec2(1e-5));

	// Make the transition sharper near 1:1, but still widen when minifying.
	// sharpness > 1 narrows blending region (crisper).
	vec2 t = clamp((o * sharpness) / (fw * min_softness), -0.5, 0.5);

	// Back to UV space.
	vec2 uv2 = (c + t) / tex_size;

	// Keep correct derivatives for mip/LOD decisions.
	return textureGrad(tex, uv2, dFdx(uv), dFdy(uv));
}

varying flat vec4 modulate;

void vertex() {
	modulate = COLOR;
}

void fragment()
{
	vec4 col = sampleTextureCrisp(TEXTURE, UV, TEXTURE_PIXEL_SIZE);
	COLOR = vec4(col.rgb * modulate.rgb, col.a * modulate.a);
}
