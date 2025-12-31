shader_type canvas_item;

uniform float pixel_size : hint_range(1.0, 16.0) = 1.0;
uniform int mix_sampling : hint_range(-1, 2) = 0; // -1 off, 0 off, 1 cross, 2 cross+diagonals
uniform bool flip_h = false;
uniform bool flip_v = false;

varying float theta;
varying vec2 block_size;

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

void vertex() {
	// atan(y, x) == atan2(y, x)
	theta = atan(MODEL_MATRIX[0][1], MODEL_MATRIX[0][0]);

	// Approximate per-axis scale from MODEL_MATRIX (works well for rotate+scale).
	float sx = length(MODEL_MATRIX[0].xy);
	float sy = length(MODEL_MATRIX[1].xy);

	block_size = vec2(
		pixel_size * TEXTURE_PIXEL_SIZE.x / max(sx, 1e-6),
		pixel_size * TEXTURE_PIXEL_SIZE.y / max(sy, 1e-6)
	);
}

void fragment() {
	// Compute trig once
	float c = cos(theta);
	float s = sin(theta);

	// Rotation matrix and its inverse (transpose for pure rotations)
	mat2 rot     = mat2(vec2(c, s), vec2(-s, c));
	mat2 inv_rot = mat2(vec2(c,-s), vec2( s, c));

	// Snap UV to rotated pixel grid (centered at 0.5)
	vec2 uv_c = UV - vec2(0.5);
	vec2 uv_r = rot * uv_c;

	vec2 cell = block_size;
	vec2 snapped_r = (floor(uv_r / cell) + vec2(0.5)) * cell;
	vec2 snapped_uv = inv_rot * snapped_r + vec2(0.5);

	// Mirror AFTER snapping (in texture space)
	if (flip_h) snapped_uv.x = 1.0 - snapped_uv.x;
	if (flip_v) snapped_uv.y = 1.0 - snapped_uv.y;

	// --- Edge cutoff margin (project rotated half-cell extents back into UV axes) ---
	// Half a cell in "rotated space"
	vec2 half_cell = 0.5 * cell;

	// Project half-cell extents onto UV axes.
	// margin.x = |inv_rot row0| dot half_cell, margin.y = |inv_rot row1| dot half_cell
	vec2 margin;
	margin.x = abs(inv_rot[0][0]) * half_cell.x + abs(inv_rot[0][1]) * half_cell.y;
	margin.y = abs(inv_rot[1][0]) * half_cell.x + abs(inv_rot[1][1]) * half_cell.y;

	bool outside = (
		snapped_uv.x < margin.x || snapped_uv.x > 1.0 - margin.x ||
		snapped_uv.y < margin.y || snapped_uv.y > 1.0 - margin.y
	);

	// Clamp sample UV to avoid wrap/repeat bleed at edges (optional but usually good)
	vec2 sample_uv = clamp(snapped_uv, margin, vec2(1.0) - margin);

	vec4 color = texture(TEXTURE, sample_uv);

	// Mix sampling (nearest “most similar” neighbor)
	if (mix_sampling > 0) {
		int count = 4 * mix_sampling; // 4 (cross) or 8 (cross + diagonals)

		vec4 best = color;
		float best_d = 1e20;

		for (int i = 0; i < 8; i++) {
			if (i >= count) break;

			vec2 nuv = sample_uv + OFFS[i] * cell;
			vec4 ncol = texture(TEXTURE, nuv);
			float d = length(color - ncol);

			if (d < best_d) {
				best_d = d;
				best = ncol;
			}
		}
		color = best;
	}

	// Your alpha rules, but applied after outside test
	if (outside) {
		color.a = 0.0;
	} else {
		color.a = step(0.5, color.a);
	}

	COLOR = color;
}
