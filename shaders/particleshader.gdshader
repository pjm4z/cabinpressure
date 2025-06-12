shader_type particles;
uniform vec3 gravity;

void process() {
	VELOCITY +=  /*gravity*/ vec3(0,10000,0) * DELTA;
	// Called for every vertex the material is visible on.
}


//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
