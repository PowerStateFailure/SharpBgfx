$input v_pos, v_view, v_normal, v_color0

/*
 * Copyright 2011-2017 Branimir Karadzic. All rights reserved.
 * License: https://github.com/bkaradzic/bgfx#license-bsd-2-clause
 */

#include "../common.sh"

uniform vec4 u_time;

vec2 blinn(vec3 _lightDir, vec3 _normal, vec3 _viewDir)
{
	float ndotl = dot(_normal, _lightDir);
	vec3 reflected = _lightDir - 2.0*ndotl*_normal; // reflect(_lightDir, _normal);
	float rdotv = dot(reflected, _viewDir);
	return vec2(ndotl, rdotv);
}

float fresnel(float _ndotl, float _bias, float _pow)
{
	float facing = (1.0 - _ndotl);
	return max(_bias + (1.0 - _bias) * pow(facing, _pow), 0.0);
}

vec4 lit(float _ndotl, float _rdotv, float _m)
{
	float diff = max(0.0, _ndotl);
	float spec = step(0.0, _ndotl) * max(0.0, _rdotv * _m);
	return vec4(1.0, diff, spec, 1.0);
}

void main()
{
	vec3 lightDir = vec3(0.0, 0.0, -1.0);
	vec3 normal = normalize(v_normal);
	vec3 view = normalize(v_view);
	vec2 bln = blinn(lightDir, normal, view);
	vec4 lc = lit(bln.x, bln.y, 1.0);
	float fres = fresnel(bln.x, 0.2, 5.0);

	float index = ( (sin(v_pos.x*3.0+u_time.x)*0.3+0.7)
				+ (  cos(v_pos.y*3.0+u_time.x)*0.4+0.6)
				+ (  cos(v_pos.z*3.0+u_time.x)*0.2+0.8)
				)*M_PI;

	vec3 color = vec3(sin(index*8.0)*0.4 + 0.6
					, sin(index*4.0)*0.4 + 0.6
					, sin(index*2.0)*0.4 + 0.6
					) * v_color0.xyz;

	gl_FragColor.xyz = pow(vec3(0.07, 0.06, 0.08) + color*lc.y + fres*pow(lc.z, 128.0), vec3_splat(1.0/2.2) );
	gl_FragColor.w = 1.0;
}
