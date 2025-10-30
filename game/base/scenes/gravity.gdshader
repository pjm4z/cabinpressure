shader_type canvas_item;
render_mode world_vertex_coords;

uniform vec3 bodies[100]; 
uniform int elements = 0;
uniform float scale = 1;
uniform float speed = 0;
uniform float zoom = 1;

const float EarthMass = 5.972 * pow(10, 7); 
varying vec2 world_pos;
void vertex() {
	world_pos = VERTEX;
	// Called for every vertex the material is visible on.
}

vec2 getHeading(vec2 offset) {
	vec2 heading = vec2(0,0);
	vec2 pos = world_pos;
	pos += offset;
	for (int i = 0; i < elements; i++) {
		vec3 body = bodies[i];
		vec2 dir = normalize(body.xy - pos);
		float dist = distance(pos, body.xy) / scale;
		float gravity = pow(dist, 2);
		gravity = (body.z * EarthMass) / gravity;
		heading += gravity * dir;
	}
	return heading;
}

void fragment() {
	if (elements > 0) {
		vec2 h = getHeading(vec2(0,0));
		float x = 2.5f;
		if (speed >= 200.0f) {
			float s = speed / 2.0f;
			if (speed >= 1000.0f) {
				s = 500.0f; 
				x *= 2.0f;
			}
			x = round(x * (100.0f / s));
		}
		x /= zoom;
		vec2 hx = getHeading(vec2(x,0));
		vec2 hy = getHeading(vec2(0,-x));
		float l = length(h);
		float lx  = length(hx);
		float ly  = length(hy);
		
		float measure = 10.0f;
		/*if (l >= 100.0f) {
			measure = 10.0f;
		} if (l >= 1000.0f) {
			measure = 100.0f;
		} */
		float d = mod(l, measure);
		float dx = mod(lx, measure);
		float dy = mod(measure, ly); 
		if ((l > lx) && (d < dx)) { // dom src of gravity to the left, x dist to the right is across the line
			COLOR = vec4(0.0, 1.0, 0.0, 1.0);
		}
		if ((l < lx) && (d > dx)) { // dom src of gravity to the right, i am across the line from x dist to right
			COLOR = vec4(0.0, 1.0, 0.0, 1.0);
		}
		/*else if (abs(l - lx) <= 0.25 && abs(d - dy) < 0.0075f) {
			COLOR = vec4(0.0, 1.0, 0.0, 1.0);
		} else {
			//COLOR = vec4(0.0, 1.0, 0.0, 0.1);
		}*/
		/*if (d > dy) {
			COLOR = vec4(0.0, 1.0, 0.0, 1.0);
		}
		if ((l > ly) && (d > 0.0f && d < dy)) {
			COLOR = vec4(0.0, 1.0, 0.0, 1.0);
		}
		if ((l < ly) && (d > 0.0f && d > dy)) {
			COLOR = vec4(0.0, 1.0, 0.0, 1.0);
		}*/
	}
}


