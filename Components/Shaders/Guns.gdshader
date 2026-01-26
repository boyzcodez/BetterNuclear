shader_type canvas_item;

uniform float pixel_size : hint_range(1.0, 16.0) = 1.0;
uniform int mix_sampling : hint_range(-1, 2) = 0; // -1 off, 0 off, 1 cross, 2 cross+diagonals
uniform bool flip_h = false;
uniform bool flip_v = false;

varying float theta;
varying vec2 block_size;

uniform bool harden_opaque_alpha = true;
uniform float alpha_cutoff : hint_range(0.0, 1.0) = 0.5;
uniform float harden_start : hint_range(0.0, 1.0) = 0.85; // start hardening near-opaque pixels

// --- OUTLINE ---
uniform bool outline_enabled = true;
uniform vec4 outline_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);

// Neighbors count as "weapon" only if alpha >= this (glow won't generate outline)
uniform float outline_source_alpha_cutoff : hint_range(0.0, 1.0) = 0.5;

// Outline can be drawn on pixels with alpha < this (so it can draw ON glow too, but not on the weapon)
uniform float outline_target_alpha_cutoff : hint_range(0.0, 1.0) = 0.5;

// Consider neighbor "solid" only if its alpha is above this (prevents glow from generating outline)
uniform float outline_solid_alpha_cutoff : hint_range(0.0, 1.0) = 0.5;

// Neighbor offsets in "pixel blocks" (up, down, right, left, diagonals...)
const vec2 OFFS[8] = vec2[](
	vec2( 0.0,  1.0), // 0 up
	vec2( 0.0, -1.0), // 1 down
	vec2( 1.0,  0.0), // 2 right
	vec2(-1.0,  0.0), // 3 left
	vec2( 1.0,  1.0), // 4 up-right
	vec2( 1.0, -1.0), // 5 down-right
	vec2(-1.0,  1.0), // 6 up-left
	vec2(-1.0, -1.0)  // 7 down-left
);

float hardened_alpha(float a) {
	if (!harden_opaque_alpha) return a;

	float t = smoothstep(harden_start, 1.0, a);
	float hard = step(alpha_cutoff, a);
	return mix(a, hard, t);
}

vec2 snapped_neighbor_uv(vec2 snapped_r, mat2 inv_rot, vec2 cell, vec2 margin, vec2 offs) {
	vec2 nr = snapped_r + offs * cell;
	vec2 nuv = inv_rot * nr + vec2(0.5);

	// Mirror AFTER snapping (in texture space)
	if (flip_h) nuv.x = 1.0 - nuv.x;
	if (flip_v) nuv.y = 1.0 - nuv.y;

	return clamp(nuv, margin, vec2(1.0) - margin);
}

void vertex() {
	theta = atan(MODEL_MATRIX[0][1], MODEL_MATRIX[0][0]);

	float sx = length(MODEL_MATRIX[0].xy);
	float sy = length(MODEL_MATRIX[1].xy);

	block_size = vec2(
		pixel_size * TEXTURE_PIXEL_SIZE.x / max(sx, 1e-6),
		pixel_size * TEXTURE_PIXEL_SIZE.y / max(sy, 1e-6)
	);
}

void fragment() {
	float c = cos(theta);
	float s = sin(theta);

	mat2 rot     = mat2(vec2(c, s), vec2(-s, c));
	mat2 inv_rot = mat2(vec2(c,-s), vec2( s, c));

	vec2 uv_c = UV - vec2(0.5);
	vec2 uv_r = rot * uv_c;

	vec2 cell = block_size;
	vec2 snapped_r  = (floor(uv_r / cell) + vec2(0.5)) * cell;
	vec2 snapped_uv = inv_rot * snapped_r + vec2(0.5);

	if (flip_h) snapped_uv.x = 1.0 - snapped_uv.x;
	if (flip_v) snapped_uv.y = 1.0 - snapped_uv.y;

	vec2 half_cell = 0.5 * cell;

	vec2 margin;
	margin.x = abs(inv_rot[0][0]) * half_cell.x + abs(inv_rot[0][1]) * half_cell.y;
	margin.y = abs(inv_rot[1][0]) * half_cell.x + abs(inv_rot[1][1]) * half_cell.y;

	bool outside = (
		snapped_uv.x < margin.x || snapped_uv.x > 1.0 - margin.x ||
		snapped_uv.y < margin.y || snapped_uv.y > 1.0 - margin.y
	);

	vec2 sample_uv = clamp(snapped_uv, margin, vec2(1.0) - margin);

	vec4 base = texture(TEXTURE, sample_uv);

	// Mix sampling (your original behavior)
	if (mix_sampling > 0) {
		int count = 4 * mix_sampling; // 4 (cross) or 8 (cross + diagonals)

		vec4 best = base;
		float best_d = 1e20;

		for (int i = 0; i < 8; i++) {
			if (i >= count) break;

			vec2 nuv = sample_uv + OFFS[i] * cell;
			vec4 ncol = texture(TEXTURE, nuv);
			float d = length(base - ncol);

			if (d < best_d) {
				best_d = d;
				best = ncol;
			}
		}
		base = best;
	}

	// Alpha rules
	if (outside) {
		base.a = 0.0;
	} else {
		base.a = hardened_alpha(base.a);
	}

	// Decide final color without early return
	vec4 out_col = base;

	// Outline: only on (nearly) fully transparent pixels, sourced only from solid neighbors, cross only
	if (outline_enabled && !outside && base.a < outline_target_alpha_cutoff) {
    float solid = 0.0;

    for (int i = 0; i < 4; i++) { // cross only -> no corner pixels
        vec2 nuv = snapped_neighbor_uv(snapped_r, inv_rot, cell, margin, OFFS[i]);
        vec4 ncol = texture(TEXTURE, nuv);
        float na = hardened_alpha(ncol.a);

        // weapon source only (ignores glow as a source)
        solid = max(solid, step(outline_source_alpha_cutoff, na));
    }

    if (solid > 0.0) {
        out_col = outline_color;
    }
}

	COLOR = out_col;
}
