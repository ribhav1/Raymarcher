#version 330 core
out vec4 FragColor;

uniform float elapsedTime;
uniform vec3 iResolution;
uniform vec3 camPos;
uniform vec3 camForward;
uniform vec3 camRight;
uniform vec3 camUp;
uniform float fov;

#define MAX_OBJS 32
#define MAX_LIGHTS 32

uniform int objCount;
uniform vec3 objPos[MAX_OBJS];
uniform vec4 objColor[MAX_OBJS];
uniform float objReflect[MAX_OBJS];
uniform int objType[MAX_OBJS];
uniform mat3 objInvRotMat[MAX_OBJS];

uniform float sphereRadii[MAX_OBJS];
uniform vec3 boxSizes[MAX_OBJS];

uniform vec3 sunDir;
uniform vec3 sunColor;

const float MAX_DIST = 50.0;
const int MAX_STEPS = 80;

float map(vec3 p, out int id, out bool isLight) {
    float minD = 1e9;
    id = -1;
    isLight = false;
    for (int i = 0; i < objCount; i++)
    {
        float type = objType[i];
        float d;

        // to render rotations, apply the inverse rotation to the input point
        if (type == 0 || type == 1) d = distance(p, objPos[i]) - sphereRadii[i];
        if (type == 2)
        {
            // move point to local space
            vec3 q = p - objPos[i];
            q = objInvRotMat[i] * q;
            q = abs(q) - boxSizes[i];
            d = length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0);
        }
        if (d < minD) {
            minD = d;
            id = i;
            if (type == 0) isLight = true;
         
        }
    }
    return minD;
}

float map(vec3 p, out int id) {
    bool dummy;
    return map(p, id, dummy);
}

float map(vec3 p) {
    bool dummyBool;
    int dummyInt;
    return map(p, dummyInt, dummyBool);
}

vec3 getNormal(vec3 p, int id, float t) {
    float eps = 0.001;
    return normalize(vec3(
        map(p + vec3(eps,0,0), id) - map(p - vec3(eps,0,0), id),
        map(p + vec3(0,eps,0), id) - map(p - vec3(0,eps,0), id),
        map(p + vec3(0,0,eps), id) - map(p - vec3(0,0,eps), id)
    ));
}

bool isInShadow(vec3 p, vec3 n, vec3 dirToLight, float maxDist) {
    // start a little outside surface
    float t = 0.01;
    vec3 ro = p + n * 0.01;

    for (int i = 0; i < MAX_STEPS; i++) {
        vec3 q = ro + dirToLight * t;
        int id;
        bool isLight;
        float d = map(q, id, isLight);
        
        if (d < 0.001) {
            if (isLight) return false; // if hit but is light, not blocked
            return true; // if hit, blocked
        }
        
        if (t >= maxDist) break; // too far, not blocked

        t += d;
    }
    return false;
}

vec3 getEnvironment(vec3 rd) {
    // simple gradient sky, up = blue, down = orange/grey
    float t = 0.5 * (rd.y + 1.0);
    return mix(vec3(0.8, 0.7, 0.6), vec3(0.2, 0.5, 0.9), t);
}

vec3 getSunContribution(vec3 p, vec3 n, vec3 rd, vec3 sunDir, vec3 sunColor)
{
    vec3 lightDir = normalize(sunDir);
    float diff = max(dot(n, lightDir), 0.0); // calculate how much normal vector is pointing to sun

    // blinn-phone specular, both the sun and scene lighting will contribute diffuse and specular
    vec3 viewDir = normalize(camPos - p);
    vec3 halfVec = normalize(lightDir + viewDir);
    float spec = clamp(pow(max(dot(n, halfVec), 0.0), 32.0), 0.0, 1.0);

    return sunColor * (diff + 0.2 * spec);
}

void raymarch(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;

    float focal = 1.0 / tan(radians(fov) / 2.0);

    vec3 ro = camPos;
    vec3 rd = normalize(focal * camForward + uv.x * camRight + uv.y * camUp);

    vec3 finalColor = vec3(0.0);
    float r_last = 1.0;

    bool hit = false;

    for (int bounce = 0; bounce < 2; bounce++) {
        float t = 0.0;
        
        int id = -1;
        bool isLight = false;

        for (int i = 0; i < MAX_STEPS; i++) {
            vec3 p = ro + rd * t;
            float d = map(p, id, isLight);

            // check for hit
            if (d < 0.001 * t) {
                hit = true;

                // hit a light sphere set pixel as white
                if (isLight) {
                    fragColor = vec4(vec3(1.0), 1.0);
                    return;
                }

                vec3 n = getNormal(p, id, t);

                // sun lighting contributions
                vec3 sunContribution = vec3(0.0);
                vec3 lightDir = normalize(sunDir);
                if (!isInShadow(p, n, lightDir, MAX_DIST)) {
                    sunContribution = getSunContribution(p, n, rd, sunDir, sunColor);
                }

                // scene lights lighting contribution
                vec3 sceneLighting = vec3(0.0);
                for (int j = 0; j < objCount; j++) {
                    if (objType[j] == 0) { // spheres used as lights
                        vec3 lightDir = normalize(objPos[j] - p);

                        float distToLight =  length(objPos[j] - p);

                        if (!isInShadow(p, n, lightDir, distToLight))
                        {
                            // diffuse term
                            float diffuse = max(dot(n, lightDir), 0.0);

                            // specular term
                            vec3 viewDir = normalize(camPos - p);
                            vec3 halfVec = normalize(lightDir + viewDir);
                            float spec = pow(max(dot(n, halfVec), 0.0), 32.0);

                            sceneLighting += objColor[j].rgb * (diffuse + 0.2 * spec);
                        }
                    }
                }

                vec3 ambient = vec3(0.0);

                float r_current = objReflect[id];

                finalColor += r_last * (1.0 - r_current) *  objColor[id].rgb * (sceneLighting + ambient + sunContribution);
                r_last *= r_current;

                // push ray slightly outside surface
                ro = p + n * 0.01;
                rd = reflect(rd, n);
                
                break;
            }

            if (t > MAX_DIST)
            {
                finalColor += r_last * getEnvironment(rd) * 0.5;
                r_last = 0.0;
                break;
            }
        
            // step forward
            t += d;
        }
    }

    fragColor = vec4(finalColor, 1.0);

    // background if no hit
    if (!hit) fragColor = vec4(getEnvironment(rd), 1.0);
}
    

void main() {
    vec4 col;
    raymarch(col, gl_FragCoord.xy);
    FragColor = col;
}