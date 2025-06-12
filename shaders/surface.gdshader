shader_type canvas_item;

uniform sampler2D data_texture; // Hidden texture holding grid datauniform sampler2D data_texture; // The texture you're comparing to
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture;
uniform sampler2D subviewport_texture;
uniform float point_dist = 25.;

/*uniform vec2 effect_origin = vec2(0.0, 0.0); // Set effect start position
 // Control the area of the effect

*/

uniform float xOffset = 0.0;
uniform float yOffset = 0.0;
uniform int gpX = 100;
uniform int gpY = 100;
uniform vec3[10000] yDiffs;
uniform vec2 screen_size;
void fragment()
{
	vec2 frag_pos = FRAGCOORD.xy;// * screen_size; // Get pixel position on screen
	COLOR = texture(subviewport_texture, SCREEN_UV);
	COLOR.b += 0.2;

	vec3 nearest_pt = vec3(round(frag_pos.x / point_dist), round(frag_pos.y / point_dist), 0); // initially packed with indexes for yDiffs lookup

	vec3 a = yDiffs[(int(nearest_pt.x) * gpY) + int(nearest_pt.y)]; 			//center
	vec3 b = yDiffs[(int(nearest_pt.x+1.) * gpY) + int(nearest_pt.y)];		//right
	vec3 c = yDiffs[(int(nearest_pt.x-1.) * gpY) + int(nearest_pt.y)];		//left
	vec3 d = yDiffs[(int(nearest_pt.x) * gpY) + int(nearest_pt.y+1.)];		//bottom
	vec3 e = yDiffs[(int(nearest_pt.x) * gpY) + int(nearest_pt.y-1.)];		//top
	float max_value = max(max(a.z, b.z), max(c.z, max(d.z, e.z)));
	if (max_value == a.z) {
		nearest_pt = a;
	} else if (max_value == b.z) {
		nearest_pt = b;
	} else if (max_value == c.z) {
		nearest_pt = c;
	} else if (max_value == d.z) {
		nearest_pt = d;
	} else if (max_value == e.z) {
		nearest_pt = e;
	}

	//vec3 yDiff = yDiffs[(int(nearest_pt.x) * gpY) + int(nearest_pt.y)];
	//float xDist = mod(nearest_pt.x, 1) * point_dist;
	//float yDist = mod(nearest_pt.y, 1) * point_dist;
	float effect_radius = nearest_pt.z/10.; //(a.z + b.z + c.z + d.z + e.z)/5.;//
	float dist = length(vec2(nearest_pt.x + xOffset, nearest_pt.y + yOffset) - frag_pos) / 10000.;
	if (dist < effect_radius && dist > effect_radius - 10.) {
		if (max_value == a.z) {
			//COLOR.b += (abs(nearest_pt.z)/dist)/10.;
		} else {
			//COLOR.b += (abs(nearest_pt.z)/dist)/20.;
		}

	}


	/*vec2 uv = UV;
	float aspect_ratio = (screen_size.x / screen_size.y);
		//uv.x*= aspect_ratio;
    // Screen center
    vec2 center =vec2(0.5, 0.5);//  nearest_pt.xy / screen_size;

    // Calculate the distance from the center
    dist = distance(uv, center); //center); //
	if (dist < .2 ) { // && dist > 0.1
    float ripple_strength = 0.02;   // Controls how much the ripple distorts
    float ripple_frequency = 20.0;  // Controls the number of ripples
    float ripple = sin(dist * ripple_frequency) * ripple_strength; // Calculate the distortion using a sine wave based on distance
	vec2 centered_uv = (uv - center);
	//centered_uv.x *= aspect_ratio;
    vec2 distorted_uv = uv + normalize(centered_uv) * ripple;	// Apply the ripple distortion (outward from the center)

    // Sample the texture at the distorted coordinates
    vec4 color = texture(TEXTURE, distorted_uv ); //

    // Output the color
    COLOR = color;

	}*/
	/*float frequency = 5.;
	float time = 3.;
	float speed = 1.;
	float amplitude = 0.02;
vec2 center = vec2(0.5); // Normalized center of the texture
float ring_width = 0.5;
    dist = distance(UV, center);

    // Calculate the expanding ring position based on time and speed
    float ring_position = time * speed;

    // Create a band around the expanding ring position
    float ring_effect = exp(-pow((dist - ring_position) / ring_width, 2.0)); // Gaussian-like falloff

    // Apply the ripple distortion only within the ring
    float ripple = sin((dist * frequency) - (time * speed)) * amplitude * ring_effect;

    // Distort UV coordinates along the direction from the center
    vec2 ripple_uv = UV + normalize(UV - center) * ripple;

    // Sample the texture
    COLOR = texture(TEXTURE, ripple_uv);*/






















	/*float x = (frag_pos.x - xOffset) / point_dist;
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
	}*/
}