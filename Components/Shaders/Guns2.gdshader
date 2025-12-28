shader_type canvas_item;
render_mode unshaded;

uniform float pixel_size : hint_range(1.0, 16.0) = 1.0;
uniform bool flip_h = false;
uniform bool flip_v = false;

/* Weapons snap harder than characters */
uniform float rotation_steps = 24.0;

/* Interior sampling only */
uniform bool enable_sampling = true;

/* How aggressive sampling is (lower = stronger correction) */
uniform float sample_threshold : hint_range(0.005, 0.05) = 0.02;

varying vec2 block_size;
varying vec2 rot_x;
varying vec2 rot_y;

/* AXIS-ONLY neighbors */
const vec2 AXIS_OFFSETS[4] = vec2[](
	vec2( 0,  1),
	vec2( 0, -1),
	vec2( 1,  0),
	vec2(-1,  0)
);

/* 2x2 Bayer (rare transparency) */
float bayer2(vec2 p) {
	p = floor(p);
	return mod(p.x + p.y * 2.0, 4.0) / 4.0;
}

/* Outline detection (near-black, opaque) */
bool is_outline(vec4 c) {
	return c.a > 0.5 && dot(c.rgb, vec3(0.333)) < 0.15;
}

void vertex() {
	float theta = atan(MODEL_MATRIX[0][1], MODEL_MATRIX[0][0]);

	/* Strong angle locking */
	float step_angle = TAU / rotation_steps;
	theta = round(theta / step_angle) * step_angle;

	float c = cos(theta);
	float s = sin(theta);

	rot_x = vec2(c, s);
	rot_y = vec2(-s, c);

	block_size = vec2(
		pixel_size * TEXTURE_PIXEL_SIZE.x / length(MODEL_MATRIX[0].xy),
		pixel_size * TEXTURE_PIXEL_SIZE.y / length(MODEL_MATRIX[1].xy)
	);
}

void fragment() {
	mat2 rotation = mat2(rot_x, rot_y);
	mat2 inverse_rotation = transpose(rotation);

	/* --- Ultra-stable object-anchored grid --- */
	vec2 local_uv = UV - 0.5;
	vec2 rotated = rotation * local_uv;
	rotated -= fract(rotation * vec2(0.0) / block_size) * block_size;

	vec2 snapped = (floor(rotated / block_size) + 0.5) * block_size;
	vec2 snapped_uv = inverse_rotation * snapped + 0.5;

	/* --- Post-snap mirroring --- */
	vec2 flip = vec2(flip_h ? -1.0 : 1.0, flip_v ? -1.0 : 1.0);
	snapped_uv = (snapped_uv - 0.5) * flip + 0.5;

	vec4 color = texture(TEXTURE, snapped_uv);

	/* --- OUTLINE-LOCKED, CONSERVATIVE SAMPLING --- */
	bool outline = is_outline(color);

	if (enable_sampling && !outline) {
		vec4 best = color;
		float best_diff = 1e20;

		for (int i = 0; i < 4; i++) {
			vec4 n = texture(TEXTURE, snapped_uv + AXIS_OFFSETS[i] * block_size);

			/* Never pull outline into interior */
			if (is_outline(n)) continue;

			vec3 d = color.rgb - n.rgb;
			float diff = dot(d * d, vec3(0.299, 0.587, 0.114));

			if (diff < best_diff) {
				best = n;
				best_diff = diff;
			}
		}

		/* Only replace if the error is meaningful */
		if (best_diff > sample_threshold) {
			color = best;
		}
	}

	/* --- Tight edge clipping --- */
	float rot_extent = max(
		abs(rot_x.x) + abs(rot_x.y),
		abs(rot_y.x) + abs(rot_y.y)
	);

	float cut_x = rot_extent * block_size.x * 0.5;
	float cut_y = rot_extent * block_size.y * 0.5;

	float inside =
		step(cut_x, snapped_uv.x) *
		step(cut_y, snapped_uv.y) *
		step(snapped_uv.x, 1.0 - cut_x) *
		step(snapped_uv.y, 1.0 - cut_y);

	/* Weapons: strict alpha */
	float dither = bayer2(snapped_uv / block_size);
	color.a *= inside * step(max(0.75, dither), color.a);

	COLOR = color;
}
