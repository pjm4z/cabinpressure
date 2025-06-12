shader_type canvas_item;

uniform sampler2D data_texture; // Hidden texture holding grid datauniform sampler2D data_texture; // The texture you're comparing to
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture;
uniform sampler2D subviewport_texture;
uniform vec2 effect_origin = vec2(0.0, 0.0); // Set effect start position
 // Control the area of the effect

uniform float point_dist = 25.;
uniform int gpX = 100;
uniform int gpY = 100;
uniform float xOffset = 0.0;
uniform float yOffset = 0.0;
uniform vec3[10000] yDiffs;

void fragment()
{
	vec2 frag_pos = FRAGCOORD.xy; // Get pixel position on screen
	COLOR = texture(subviewport_texture, SCREEN_UV);
	COLOR.b += 0.2;
	float x = (frag_pos.x - xOffset) / point_dist;
	if (mod(x, 1) > 0.5) {
		x = (frag_pos.x) / point_dist;
	}
	float xDist = mod(x, 1) * point_dist;
	int xInd = int(x);
	float y = (frag_pos.y - yOffset) / point_dist;
	if (mod(y, 1) > 0.5) {
		y = (frag_pos.y) / point_dist;
	}
	float yDist = mod(y, 1) * point_dist;
	int yInd = int(y);
	float effect_radius = yDiffs[(xInd * gpY) + yInd].z;
	float dist = sqrt((xDist * xDist) + (yDist * yDist));

	vec4 pixel_pos = texture(subviewport_texture, SCREEN_UV - vec2(0.5));// - vec2(0.5); // Normalize pixel position to center

    dist = length(frag_pos - pixel_pos.xy);
	effect_radius = 1.0;
	if (dist > effect_radius && dist < effect_radius + 10.0) {
	 	COLOR.r += yDiffs[(xInd * gpY) + yInd].z/dist;
	}
}