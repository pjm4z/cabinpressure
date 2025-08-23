shader_type canvas_item;

uniform sampler2D main_tex: source_color;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	ivec2 tex_dimensions = textureSize(main_tex, 0);
    vec2 tex_size = vec2(tex_dimensions);

	vec2 box_size = clamp(fwidth(UV) * tex_size, 1e-5, 1);

	vec2 tx = UV * tex_size - 0.5 * box_size;

	vec2 tx_offset = smoothstep(vec2(1.0f - box_size.x, 1.0f - box_size.y), vec2(1,1), (tx - floor(tx)));

	vec2 uv = (floor(tx) + 0.5 + tx_offset) * TEXTURE_PIXEL_SIZE;

	vec4 color = textureGrad(main_tex, uv, dFdx(UV), dFdy(UV));

	COLOR = color;
	// Called for every pixel the material is visible on.
}
/*
void fragment() {
    // 1. Compute texture size in texels (inverse of texel size)
    vec2 tex_size = vec2(textureSize(main_tex, 0)); // _MainTex_TexelSize.zw == 1 / tex_size

    // 2. fwidth in UV space → screen-space pixel size in UVs
    vec2 box_size = clamp(fwidth(UV) * tex_size, 1e-5, 1.0);

    // 3. Convert UVs to texel space and offset for sampling
    vec2 tx = UV * tex_size - 0.5 * box_size;

    // 4. Fractional offset with smoothstep to soften edges
    vec2 tx_offset = smoothstep(vec2(1.0) - box_size, vec2(1.0), fract(tx));

    // 5. Final UV in normalized coordinates
    vec2 uv = (floor(tx) + 0.5 + tx_offset) / tex_size;

    // 6. Sample texture using explicit derivatives
    vec4 color = textureGrad(main_tex, uv, dFdx(UV), dFdy(UV));

    COLOR = color;
}
*/
//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
