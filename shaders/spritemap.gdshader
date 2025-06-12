shader_type canvas_item;

uniform sampler2D sprite;
uniform sampler2D spriteMap;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
//	vec4 spriteColor = texture(sprite, UV);
//	vec2 coords = vec2(spriteColor.r * 255., spriteColor.g * 255.);
 //   vec4 spriteMapColor = texture(spriteMap, coords);

//	COLOR = spriteMapColor;

	//take color 1 as uv to pull color from color2, set tit o COLOR
	// Called for every pixel the material is visible on.

}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
