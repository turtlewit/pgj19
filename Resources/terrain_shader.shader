shader_type spatial;
render_mode blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_schlick_ggx;
uniform vec4 albedo : hint_color;
uniform sampler2D texture_albedo : hint_albedo;
uniform sampler2D texture_grass : hint_albedo;
uniform vec4 color_grass : hint_color;
uniform vec4 color_stone : hint_color;
uniform float specular;
uniform float metallic;
uniform float roughness : hint_range(0,1);
uniform float point_size : hint_range(0,128);
uniform sampler2D texture_metallic : hint_white;
uniform vec4 metallic_texture_channel;
uniform sampler2D texture_roughness : hint_white;
uniform vec4 roughness_texture_channel;
uniform vec3 uv1_scale;
uniform vec3 uv1_offset;
uniform vec3 uv2_scale;
uniform vec3 uv2_offset;

varying vec3 norm;


void vertex() {
	norm = NORMAL;
	UV=UV*uv1_scale.xy+uv1_offset.xy;
}




void fragment() {
	vec2 base_uv = UV;
	vec4 albedo_tex = texture(texture_albedo,base_uv * 16.);
	vec4 grass_tex = texture(texture_grass,base_uv * 8.);
	ALBEDO = albedo.rgb * (((1.-norm.y) * albedo_tex.rgb * color_stone.rgb) + (norm.y * grass_tex.rgb * color_grass.rgb));
	//ALBEDO = norm;
	float metallic_tex = dot(texture(texture_metallic,base_uv),metallic_texture_channel);
	METALLIC = 0.;
	float roughness_tex = dot(texture(texture_roughness,base_uv),roughness_texture_channel);
	ROUGHNESS = 1.;
	SPECULAR = specular;
}
