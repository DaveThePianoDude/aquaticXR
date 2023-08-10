// Upgrade NOTE: replaced 'PositionFog()' with transforming position into clip space.
// Upgrade NOTE: replaced 'V2F_POS_FOG' with 'float4 pos : SV_POSITION'

// Transparent_ShadowSupport.shader  
// Author: Noisecrime
// Date:   14.10.10
// Version 0.3
// Based on the Normal-Diffuse.shader
 
 
// INFO:
// Special FX shader that will only render a shadow onto the polygons, outside of the shadow area will be completely transparent.
// Shadow can be given a specific colour and alpha partly controls intensity, though mainly still controlled via light>shadow property.
// This is useful if you want to have a shadow on a ground plane, but not display the actual plane.
// Currently only works for Directional Lights.
 
 
// USAGE:
// Just apply the shader to a material.
 
// NOTES:
// Not tested with fog and may require tweaking to work with specific projects.
 
 
Shader "FX/Transparent_ShadowSupport" {
Properties
{
    _Color ("Shadow Color", Color) = (1,1,1,1)
}
 
Category {
    //Tags { "RenderType"="Opaque" }
    //  Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
    Tags { "RenderType"="Transparent" }
    LOD 200
    Zwrite Off
    Blend  SrcAlpha OneMinusSrcAlpha
    Fog { Color [_AddFog] }  // or  Fog { Color (0,0,0,0) }
    Lighting Off
   
    // ------------------------------------------------------------------
    // ARB fragment program
   
    #warning Upgrade NOTE: SubShader commented out; uses Unity 2.x per-pixel lighting. You should rewrite shader into a Surface Shader.
/*SubShader
    {
        // Pixel lights
        Pass
        {
            Name "PPL"
            Tags { "LightMode" = "Pixel" }
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_builtin
                #pragma fragmentoption ARB_fog_exp2
                #pragma fragmentoption ARB_precision_hint_fastest
                #include "UnityCG.cginc"
                #include "AutoLight.cginc"
 
                struct v2f {
                    float4 pos : SV_POSITION;
                    LIGHTING_COORDS
                        };
 
                uniform float4 _Color;
 
                v2f vert (appdata_base v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos (v.vertex);
                    TRANSFER_VERTEX_TO_FRAGMENT(o);
                                           
                    return o;
                }
 
 
                float4 frag (v2f i) : COLOR
                {
                    half4 texcol = _Color*(1.0f-SHADOW_ATTENUATION(i));
                    return texcol;  
                }
                ENDCG
 
        }
    }*/
}
 
Fallback "VertexLit", 2
 
}