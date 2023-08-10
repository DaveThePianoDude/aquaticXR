using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Threading;
using System;


public struct uniItem
{
	public uint Key;
	public uint Value;
	public uniItem(uint aKey, uint aVal){ 
		Key = aKey; 
		Value = aVal;
	}
}


public struct cVec3i
{
	public int x;
	public int y;
	public int z;
}


public class OneLayerInfo
{

	static public void RGBtoHCY (ref Color rgb, ref Color hsl){
		float r = rgb.r;
		float g = rgb.g;
		float b = rgb.b;
		float h, s, v;
		float mn = r, mx = r;
		int maxVal = 0;
		if (g > mx) { mx = g; maxVal = 1; }
		if (b > mx) { mx = b; maxVal = 2; }
		if (g < mn) mn = g;
		if (b < mn) mn = b;
		float  delta = mx - mn;
		v = 0.3f*r+0.59f*g+0.11f*b;
		if (delta != 0){
			s = delta;
		}else{
			s = 0;
			h = 0;
			goto ass_end;
		}
		if (s == 0.0f){
			h = 0; 
			goto ass_end;
		}else{
			switch (maxVal){
			case 0: h = 0 + ( g - b ) / delta; break;    
			case 1: h = 2 + ( b - r ) / delta; break;    
			case 2: h = 4 + ( r - g ) / delta; break;    
			default: h = 0 ; break ; 
			}
		}
		h *= 60;
		if (h < 0) h += 360;
	ass_end:   
		hsl.r = h;
		hsl.g = s;
		hsl.b = v;
	}


	static public void HCYtoRGB (ref Color hsv, ref Color rgb){
		while(hsv.r<0)hsv.r+=360;
		while(hsv.r>=360)hsv.r-=360;
		int i;
		float hTemp;
		float h = hsv.r;
		float s = hsv.g;
		float v = hsv.b;
		float r = 0, g = 0, b = 0;
		if (s == 0.0f || h == -1.0f){ // s==0? Totally unsaturated = grey
			rgb.r = rgb.g = rgb.b = v;
		}
		float C=s;
		hTemp = h / 60.0f;
		//float X=C*(1-__abs(fmod(hTemp,2.0f)-1));	
		float v0=0;
		float cmax=1;
		for(int j=0;j<2;j++){
			if(s>cmax)s=cmax;
		C=j>0?s:1.0f;
			float X=C*(1-Mathf.Abs((hTemp % 2.0f)-1));
			i = (int) Mathf.Floor(hTemp);                 // which sector
			switch (i){
			case 0: r = C; g = X; b = 0; break;
			case 1: r = X; g = C; b = 0; break;
			case 2: r = 0; g = C; b = X; break;
			case 3: r = 0; g = X; b = C; break;
			case 4: r = X; g = 0; b = C; break;
			case 5: r = C; g = 0; b = X; break;
			default: r=0 ; g=0 ; b=0 ; break ; // Should never occur
			}
			if(j==0){
				v0=0.3f*r+0.59f*g+0.11f*b;
				if(v<v0)cmax=v/v0;
				else cmax=(1-v)/(1-v0);
			}
		}
		float m=v-(0.3f*r+0.59f*g+0.11f*b);
		rgb.r = b+m;
		rgb.g = g+m;
		rgb.b = r+m;
	}

	static public void toRGB3 (ref Color clr){
		Color hsv = new Color(clr.r,clr.g,clr.b);	
		if(clr.g<0)clr.g=0;
		if(clr.g>1)clr.g=1;
		HCYtoRGB(ref hsv,ref clr);	
	}
	static public void toHCY (ref Color clr){
		Color hsv = new Color(clr.r,clr.g,clr.b);	
		RGBtoHCY(ref hsv,ref clr);	
	}

	static public float Getcpow(Color c){
		return c.b*0.3f+c.g*0.59f+c.r*0.11f;
	}

	static public void nrmcolor(ref Color c){
		float mx=0;
		for(int i=0;i<3;i++)mx=Mathf.Max(mx,c[i]);
		if (mx > 1.0f) {
			c.r /= mx;
			c.g /= mx;
			c.b /= mx;
		}
	}


    static public void Apply_Op_Dissolve(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {

            Color B1 = B[i];
            float a = B1.a*t;
            if(UnityEngine.Random.Range(0,1000)/1000.0f <= a) A[i] =  B1;
        }
    }

    static public void Apply_Op_Blend_Normal(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color B1 = B[i];
            float a = B1.a*t;
            A[i] = A[i] * (1.0f-a) + B1 * a;
        }
    }

    static public void Apply_Op_Blend_Hue(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            //		float s1=Getcpow(B1);
            toHCY(ref A[i]);
            toHCY(ref B1);
            if (A[i].g == 0)
            {
                A[i].b = B1.b;
            }
            else
            {
                A[i].g = B1.g;
                A[i].b = B1.b;
            }
            toRGB3(ref A[i]);
            A[i] *= 255;
            nrmcolor(ref A[i]);
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
	}
    static public void Apply_Op_Blend_Saturation(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            toHCY(ref A[i]);
            toHCY(ref B1);
            if (B1.g != 0) A[i].r = B1.r;
            else A[i].g = 0;
            A[i].b = B1.b;
            toRGB3(ref A[i]);
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
	}
    static public void Apply_Op_Blend_Color(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            toHCY(ref A[i]);
            toHCY(ref B1);
            A[i].b = B1.b;
            toRGB3(ref A[i]);
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
	}
    static public void Apply_Op_Blend_Lightnrss(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            toHCY(ref A[i]);
            toHCY(ref B1);
            if (B1.g != 0) A[i].r = B1.r;
            A[i].g = B1.g;
            toRGB3(ref A[i]);
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
	}

    static public void ApplyOp_Modulate(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            A[i] = A[i] * B1;
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
	}
    static public void ApplyOp_Modulate2x(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            A[i] = A[i] * B1 * 2;
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
    }
    static public void ApplyOp_Add(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            A[i] = A[i] + B1;
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
    }
    static public void ApplyOp_Subtract(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            A[i] = A[i] - B1;
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
    }
    static public void ApplyOp_Max(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            A[i].r = Mathf.Max(B1.r, A[i].r);
            A[i].g = Mathf.Max(B1.g, A[i].g);
            A[i].b = Mathf.Max(B1.b, A[i].b);
            A[i].a = B1.a;
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
	}
    static public void ApplyOp_Min(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            A[i].r = Mathf.Min(B1.r, A[i].r);
            A[i].g = Mathf.Min(B1.g, A[i].g);
            A[i].b = Mathf.Min(B1.b, A[i].b);
            A[i].a = B1.a;
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
    }
	//PS blending types
    static public void Apply_Op_Blend_Lighten(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            A[i].r = Mathf.Max(B1.r, A[i].r);
            A[i].g = Mathf.Max(B1.g, A[i].g);
            A[i].b = Mathf.Max(B1.b, A[i].b);
            A[i].a = Mathf.Max(B1.a, A[i].a);
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
    }
    static public void Apply_Op_Blend_Darken(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            A[i].r = Mathf.Min(B1.r, A[i].r);
            A[i].g = Mathf.Min(B1.g, A[i].g);
            A[i].b = Mathf.Min(B1.b, A[i].b);
            A[i].a = Mathf.Min(B1.a, A[i].a);
            float a = B1.a * t;
            A[i] =  OldA * (1.0f - a) + A[i] * a;
        }
	}
    static public void Apply_Op_Blend_Multiply(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            A[i] = A[i] * B1;
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
    }
    static public void Apply_Op_Blend_Average(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            A[i] = (A[i] + B1) / 2.0f;
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
    }
    static public void Apply_Op_Blend_Add(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            A[i] = A[i] + B1;
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
    }
    static public void Apply_Op_Blend_Subtract(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            A[i] = A[i] + B1 - Color.white;
            if (A[i].r < 0.0f) A[i].r = 0.0f;
            if (A[i].g < 0.0f) A[i].g = 0.0f;
            if (A[i].b < 0.0f) A[i].b = 0.0f;
            if (A[i].a < 0.0f) A[i].a = 0.0f;
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
    }
    static public void Apply_Op_Blend_Difference(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            A[i] -= B1;
            A[i].r = Mathf.Abs(A[i].r);
            A[i].g = Mathf.Abs(A[i].g);
            A[i].b = Mathf.Abs(A[i].b);
            A[i].a = Mathf.Abs(A[i].a);
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
	}
    static public void Apply_Op_Blend_Negation(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            A[i].r = 1.0f - Mathf.Abs(1.0f - A[i].r - B1.r);
            A[i].g = 1.0f - Mathf.Abs(1.0f - A[i].g - B1.g);
            A[i].b = 1.0f - Mathf.Abs(1.0f - A[i].b - B1.b);
            A[i].a = 1.0f - Mathf.Abs(1.0f - A[i].a - B1.a);
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
	}
    static public void Apply_Op_Blend_Screen(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            A[i].r = 1.0f - (1.0f - A[i].r) * (1.0f - B1.r) / 1.0f;
            A[i].g = 1.0f - (1.0f - A[i].g) * (1.0f - B1.g) / 1.0f;
            A[i].b = 1.0f - (1.0f - A[i].b) * (1.0f - B1.b) / 1.0f;
            A[i].a = 1.0f - (1.0f - A[i].a) * (1.0f - B1.a) / 1.0f;
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
	}
    static public void Apply_Op_Blend_Exclusion(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            A[i].r = A[i].r + B1.r - A[i].r * B1.r * 2.0f;
            A[i].g = A[i].g + B1.g - A[i].g * B1.g * 2.0f;
            A[i].b = A[i].b + B1.b - A[i].b * B1.b * 2.0f;
            A[i].a = A[i].a + B1.a - A[i].a * B1.a * 2.0f;
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
	}
    static public void Apply_Op_Blend_Overlay(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int j = 0; j < 256; j++)
        {
            Color OldA = A[j];
            Color B1 = B[j];
            for (int i = 0; i < 4; i++)
            {
                if (B1[i] < 0.5f) A[j][i] *= B1[i] * 2.0f / 1.0f;
                else A[j][i] = 1.0f - (1.0f - A[j][i]) * (1.0f - B1[i]) / 0.5f;
            }
            float a = B1.a * t;
            A[j] = OldA * (1.0f - a) + A[j] * a;
        }
	}
    static public void Apply_Op_Blend_Overlay(ref Color A, ref Color B, ref float t)
    {
            Color B1 = B;
            for (int i = 0; i < 4; i++)
            {
                if (B1[i] < 0.5f) A[i] *= B1[i] * 2.0f / 1.0f;
                else A[i] = 1.0f - (1.0f - A[i]) * (1.0f - B1[i]) / 0.5f;
            }
    }

    static public void Apply_Op_Blend_SoftLight(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int j = 0; j < 256; j++)
        {
            Color OldA = A[j];
            Color B1 = B[j];
            for (int i = 0; i < 4; i++)
            {

                if (A[j][i] <= 0.5f) A[j][i] = (1.0f - 2.0f * A[j][i]) * B1[i] * B1[i] / 2.0f + 2.0f * A[j][i] * B1[i];
                else A[j][i] = (2.0f * A[j][i] - 1.0f) * Mathf.Sqrt(B1[i]) + 2.0f * (1.0f - A[j][i]) * B1[i];

            }
            float a = B1.a * t;
            A[j] = OldA * (1.0f - a) + A[j] * a;
        }
	}

    static public void Apply_Op_Blend_HardLight(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color A1 = A[i];
            Color B1 = B[i];
            A[i] = B1;
            Apply_Op_Blend_Overlay(ref A[i], ref A1, ref t);
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
	}

    static public void Apply_Op_Blend_ColorDodge(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int j = 0; j < 256; j++)
        {
            Color OldA = A[j];
            Color B1 = B[j];
            for (int i = 0; i < 4; i++)
            {
                A[i] *= t;
                float s = A[j][i] + B1[i];
                if (s == 0) A[j][i] = 0;
                else if (s <= 1.0f) A[j][i] = 1.0f * B1[i] / (1.0f - A[j][i]);
                else if (s > 1.0f) A[j][i] = 1.0f;

            }
            t *= 20;
            if (t > 1) t = 1;
            float a = B1.a * t;
            A[j] = OldA * (1.0f - a) + A[j] * a;
        }
	}

    static public void Apply_Op_Blend_ColorBurn(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int j = 0; j < 256; j++)
        {
            Color OldA = A[j];
            Color B1 = B[j];  
            for (int i = 0; i < 4; i++)
            {
                A[j][i] *= t;
                A[j][i] += (1.0f - t);
                float s = A[j][i] + B1[i];
                if (B1[i] == 1.0f) A[j][i] = 1.0f;
                else if (s >= 1.0f) A[j][i] = (1.0f - (1.0f - B1[i]) / A[j][i]);
                else A[j][i] = 0.0f;
            }
            if (t > 1) t = 1;
            t *= 20;
            if (t > 1) t = 1;
            float a = B1.a * t;
            A[j] = OldA * (1.0f - a) + A[j] * a;
        }
	}
    static public void Apply_Op_Blend_LinearDodge(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color B1 = B[i];
            A[i] += B1;
            if (A[i].r > 255.0f) A[i].r = 255.0f;
            if (A[i].g > 255.0f) A[i].g = 255.0f;
            if (A[i].b > 255.0f) A[i].b = 255.0f;
            if (A[i].a > 255.0f) A[i].a = 255.0f;
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
	}
    static public void Apply_Op_Blend_LinearBurn(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int i = 0; i < 256; i++)
        {
            Color OldA = A[i];
            Color S = Color.white;
            S -= A[i];
            Color B1 = B[i];
            A[i] = B1;
            A[i] -= S;
            if (A[i].r < 0) A[i].r = 0;
            if (A[i].g < 0) A[i].g = 0;
            if (A[i].b < 0) A[i].b = 0;
            if (A[i].a < 0) A[i].a = 0;
            A[i] *= t;
            S = B1;
            S *= 1.0f - t;
            A[i] += S;
            t *= 20;
            if (t > 1) t = 1;
            float a = B1.a * t;
            A[i] = OldA * (1.0f - a) + A[i] * a;
        }
	}
    static public void Apply_Op_Blend_LinearLight(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int j = 0; j < 256; j++)
        {
            Color OldA = A[j];
            Color B1 = B[j];
            for (int i = 0; i < 4; i++)
            {
                A[j][i] = B1[i] + 2 * A[j][i] - 1.0f;
                if (A[j][i] < 0) A[j][i] = 0.0f;
                if (A[j][i] > 1.0f) A[j][i] = 1.0f;
            }
            float a = B1.a * t;
            A[j] = OldA * (1.0f - a) + A[j] * a;
        }
	}
    static public void Apply_Op_Blend_VividLight(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int j = 0; j < 256; j++)
        {
            Color OldA = A[j];
            Color B1 = B[j];
            for (int i = 0; i < 4; i++)
            {
                if (A[j][i] == 1.0f && B1[i] == 0.0f) A[j][i] = 1.0f;
                else if (A[j][i] <= 0.5f && B1[i] + 2.0f *  A[j][i] >= 255) A[j][i] = (B1[i] - (1.0f - 2.0f * A[j][i])) / 2.0f / A[j][i];
                else if (A[j][i] <= 0.5f && B1[i] + 2.0f *  A[j][i] <  255) A[j][i] = 0.0f;
                else if (A[j][i] >  0.5f && B1[i] + 2.0f * (A[j][i] -  128) <= 255) A[j][i] = B1[i] / (1.0f - 2.0f * (A[j][i] - 0.5f));
                else A[j][i] = 1.0f;
            }
            float a = B1.a * t;
            A[j] = OldA * (1.0f - a) + A[j] * a;
        }
	}
    static public void Apply_Op_Blend_PinLight(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int j = 0; j < 256; j++)
        {
            Color OldA = A[j];
            Color B1 = B[j];
            for (int i = 0; i < 4; i++)
            {
                if  (A[j][i] > 0.5f) A[j][i] = Mathf.Max(B1[i], 2.0f * A[j][i] - 1.0f);
                else A[j][i] = Mathf.Min(B1[i], 2.0f * A[j][i]);
                if  (A[j][i] < 0.0f) A[j][i] = 0;
                if  (A[j][i] > 1.0f) A[j][i] = 1.0f;
            }
            float a = B1.a * t;
            A[j] = OldA * (1.0f - a) + A[j] * a;
        }
	}
    static public void Apply_Op_Blend_HardMix(ref Color[] A, ref Color32[] B, ref float t)
    {
        for (int j = 0; j < 256; j++)
        {
            Color OldA = A[j];
            Color B1 = B[j];
            for (int i = 0; i < 4; i++)
            {
                if (A[j][i] >= 0.5f)
                {
                    if (A[j][i] <= 1.0f - B1[i]) A[j][i] = 0.0f;
                    else A[j][i] = 1.0f;
                }
                else
                {
                    if (A[j][i] < 1.0f - B1[i]) A[j][i] = 0.0f;
                    else A[j][i] = 1.0f;
                }
            }
            float a = B1.a * t;
            A[j] = OldA * (1.0f - a) + A[j] * a;
        }
	}

    public delegate void Op_Blend_Method(ref Color[] A, ref Color32[] B, ref float t);

    static public Op_Blend_Method[] LOP_list = new Op_Blend_Method[30]{Apply_Op_Blend_Normal, Apply_Op_Dissolve,
		Apply_Op_Blend_Darken,
		Apply_Op_Blend_Multiply,
		Apply_Op_Blend_ColorBurn,
		Apply_Op_Blend_LinearBurn,
		Apply_Op_Blend_Lighten,
		Apply_Op_Blend_Screen,
		Apply_Op_Blend_ColorDodge,
		Apply_Op_Blend_LinearDodge,
		Apply_Op_Blend_Overlay,
		Apply_Op_Blend_SoftLight,
		Apply_Op_Blend_HardLight,
		Apply_Op_Blend_VividLight,
		Apply_Op_Blend_LinearLight,
		Apply_Op_Blend_PinLight,
		Apply_Op_Blend_HardMix,
		Apply_Op_Blend_Difference,
		Apply_Op_Blend_Exclusion,
		Apply_Op_Blend_Saturation,
		Apply_Op_Blend_Hue,
		Apply_Op_Blend_Lightnrss,
		Apply_Op_Blend_Color,
		ApplyOp_Modulate2x,
		ApplyOp_Add,
		ApplyOp_Subtract,
		null,
		null,
		null,
		null};

	public string Name;
	public int LayerID;
	public bool Visible;
	public bool IsFolder;
	public bool EndOfSection; 
	public bool IsOpen;
	public bool LockTransparency;
	public bool UseAsWeightmap;
	public int ColorOp; // 26 - normalMap; 27 - colored specular; 28,29 - emissive; 23 - Overlay; 3 - Multiply; 
	public int DepthOp;
	public float EmbossPower;
	public float DepthTransparency;
	public float ColorTransparency;
	public float Contrast;
	public float Brightness;
	public float SpecContrast;
	public float SpecularMod;
	public float SpecBrightness;
	public float GlossMod;
	public float RoughMod;
	public float MetalnessOpacity;
	public float MetalBrightness;
	public string LinkedLayer;
	public bool InverseLinkage;
	public string HiddenMaskOwner;

	public int LinkedLayerId;
    public bool isOcclusion;
    public bool isCurvature;
    public OneLayerInfo ParentLayer = null;

    public List<MRenderTarget.LayersBlock> LayerBlocks = new List<MRenderTarget.LayersBlock>();

    public bool VisibleInHierarchy;

    public bool useCustomVisible = false;

    bool _CustomVisible = false;

    public void SetLayerDirty()
    {
//        Debug.Log("Dirty"+LayerBlocks.Count);
        foreach (MRenderTarget.LayersBlock lb in LayerBlocks)
        {
            lb.dirty = true;
        }

    }

    static public bool changeInLayers = false;
    public bool CustomVisible{
            get{
                if(useCustomVisible) return _CustomVisible;
                else return Visible;
            }
            set{
                if(value != CustomVisible){
                    SetLayerDirty();

                    useCustomVisible = true;
                    _CustomVisible = value;
                    changeInLayers = true;
                }
            }        
    }

    

    public int revisionId = -1;
}


public class MRenderTarget
{
	public string Name;
    public Asset3DCoat asset3DCoat;
	public int rtSizeX;
	public int rtSizeY;
	public int CTextureID = -1;
	public int NTextureID = -1;
	public int STextureID = -1;
	public int SubPatchLevel;
	public bool DrawMicrovertices;
	public bool SkipNormalmap;
	public bool UseExternalTexture = false;
    public Material material;
    public Material materialAlpha;
    public Texture2D Bump;
    public Texture2D HeightMap;
    public Texture2D Occlusion;
    public Texture2D Curvature;
    public Texture2D MetallicGlossOc;
	public Texture2D Albedo;
	public Texture2D Emission;
	//	public ModelPointsScope* DPData=null;
//	public AllocPatchData* ALD=null;

    public Color32[] cNormMap;
    public byte[] meshMask;

    public const int QuadSize = 16;
    public const int QuadSh = 4;
    public const int QuadSq = (QuadSize * QuadSize);

    public const int TexQuadSize = 16;
    public const int TexQuadSh = 4;
    public float[] bumpBuf;// = new float[QuadSq];

    public LayersBlock[] QuadBlocksMatrix;

    public MRenderTarget()
    {
        for(int i = 0; i < 8; i++){
            threads[i] = new ThreadQuads();
        }
    }


    public class ThraedQuad
    {
        public LayersBlock mrb;
        public bool finished = true;
        public int PosX;
        public int PosY;
        public Color[] metallicGlossOcQuad = new Color[QuadSq];
        public float[] metallicQuad = new float[QuadSq];
        public Color[] occlusionQuad = new Color[QuadSq];
        public Color[] curvatureQuad = new Color[QuadSq];
        public Color[] objNormQuad = new Color[QuadSq];
        public Color[] bumpMapQuad = new Color[QuadSq];
        public float[] heightMapQuad = new float[QuadSq];
        public Color[] emissionQuad = new Color[QuadSq];
        public Color[] albedoQuad = new Color[QuadSq];
        public bool ibAlbedo = false;
        public bool ibEmission = false;
        public bool ibCurvature = false;
        public bool ibMetallicGlossOc = false;
        public bool ibOcclusion = false;
        public bool ibObjNormMap = false;

    }

    public class ThreadQuads
    {
        public bool finished = false;
        public ThraedQuad[] threadQuads = new ThraedQuad[64];
        public Thread thread;
        public MRenderTarget mrt;
        public int SavedId = 0;
        public int From = 0;
        public int To = 0;
        public ThreadQuads()
        {
            for (int i = 0; i < threadQuads.Length; i++)
                threadQuads[i] = new ThraedQuad();
        }
    }

    public ThreadQuads[] threads = new ThreadQuads[8];

    public bool thereAO = false;
    public bool thereCurvature = false;
    public bool thereEmission = false;
    public bool thereBump = false;
    public bool thereSpecular = false;
    public bool thereNormalMap = false;

    public int dirtyBlocksCount = 0;
    public int[] dirtyBlocks = new int[0];

    public class LayersBlock
    {
        public int posX;
        public int posY;
        public ushort[] freeze = null;

        public class RawBlock
        {
            public int LayerID;
            public ushort options;
            public RawBlock linkedBlock = null;

            public float[] bump = null;

            public byte[] Specular = null;
            public byte[] SpecMask = null;
            public byte[] Metalness = null;
            public Color32[] color = null;
        }


        public bool dirty = true;
        public RawBlock[] rawBlockLayers;

    }

    ThraedQuad mainThreadQuad = new ThraedQuad();

    static void updateBlocks(object param)
    {
        ThreadQuads threadQuads = param as ThreadQuads;
        threadQuads.finished = false;
        try
        {
            for (int i = threadQuads.From; i < threadQuads.To; i++)
            {

                int tqid = i % threadQuads.threadQuads.Length;
                int wn = 0;
                while (wn++ < 100 && threadQuads.threadQuads[tqid].finished) Thread.Sleep(1);
                threadQuads.threadQuads[tqid].finished = false;
                threadQuads.mrt.updateBlock(threadQuads.mrt.rawLayersBlocks[threadQuads.mrt.dirtyBlocks[i]], threadQuads.threadQuads[tqid]);
         //       threadQuads.mrt.ApplyThreadQuad(threadQuads.mrt.mainThreadQuad);
            }
        }
        finally
        {

            threadQuads.finished = true;
        }
    }


	public void CreateTxt2DPadding(ref Texture2D ipbufTxt){
		Color32[] ipbuf = ipbufTxt.GetPixels32 ();
		for (int i = 0; i < ipbuf.Length; i++) {
			if (meshMask [i] != 1)	meshMask[i] = 255;
		}
//		Debug.Log("mm"+meshMask.Length+" ss"+ipbuf.Length);
		for (int i = 0; i < ipbuf.Length - 1 - ipbufTxt.width; i++) {
			if (meshMask [i] < 250 && meshMask [i] < meshMask [i + 1]) {
				meshMask [i+1] = (byte)(meshMask [i]+1);
				ipbuf [i+1] = ipbuf [i];
			}

			if (meshMask [i] < 250 && meshMask [i] < meshMask [i + ipbufTxt.width]) {
				meshMask [i+ipbufTxt.width] =  (byte)(meshMask [i]+1);
				ipbuf [i+ipbufTxt.width] = ipbuf [i];
			}
		}
		for (int i = ipbuf.Length - 1; i > ipbufTxt.width; i--) {
			if (meshMask [i] < 250 && meshMask [i] < meshMask [i - 1]) {
				meshMask [i-1] = (byte)(meshMask [i]+1);
					ipbuf [i-1] = ipbuf [i];
				}

			if (meshMask [i] < 250 && meshMask [i] < meshMask [i - ipbufTxt.width]) {
				meshMask [i-ipbufTxt.width] = (byte)(meshMask [i]+1);
					ipbuf [i-ipbufTxt.width] = ipbuf [i];
			}
		}

		ipbufTxt.SetPixels32 (ipbuf);

	}

	public void CreateBumpPadding(){
		float[] ipbuf = bumpBuf;
		Texture2D ipbufTxt = Albedo;
		for (int i = 0; i < ipbuf.Length; i++) {
			if (meshMask [i] != 1)	meshMask[i] = 255;
		}
		//		Debug.Log("mm"+meshMask.Length+" ss"+ipbuf.Length);
		for (int i = 0; i < ipbuf.Length - 1 - ipbufTxt.width; i++) {
			if (meshMask [i] < 250 && meshMask [i] < meshMask [i + 1]) {
				meshMask [i+1] = (byte)(meshMask [i]+1);
				ipbuf [i+1] = ipbuf [i];
			}

			if (meshMask [i] < 250 && meshMask [i] < meshMask [i + ipbufTxt.width]) {
				meshMask [i+ipbufTxt.width] = (byte)(meshMask [i]+1);
				ipbuf [i+ipbufTxt.width] = ipbuf [i];
			}
		}
		for (int i = ipbuf.Length - 1; i > ipbufTxt.width; i--) {
			if (meshMask [i] < 250 && meshMask [i] < meshMask [i - 1]) {
				meshMask [i-1] = (byte)(meshMask [i]+1);
				ipbuf [i-1] = ipbuf [i];
			}

			if (meshMask [i] < 250 && meshMask [i] < meshMask [i - ipbufTxt.width]) {
				meshMask [i-ipbufTxt.width] = (byte)(meshMask [i]+1);
				ipbuf [i-ipbufTxt.width] = ipbuf [i];
			}
		}

		bumpBuf = ipbuf;
	}

    public void UpdateDirtyBlocks()
    {
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();


        thereAO = false;
        thereCurvature = false;
        thereEmission = false;
        thereBump = false;
        thereSpecular = false;
        thereNormalMap = false;

        asset3DCoat.thereTransparent = false;

        Array.Clear(bumpBuf, 0, bumpBuf.Length);

        if (dirtyBlocks.Length == 0)
        {
            dirtyBlocks = new int[(rtSizeX / QuadSize) * (rtSizeY / QuadSize)];
        }
        dirtyBlocksCount = 0;

        for (int i = 0; i < rawLayersBlocks.Length; i++) if (rawLayersBlocks[i].dirty)
            {
                dirtyBlocks[dirtyBlocksCount] = i;
                dirtyBlocksCount++;
            }

   //     Thread thread = new Thread(updateBlocks);
 //       thread.Start();
		CreateBumpPadding();
        if (dirtyBlocksCount < 8)
        {
            for (int i = 0; i < dirtyBlocks.Length; i++)
            {
                updateBlock(rawLayersBlocks[dirtyBlocks[i]], mainThreadQuad);
                ApplyThreadQuad(mainThreadQuad);
            }
        }
        else
        {
            int tSize = (dirtyBlocksCount / threads.Length);
            for (int ti = 0; ti < threads.Length; ti++)
            {
                threads[ti].mrt = this;
                threads[ti].From = ti * tSize;
                threads[ti].To = Math.Min(dirtyBlocksCount, (ti + 1) * tSize);


                threads[ti].thread = new Thread(new ParameterizedThreadStart(updateBlocks));
                threads[ti].finished = false;
                threads[ti].SavedId = 0;
                for (int qi = 0; qi < threads[ti].threadQuads.Length; qi++)
                    threads[ti].threadQuads[qi].finished = false;
                threads[ti].thread.Start(threads[ti]);

            }
            bool finished = false;
            do{
                finished = true;
                for (int ti = 0; ti < threads.Length; ti++)
                {
/*                    while (threads[ti].threadQuads[threads[ti].SavedId].finished)
                    {
                        Debug.Log("Finished" + ti + "_" + threads[ti].SavedId);
                        ApplyThreadQuad(threads[ti].threadQuads[threads[ti].SavedId]);
                        threads[ti].threadQuads[threads[ti].SavedId].finished = false;
                        threads[ti].SavedId = (threads[ti].SavedId+1) % threads[ti].threadQuads.Length;
                    }*/
                    if (/*!threads[ti].finished*/threads[ti].thread.IsAlive) finished = false;
                    for (int tti = 0; tti < threads[ti].threadQuads.Length; tti++)
                    {
                        if (threads[ti].threadQuads[tti].finished)
                        {
                            ApplyThreadQuad(threads[ti].threadQuads[tti]);
                            threads[ti].threadQuads[tti].finished = false;
                            finished = false;
                        }
                    }
                    Thread.Sleep(1);
                }
            } while (!finished);
        }

        //        foreach (LayersBlock mrb in rawLayersBlocks) if (mrb.dirty)
  //          {

    //            updateBlock(mrb);
      //      }

     //   thread.Join();

        stopWatch.Stop();
//        long swLayersUpdate = stopWatch.ElapsedMilliseconds;
        stopWatch.Reset();
        stopWatch.Start();

        if (thereNormalMap) cNormMap = Bump.GetPixels32();
        foreach (LayersBlock mrb in rawLayersBlocks) if (mrb.dirty)
            {

                updateBumpBlock(mrb, mainThreadQuad);
                mrb.dirty = false;
            }
        stopWatch.Stop();
//        long swBumpUpdate = stopWatch.ElapsedMilliseconds;
        stopWatch.Reset();
        stopWatch.Start();


		if (asset3DCoat.CreatePadding) {
			CreateTxt2DPadding (ref Albedo);
		}
		Albedo.alphaIsTransparency = true;
		Albedo.Apply();


    //    if (thereSpecular)
    //    {
			if (asset3DCoat.CreatePadding) {
				CreateTxt2DPadding (ref MetallicGlossOc);
			}
		MetallicGlossOc.Apply();
    //    }



        if (/*thereBump &&*/ asset3DCoat.CreateNormalMap)
        {
			if (asset3DCoat.CreatePadding) {
				CreateTxt2DPadding (ref Bump);
			}
			Bump.Apply();

        }

        if (/*thereEmission &&*/ asset3DCoat.ImportEmission)
        {
			if(Emission != null){
				if (asset3DCoat.CreatePadding) {
					CreateTxt2DPadding (ref Emission);
				}
				Emission.Apply();
			}
        }

        if (/*thereAO &&*/ asset3DCoat.ExtractAO && !asset3DCoat.MargeAOAndMetallic)
        {
			if (asset3DCoat.CreatePadding) {
				CreateTxt2DPadding (ref Occlusion);
			}
			Occlusion.Apply();
        }


        if (/*thereCurvature &&*/ asset3DCoat.ExtractCurvature)
        {
			if (asset3DCoat.CreatePadding) {
				CreateTxt2DPadding (ref Curvature);
			}
			Curvature.Apply();
        }

        stopWatch.Stop();
//        long swMatUpdate = stopWatch.ElapsedMilliseconds;
        stopWatch.Reset();
        stopWatch.Start();
//        Debug.Log("UpdateLayers: " + swLayersUpdate + "ms  "+"BumpUpdate: " + swBumpUpdate + "ms  "+"MatUpdate: " + swMatUpdate + "ms  ");

    }

    public int Lx;
    public int Ly;
    public int MatID;

    public LayersBlock[] rawLayersBlocks;

    public void updateBumpBlock(LayersBlock mrb, ThraedQuad threadQuad)
    {
        int px = mrb.posX * 16;
        int py = mrb.posY * 16;


        if (/*thereBump && */asset3DCoat.CreateNormalMap)
        {

            if (thereNormalMap && cNormMap != null)
            {
                for (int ix = 0; ix < QuadSize; ix++)
                {
                    for (int iy = 0; iy < QuadSize; iy++)
                    {
                        Color norm = cNormMap[px + ix + (py + iy) * rtSizeX];

                        int nextX = (ix + px + 1) % rtSizeX;
                        int nextY = (iy + py + 1) % rtSizeY;

                        float nx = bumpBuf[nextX + (iy + py) * rtSizeX] - bumpBuf[(ix + px) + (iy + py) * rtSizeX];
                        float ny = bumpBuf[(ix + px) + nextY * rtSizeX] - bumpBuf[(ix + px) + (iy + py) * rtSizeX];

                        Vector3 n = new Vector3(-nx, ny, 1.0f / (asset3DCoat.NormalMapMultiply * 150)).normalized / 2.0f;
                        threadQuad.bumpMapQuad[ix + iy * QuadSize] = new Color(n.x + norm.r, n.y + norm.g, n.z + norm.b, n.x + norm.r);
                    }

                }

            }
            else
            {
                for (int ix = 0; ix < QuadSize; ix++)
                {
                    for (int iy = 0; iy < QuadSize; iy++)
                    {
                        int nextX = (ix + px + 1) % rtSizeX;
                        int nextY = (iy + py + 1) % rtSizeY;

                        float nx = bumpBuf[nextX + (iy + py) * rtSizeX] - bumpBuf[(ix + px) + (iy + py) * rtSizeX];
                        float ny = bumpBuf[(ix + px) + nextY * rtSizeX] - bumpBuf[(ix + px) + (iy + py) * rtSizeX];

                        Vector3 n = new Vector3(-nx, ny, 1.0f / (asset3DCoat.NormalMapMultiply * 150)).normalized / 2.0f;
                        threadQuad.bumpMapQuad[ix + iy * QuadSize] = new Color(n.x + 0.5f, n.y + 0.5f, n.z + 0.5f, n.x + 0.5f);
                    }

                }
            }

            if(Bump != null) Bump.SetPixels(px, py, QuadSize, QuadSize, threadQuad.bumpMapQuad);

        }


    }

    public void updateBlock(LayersBlock mrb, ThraedQuad threadQuad)
    {

        Asset3DCoat.metallicGlossOcQuadEmpty.CopyTo(threadQuad.metallicGlossOcQuad, 0);
        Asset3DCoat.bumpQuadEmpty.CopyTo(threadQuad.metallicQuad, 0);
        if (!asset3DCoat.MargeAOAndMetallic) Asset3DCoat.occlusionQuadEmpty.CopyTo(threadQuad.occlusionQuad, 0);
        if (asset3DCoat.ExtractCurvature) Asset3DCoat.occlusionQuadEmpty.CopyTo(threadQuad.curvatureQuad, 0);
        Asset3DCoat.occlusionQuadEmpty.CopyTo(threadQuad.objNormQuad, 0);
        //       Asset3DCoat.bumpQuadEmpty.CopyTo(bumpQuad, 0);
        Asset3DCoat.emissionQuadEmpty.CopyTo(threadQuad.emissionQuad, 0);
        Asset3DCoat.albedoQuadEmpty.CopyTo(threadQuad.albedoQuad, 0);

      

        threadQuad.ibAlbedo = false;
        threadQuad.ibEmission = false;
        threadQuad.ibCurvature = false;
        threadQuad.ibMetallicGlossOc = false;
        threadQuad.ibOcclusion = false;
        threadQuad.ibObjNormMap = false;

        threadQuad.mrb = mrb;
        foreach (LayersBlock.RawBlock rb in mrb.rawBlockLayers)
        {

            LayersBlock.RawBlock linkedBlock = null;
            linkedBlock = rb.linkedBlock;

            if (asset3DCoat.layersInfo[rb.LayerID].LinkedLayerId == -1 || asset3DCoat.layersInfo[rb.LayerID].InverseLinkage || (linkedBlock != null && linkedBlock.color != null))
            {

                OneLayerInfo iLayer = asset3DCoat.layersInfo[rb.LayerID];
                bool isOc = asset3DCoat.layersInfo[rb.LayerID].isOcclusion;
                bool isCv = asset3DCoat.layersInfo[rb.LayerID].isCurvature;
                bool lVisibled = asset3DCoat.layersInfo[rb.LayerID].VisibleInHierarchy;
                bool isObjNormMap = asset3DCoat.layersInfo[rb.LayerID].ColorOp == 26;
                if (isObjNormMap) thereBump = true;
                if (isObjNormMap) asset3DCoat.thereBump = true;
                if (rb.color != null && (lVisibled || isOc || isCv))
                {
                    threadQuad.ibAlbedo = true;
                    bool isEmission = (asset3DCoat.layersInfo[rb.LayerID].ColorOp == 28 || asset3DCoat.layersInfo[rb.LayerID].ColorOp == 29);
                    bool isEmissionGS = asset3DCoat.layersInfo[rb.LayerID].ColorOp == 29;
                    bool isColoredSpecular = asset3DCoat.layersInfo[rb.LayerID].ColorOp == 27;


                    int nlop = OneLayerInfo.LOP_list.Length;
                    int ColorOp = asset3DCoat.layersInfo[rb.LayerID].ColorOp;
                    OneLayerInfo.Op_Blend_Method tp = ColorOp < nlop ? OneLayerInfo.LOP_list[ColorOp] : null;

                    float T = asset3DCoat.layersInfo[rb.LayerID].ColorTransparency;
                    if (lVisibled && (!isOc || !asset3DCoat.HideAOInAlbedo) && !isEmission && !isEmission && !isEmissionGS && !isColoredSpecular && tp != null) tp(ref threadQuad.albedoQuad, ref rb.color, ref T);

                    if (isCv)
                    {
                        threadQuad.ibCurvature = true;
                        thereCurvature = true;
                        asset3DCoat.thereCurvature = true;

                        for (int iy = 0; iy < QuadSize; iy++)
                        {

                            int QYPos = iy * QuadSize;
                            for (int ix = 0; ix < QuadSize; ix++)
                            {

                                Color32 cl32 = rb.color[ix + iy * QuadSize];
                                float A = cl32.a / 255.0f;
                                threadQuad.curvatureQuad[ix + QYPos].r = threadQuad.curvatureQuad[ix + QYPos].r * (1.0f - A) + cl32.r * A / 255.0f;
                                threadQuad.curvatureQuad[ix + QYPos].g = threadQuad.curvatureQuad[ix + QYPos].g * (1.0f - A) + cl32.g * A / 255.0f;
                                threadQuad.curvatureQuad[ix + QYPos].b = threadQuad.curvatureQuad[ix + QYPos].b * (1.0f - A) + cl32.b * A / 255.0f;

                            }
                        }
                       
                    }

                    if (isObjNormMap)
                    {
                        threadQuad.ibObjNormMap = true;
                        thereNormalMap = true;
                        for (int iy = 0; iy < QuadSize; iy++)
                        {

                            int QYPos = iy * QuadSize;
                            for (int ix = 0; ix < QuadSize; ix++)
                            {

                                Color32 cl32 = rb.color[ix + iy * QuadSize];
                                float A = cl32.a / 255.0f;
                                threadQuad.objNormQuad[ix + QYPos].r = threadQuad.objNormQuad[ix + QYPos].r * (1.0f - A) + cl32.r * A / 255.0f;
                                threadQuad.objNormQuad[ix + QYPos].g = threadQuad.objNormQuad[ix + QYPos].g * (1.0f - A) + cl32.g * A / 255.0f;
                                threadQuad.objNormQuad[ix + QYPos].b = threadQuad.objNormQuad[ix + QYPos].b * (1.0f - A) + cl32.b * A / 255.0f;

                            }
                        }

                    }

                    if (isColoredSpecular && asset3DCoat.SpecularSetupMode)
                    {
                        threadQuad.ibMetallicGlossOc = true;

                          for (int iy = 0; iy < QuadSize; iy++)
                          {

                                    int QYPos = iy * QuadSize;
                                    for (int ix = 0; ix < QuadSize; ix++)
                                    {

                                        Color32 cl32 = rb.color[ix + iy * QuadSize];
                                        float A = (cl32.a / 255.0f)*iLayer.SpecularMod;
                                        threadQuad.metallicGlossOcQuad[ix + QYPos].r = threadQuad.metallicGlossOcQuad[ix + QYPos].r * (1.0f - A) + cl32.r * A / 255.0f;
                                        threadQuad.metallicGlossOcQuad[ix + QYPos].g = threadQuad.metallicGlossOcQuad[ix + QYPos].g * (1.0f - A) + cl32.g * A / 255.0f;
                                        threadQuad.metallicGlossOcQuad[ix + QYPos].b = threadQuad.metallicGlossOcQuad[ix + QYPos].b * (1.0f - A) + cl32.b * A / 255.0f;

                                    }
                          }


                    }

                    if (isEmission)
                    {
                        threadQuad.ibEmission = true;
                        thereEmission = true;
                        asset3DCoat.thereEmission = true;

                        if (asset3DCoat.ImportEmission)
                        {

                            if (isEmissionGS) {
                                for (int iy = 0; iy < QuadSize; iy++)
                                {

                                    int QYPos = iy * QuadSize;
                                    for (int ix = 0; ix < QuadSize; ix++)
                                    {
                                        Color32 cl32 = rb.color[ix + iy * QuadSize];
                                        float A = cl32.a / 255.0f;

                                        float e = (threadQuad.emissionQuad[ix + QYPos].r + threadQuad.emissionQuad[ix + QYPos].g + threadQuad.emissionQuad[ix + QYPos].b) / 3.0f;
                                        float e2 = (cl32.r / 255.0f + cl32.g / 255.0f + cl32.b / 255.0f)/3.0f;


                                        if (e2 > e)
                                        {
                                            if (e > 0.001f)
                                            {
                                                float ee = e2 / e;


                                                threadQuad.emissionQuad[ix + QYPos].r = threadQuad.emissionQuad[ix + QYPos].r * (1.0f - A) + threadQuad.emissionQuad[ix + QYPos].r * ee * A;
                                                threadQuad.emissionQuad[ix + QYPos].g = threadQuad.emissionQuad[ix + QYPos].g * (1.0f - A) + threadQuad.emissionQuad[ix + QYPos].g * ee * A;
                                                threadQuad.emissionQuad[ix + QYPos].b = threadQuad.emissionQuad[ix + QYPos].b * (1.0f - A) + threadQuad.emissionQuad[ix + QYPos].b * ee * A;
                                            }
                                            else
                                            {
                                                threadQuad.emissionQuad[ix + QYPos].r = threadQuad.emissionQuad[ix + QYPos].r * (1.0f - A) + e2 * A;
                                                threadQuad.emissionQuad[ix + QYPos].g = threadQuad.emissionQuad[ix + QYPos].g * (1.0f - A) + e2 * A;
                                                threadQuad.emissionQuad[ix + QYPos].b = threadQuad.emissionQuad[ix + QYPos].b * (1.0f - A) + e2 * A;
                                            }
                                        }

                                    }
                                }
                            } else
                            {
                                for (int iy = 0; iy < QuadSize; iy++)
                                {

                                    int QYPos = iy * QuadSize;
                                    for (int ix = 0; ix < QuadSize; ix++)
                                    {

                                        Color32 cl32 = rb.color[ix + iy * QuadSize];
                                        float A = cl32.a / 255.0f;
                                        threadQuad.emissionQuad[ix + QYPos].r = threadQuad.emissionQuad[ix + QYPos].r * (1.0f - A) + cl32.r * A / 255.0f;
                                        threadQuad.emissionQuad[ix + QYPos].g = threadQuad.emissionQuad[ix + QYPos].g * (1.0f - A) + cl32.g * A / 255.0f;
                                        threadQuad.emissionQuad[ix + QYPos].b = threadQuad.emissionQuad[ix + QYPos].b * (1.0f - A) + cl32.b * A / 255.0f;

                                    }
                                }
                            }

                        }

                    }


                    if (isOc)
                    {
                        threadQuad.ibOcclusion = true;
                        if (asset3DCoat.MargeAOAndMetallic) threadQuad.ibMetallicGlossOc = true;
                        thereAO = true;
                        asset3DCoat.thereAO = true;
                        if (asset3DCoat.ExtractAO)
                        {
                            if (asset3DCoat.MargeAOAndMetallic)
                            {
                                for (int iy = 0; iy < QuadSize; iy++)
                                {

                                    int QYPos = iy * QuadSize;
                                    for (int ix = 0; ix < QuadSize; ix++)
                                    {

                                        Color32 cl32 = rb.color[ix + iy * QuadSize];
                                        float A = cl32.a / 255.0f;
                                        threadQuad.metallicGlossOcQuad[ix + QYPos].g = threadQuad.metallicGlossOcQuad[ix + QYPos].g * (1.0f - A) + cl32.r * A / 255.0f;

                                    }
                                }
                            }
                            else
                            {
                                for (int iy = 0; iy < QuadSize; iy++)
                                {

                                    int QYPos = iy * QuadSize;
                                    for (int ix = 0; ix < QuadSize; ix++)
                                    {

                                        Color32 cl32 = rb.color[ix + iy * QuadSize];
                                        float A = cl32.a / 255.0f;
                                        float e = threadQuad.metallicGlossOcQuad[ix + QYPos].g * (1.0f - A) + cl32.r * A / 255.0f;
                                        threadQuad.occlusionQuad[ix + QYPos].r = e;
                                        threadQuad.occlusionQuad[ix + QYPos].g = e;
                                        threadQuad.occlusionQuad[ix + QYPos].b = e;

                                    }
                                }
                            }
                        }
                    }

                    /*
                    for (int iy = 0; iy < QuadSize; iy++)
                    {
                        
                        int QYPos = iy * QuadSize;
                        for (int ix = 0; ix < QuadSize; ix++)
                        {
                            Vector4 c1 = albedoQuad[ix + QYPos];
                            if (isEmission) c1 = emissionQuad[ix + QYPos];
                            Vector4 c1old = c1;
                            Color32 cl32 = rb.color[ix + iy * QuadSize];
                            if (isEmissionGS)
                            {
                                byte emgs = (byte)((cl32.r + cl32.g + cl32.b) / 3);
                                cl32 = new Color32(emgs, emgs, emgs, cl32.a);
                            }
                            Vector4 c2 = new Vector4(cl32.r, cl32.g, cl32.b, cl32.a);
                            float T = asset3DCoat.layersInfo[rb.LayerID].ColorTransparency;

                            float A = cl32.a / 255.0f;
                            if (linkedBlock != null && linkedBlock.color != null)
                            {
                                if (asset3DCoat.layersInfo[rb.LayerID].InverseLinkage)
                                    A = A * (1.0f - linkedBlock.color[ix + iy * QuadSize].a / 255.0f);
                                else A = A * linkedBlock.color[ix + iy * QuadSize].a / 255.0f;
                            }
                            A = A * asset3DCoat.layersInfo[rb.LayerID].ColorTransparency;

                            if (tp != null)
                            {
                                tp(ref c1, c2, ref T);
                            }
                            else
                            {
                                c1 = c2;
                            }

                            Vector4 result = c1old * (1.0f - A) + c1 * A;
                            result.w = Mathf.Min(1.0f, c1old.w + c1.w);

                            if (isOc) occlusionQuad[ix + QYPos] = occlusionQuad[ix + QYPos] * (1.0f - A) + cl32.r * A;
                            else if (isEmission) emissionQuad[ix + QYPos] = result;
                            else if (!isOc) albedoQuad[ix + QYPos] = result;


                        }

                    }
                     */

                }

                if (asset3DCoat.layersInfo[rb.LayerID].VisibleInHierarchy && rb.SpecMask != null)
                {
                    threadQuad.ibMetallicGlossOc = true;
                    thereSpecular = true;
                    asset3DCoat.thereSpecular = true;

                    if (asset3DCoat.SpecularSetupMode)
                    {
                        for (int iy = 0; iy < QuadSize; iy++)
                        {
                            int QYPos = iy * QuadSize;
                            for (int ix = 0; ix < QuadSize; ix++)
                            {
                                float a = rb.SpecMask[ix + QYPos] / 255.0f;

                                float aMt = Mathf.Max(0.0f, Mathf.Min(1.0f, a * iLayer.MetalnessOpacity));
                                float aSm = Mathf.Max(0.0f, Mathf.Min(1.0f, a * iLayer.SpecularMod));

                                float metE = rb.Metalness[ix + QYPos] * aMt / 255.0f;
                                Color specColor = threadQuad.albedoQuad[ix + QYPos] * metE + Color.white * (1.0f - metE) * (1.0f - aSm);
                //                albedoQuad[ix + QYPos] = albedoQuad[ix + QYPos] * (1.0f-metE);
                                threadQuad.metallicQuad[ix + QYPos] = threadQuad.metallicQuad[ix + QYPos] * (1.0f - aMt) + metE * aMt;

                                threadQuad.metallicGlossOcQuad[ix + QYPos].r = (threadQuad.metallicGlossOcQuad[ix + QYPos].r * (1.0f - aMt) + specColor.r * aMt);
                                threadQuad.metallicGlossOcQuad[ix + QYPos].g = (threadQuad.metallicGlossOcQuad[ix + QYPos].g * (1.0f - aMt) + specColor.g * aMt);
                                threadQuad.metallicGlossOcQuad[ix + QYPos].b = (threadQuad.metallicGlossOcQuad[ix + QYPos].b * (1.0f - aMt) + specColor.b * aMt);
                                threadQuad.metallicGlossOcQuad[ix + QYPos].a = (threadQuad.metallicGlossOcQuad[ix + QYPos].a * (1.0f - aSm) + rb.Specular[ix + QYPos] * aSm / 255.0f);

                            }

                        }
                    }
                    else
                    {
                        for (int iy = 0; iy < QuadSize; iy++)
                        {
                            int QYPos = iy * QuadSize;
                            for (int ix = 0; ix < QuadSize; ix++)
                            {
                                float a = rb.SpecMask[ix + QYPos] / 255.0f;

                                float aMt = Mathf.Max(0.0f, Mathf.Min(1.0f, a * iLayer.MetalnessOpacity));
                                float aSm = Mathf.Max(0.0f, Mathf.Min(1.0f, a * iLayer.SpecularMod));

                                threadQuad.metallicGlossOcQuad[ix + QYPos].r = (threadQuad.metallicGlossOcQuad[ix + QYPos].r * (1.0f - aMt) + rb.Metalness[ix + QYPos] * aMt / 255.0f);
                                threadQuad.metallicGlossOcQuad[ix + QYPos].a = (threadQuad.metallicGlossOcQuad[ix + QYPos].a * (1.0f - aSm) + rb.Specular[ix + QYPos] * aSm / 255.0f);
                                threadQuad.metallicGlossOcQuad[ix + QYPos].b = threadQuad.metallicGlossOcQuad[ix + QYPos].a;

                            }

                        }
                    }
                }
                
                if (asset3DCoat.layersInfo[rb.LayerID].VisibleInHierarchy && rb.bump != null)
                {
                    thereBump = true;
                    asset3DCoat.thereBump = true;

//                    else
                        if (asset3DCoat.CreateNormalMap || asset3DCoat.CreateHeightMap)
                        {
                            if (iLayer.DepthOp == 1)
                            {

                                for (int iy = 0; iy < QuadSize; iy++)
                                {
                                    int QYPos = iy * QuadSize;
                                    int MYPos = (mrb.posY * QuadSize + iy) * rtSizeX;
                                    for (int ix = 0; ix < QuadSize; ix++)
                                    {
                                        float a = asset3DCoat.layersInfo[rb.LayerID].DepthTransparency;
                                        bumpBuf[MYPos + ix + mrb.posX * QuadSize] -= rb.bump[ix + QYPos] * a;
                                    }

                                }
                            } else
                            if (iLayer.DepthOp == 2)
                            {

                                for (int iy = 0; iy < QuadSize; iy++)
                                {
                                    int QYPos = iy * QuadSize;
                                    int MYPos = (mrb.posY * QuadSize + iy) * rtSizeX;
                                    for (int ix = 0; ix < QuadSize; ix++)
                                    {
                                        float a = asset3DCoat.layersInfo[rb.LayerID].DepthTransparency;
                                        bumpBuf[MYPos + ix + mrb.posX * QuadSize] = bumpBuf[MYPos + ix + mrb.posX * QuadSize]*(1.0f-a) + Mathf.Max(bumpBuf[MYPos + ix + mrb.posX * QuadSize], rb.bump[ix + QYPos]) * a;
                                    }

                                }
                            } else
                            if (iLayer.DepthOp == 3)
                            {

                                for (int iy = 0; iy < QuadSize; iy++)
                                {
                                    int QYPos = iy * QuadSize;
                                    int MYPos = (mrb.posY * QuadSize + iy) * rtSizeX;
                                    for (int ix = 0; ix < QuadSize; ix++)
                                    {
                                        float a = asset3DCoat.layersInfo[rb.LayerID].DepthTransparency;
                                        bumpBuf[MYPos + ix + mrb.posX * QuadSize] +=  (rb.bump[ix + QYPos]-0.5f) * a;
                                    }

                                }
                            } else
                            if (iLayer.DepthOp == 4)
                            {

                                for (int iy = 0; iy < QuadSize; iy++)
                                {
                                    int QYPos = iy * QuadSize;
                                    int MYPos = (mrb.posY * QuadSize + iy) * rtSizeX;
                                    for (int ix = 0; ix < QuadSize; ix++)
                                    {
                                        float a = asset3DCoat.layersInfo[rb.LayerID].DepthTransparency;
                                        bumpBuf[MYPos + ix + mrb.posX * QuadSize] = bumpBuf[MYPos + ix + mrb.posX * QuadSize] * (1.0f - a) + rb.bump[ix + QYPos] * a;
                                    }

                                }
                            } else
                            {

                                for (int iy = 0; iy < QuadSize; iy++)
                                {
                                    int QYPos = iy * QuadSize;
                                    int MYPos = (mrb.posY * QuadSize + iy) * rtSizeX;
                                    for (int ix = 0; ix < QuadSize; ix++)
                                    {
                                        float a = asset3DCoat.layersInfo[rb.LayerID].DepthTransparency;
                                        bumpBuf[MYPos + ix + mrb.posX * QuadSize] += rb.bump[ix + QYPos] * a;
                                    }

                                }
                            }
                        }


                }
            }

        }

     //   for (int iy = 0; iy < QuadSize; iy++)
    //    {
   //         int LYPos = (iy + pY) * rtSizeX;
  //          int QYPos = iy * QuadSize;
   //         for (int ix = 0; ix < QuadSize; ix++)
  //          {
//                Vector4 vc = albedoQuad[QYPos + ix];
 //               Vector4 em = emissionQuad[QYPos + ix];
/*                albedo[QYPos + ix] = new Color32((byte)vc.x, (byte)vc.y, (byte)vc.z, (byte)vc.w);
                emission[QYPos + ix] = new Color32((byte)em.x, (byte)em.y, (byte)em.z, (byte)em.w);
                metallicGlossOc[QYPos + ix] = new Color32((byte)metalicQuad[ix + QYPos], (byte)Mathf.Min(255, Mathf.Max(0, occlusionQuad[ix + QYPos])), (byte)Mathf.Min(255, Mathf.Max(0, glossQuad[ix + QYPos])), (byte)Mathf.Min(255, Mathf.Max(0, glossQuad[ix + QYPos])));

                bump[QYPos + ix].g = bumpQuad[ix + QYPos];
                */
     //       }

//        }

        if (asset3DCoat.CreateHeightMap && asset3DCoat.MargeHMAndMetallic)
        {
            threadQuad.ibMetallicGlossOc = true;
            for (int iy = 0; iy < QuadSize; iy++)
            {
                int QYPos = iy * QuadSize;
                int MYPos = (mrb.posY * QuadSize + iy) * rtSizeX;
                for (int ix = 0; ix < QuadSize; ix++)
                {
                    threadQuad.metallicGlossOcQuad[QYPos + ix].g = bumpBuf[MYPos + ix + mrb.posX * QuadSize];
                }

            }

        }

        if (asset3DCoat.SpecularSetupMode && threadQuad.ibMetallicGlossOc)
        {
            for (int iy = 0; iy < QuadSize; iy++)
            {
                int QYPos = iy * QuadSize;
                for (int ix = 0; ix < QuadSize; ix++)
                {
                    float aa = threadQuad.albedoQuad[ix + QYPos].a;
                    threadQuad.albedoQuad[ix + QYPos] = threadQuad.albedoQuad[ix + QYPos] * (1.0f - threadQuad.metallicQuad[ix + QYPos]);
                    threadQuad.albedoQuad[ix + QYPos].a = aa;
                }

            }
        }


        threadQuad.finished = true;
        
    }

    public void ApplyThreadQuad(ThraedQuad threadQuad)
    {

        if (threadQuad.ibAlbedo) Albedo.SetPixels(threadQuad.mrb.posX * QuadSize, threadQuad.mrb.posY * QuadSize, QuadSize, QuadSize, threadQuad.albedoQuad);
		if ((threadQuad.ibEmission || Emission != null)  && asset3DCoat.ImportEmission) Emission.SetPixels(threadQuad.mrb.posX * QuadSize, threadQuad.mrb.posY * QuadSize, QuadSize, QuadSize, threadQuad.emissionQuad);
        if (threadQuad.ibCurvature && asset3DCoat.ExtractCurvature) Curvature.SetPixels(threadQuad.mrb.posX * QuadSize, threadQuad.mrb.posY * QuadSize, QuadSize, QuadSize, threadQuad.curvatureQuad);
        if (threadQuad.ibObjNormMap && asset3DCoat.CreateNormalMap) Bump.SetPixels(threadQuad.mrb.posX * QuadSize, threadQuad.mrb.posY * QuadSize, QuadSize, QuadSize, threadQuad.objNormQuad);
        //     Bump.SetPixels(mrb.posX * QuadSize, mrb.posY * QuadSize, QuadSize, QuadSize, bump);
        if (threadQuad.ibMetallicGlossOc) MetallicGlossOc.SetPixels(threadQuad.mrb.posX * QuadSize, threadQuad.mrb.posY * QuadSize, QuadSize, QuadSize, threadQuad.metallicGlossOcQuad);
        if (threadQuad.ibOcclusion && !asset3DCoat.MargeAOAndMetallic && Occlusion != null) Occlusion.SetPixels(threadQuad.mrb.posX * QuadSize, threadQuad.mrb.posY * QuadSize, QuadSize, QuadSize, threadQuad.occlusionQuad);

    } 

};


public class RTUVSet
{
	public string Name;
	public string TextureName;
	public int TexSizeX = 2048;
	public int TexSizeY = 2048;
};

public class OneSubObject
{
	public string Name;	
	public bool Visible;
	public bool Locked;
};


public struct VertexUV
{
	public uint PosIndex;
	public uint NIndex;        
	public float u;
	public float v;    
	public Vector3 T;
	public Vector3 B;	
	
	public float u0;
	public float v0;
	
	public Vector2 Pad;
	
	public Vector3 uv(){
		return new Vector3(u,v,0);
	}
	public Vector3 uv0(){
		return new Vector3(u0,v0,0);
	}
};

public struct VertexPos
{
	public Vector3 Pos;	
	public float    W;   
	public byte SelectionDegree;
	public byte SubdivLevel;
	public byte TempSelection;	
};



public struct tri_DWORD
{
	public uint V1,V2,V3;
	public tri_DWORD(uint v1,uint v2,uint v3){V1=v1;V2=v2;V3=v3;}
};


public struct SurfPoint3D
{
	public Vector3	Pos;
	public Vector3	N;
	public uint		Color;
	public uint		ColSpec;
	public float	Freeze;
	public float	Transp;
	public uint		spMetall;
	public Vector3	N0;   
//	uint GetCol(bool src);
//	uint GetSpecl(bool src);
};

public struct ShortPt2
{
	public Vector3 Pos;
	public Vector3 N;
	public short x,y;	
};



public class PlaneTriangulator
{
	List<Vector2> Points = new List<Vector2>();
	List<int> temp = new List<int>();
	bool inv;
	public List<Vector3> P3 = new List<Vector3>();
	public List<int> Result = new List<int>();

	public void AddPoint(Vector2 pt){
		Points.Add(pt);
	}
	public void AddPoint(Vector3 pt){
		P3.Add(pt);
	}
	public void Clear(){
		temp.Clear();
		Result.Clear();
		Points.Clear();
		P3.Clear();
	}

	bool CheckLinesIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 intrs, ref float t1, ref float t2){
		float a,b,c,d,e,f,det;
		a=p2.x-p1.x;b=p3.x-p4.x;
		c=p2.y-p1.y;d=p3.y-p4.y;
		e=p3.x-p1.x;f=p3.y-p1.y;
		det=a*d-b*c;
		if(det==0)return false;
		t1=(d*e-b*f)/det;
		if(t1<0.0||t1>0.9999999999)return false;
		t2=(a*f-c*e)/det;
		if(t2<0.0||t2>0.9999999999)return false;
		intrs=p3*t2+p4*(1-t2);
		return true;
	}

	Vector2 cp2(Vector2 r){
		return new Vector2(r.y,-r.x);
	}

	bool PointInTriangle(Vector2 p1,Vector2 p2,Vector2 p3,Vector2 p){
		float s1=Vector2.Dot(p-p1,cp2(p2-p1));
		float s2=Vector2.Dot(p-p2,cp2(p3-p2));
		float s3=Vector2.Dot(p-p3,cp2(p1-p3));
		return s1>0 && s2>0 && s3>0;
	}

	float GetAngle(int c,int p,int n){
		Vector2 dn=(Points[n]-Points[c]).normalized;
		Vector2 dp=(Points[p]-Points[c]).normalized;

		float _cos=Vector2.Dot(dp,dn);
		float _sin=dp.x*dn.y-dp.y*dn.x;
		if(inv)_sin*=-1;
		float ang=Mathf.Atan2(_sin,_cos);
		return ang>0 ? ang : ang+Mathf.PI*2.0f;
	}

	bool CheckPointInTriangle(int pt,int t1,int t2,int t3){
		return false;
	}

    public struct uniibdw
    {
		public int Key;
		public uniItem Value;
		public uniibdw(int aKey, uniItem aValue){
			Key = aKey;
			Value = aValue;
		}
		public uniibdw(int aKey, uint v1,  uint v2){
			Key = aKey;
			Value.Key = v1;
			Value.Value = v2;
		}
	}

	public void Triangulate(bool simplified = false){
		if(!(Points.Count > 2 || P3.Count > 2))return;
			Result.Clear();
			if(Points.Count == 3 || P3.Count == 3){
				Result.Add(0);
				Result.Add(1);
				Result.Add(2);
				return;
			}
//		try {
	/*				if(AppOpt.TriangulationMethod==1){
				//naive
				for(int k=2;k<P3.Count;k++){
					Result.Add(0);
					Result.Add(k-1);
					Result.Add(k);
				}
				return;
			}*/
			if(P3.Count == 4 && simplified){
				float L1=Vector3.Distance(P3[0], P3[2]);
				float L2=Vector3.Distance(P3[1], P3[3]);
				if(simplified){
					if(L1<L2 || Mathf.Abs(L1-L2)/(L1+L2)<0.01){
						Result.Add(0);
						Result.Add(1);
						Result.Add(2);
						Result.Add(0);
						Result.Add(2);
						Result.Add(3);
					}else{
						Result.Add(0);
						Result.Add(1);
						Result.Add(3);
						Result.Add(3);
						Result.Add(1);
						Result.Add(2);
					}
					return;
				}
			}
//		return;
			Vector3 nr = Vector3.zero;
			if(Points.Count == 0 && P3.Count > 0){
				int n3=P3.Count;
				Vector3 c = Vector3.zero;
				for(int i=0;i<n3;i++){
					int p=(i+n3-1)%n3;
					int n=(i+1)%n3;
					nr+=Vector3.Cross(P3[p]-P3[i],P3[n]-P3[i]);
					c+=P3[i];
				}
				c/=n3;
				nr.Normalize();
				Vector3 ax=Vector3.Cross(nr,Vector3.right);
				if(Vector3.Distance(Vector3.zero, ax)<0.1){
					ax=Vector3.Cross(nr,Vector3.up);
				}
				ax.Normalize();
				Vector3 ay=Vector3.Cross(nr,ax);
				ay.Normalize();
				for(int i=0;i<n3;i++){
					Vector2 v = new Vector2(Vector3.Dot(ax,P3[i]-c), Vector3.Dot(ay, P3[i]));
					Points.Add(v);
				}
			}
			float s=0;

			int npc2=Points.Count;
	//		temp.Count();
			for(int i=0;i<npc2;i++){
				int inx=(i+1)%npc2;
				s+=(Points[inx].x-Points[i].x)*(Points[inx].y+Points[i].y);
				temp.Add(i);
			}
			inv=s<0;
			if(Points.Count==4){
				float a1=GetAngle(0,3,1)+GetAngle(2,1,3);
				float a2=GetAngle(1,0,2)+GetAngle(3,2,0);
				if(a1<=a2*1.001){
					Result.Add(0);
					Result.Add(1);
					Result.Add(3);
					
					Result.Add(1);
					Result.Add(2);
					Result.Add(3);
				}else{
					Result.Add(0);
					Result.Add(1);
					Result.Add(2);
					
					Result.Add(2);
					Result.Add(3);
					Result.Add(0);
				}
				return;
			}
			do{
				int nt=temp.Count;
				if(nt==3){
					Result.Add(temp[0]);
					Result.Add(temp[1]);
					Result.Add(temp[2]);
					temp.Clear();
				}else{
					int ac=-1;
					float maxminang=1000;
					for(int i=0;i<nt;i++){
						int p=temp[(i+nt-1)%nt];
						int n2=temp[(i+1)%nt];
						int c=temp[i];
						float a1=GetAngle(c,p,n2);
						if(a1>0 && a1<Mathf.PI){
							if(a1<maxminang){
								//checking if no intersections
								Vector2 pp=Points[p];
								Vector2 pn=Points[n2];
								Vector2 pc=Points[c];
								bool fail=false;
								for(int k=0;k<nt;k++){
									int p1=temp[k];
									int p2=temp[(k+1)%nt];
									if(p1!=c && p1!=n2 && p1!=p){
										if(p2!=c && p2!=n2 && p2!=p){
											Vector2 tr = new Vector2();
											float t1 = 0.0f;
											float t2 = 0.0f;
											if(CheckLinesIntersection(pp, pn,Points[p1],Points[p2],ref tr, ref t1, ref t2)){
												fail=true;
												break;
											}
										}
										//check if point in triangle
										if(PointInTriangle(pc,pn,pp,Points[p1])){
											fail=true;
											break;
										}								
									}
								}
								if(!fail){
									maxminang=a1;
									ac=i;
								}
							}
						}
					}
					if(ac!=-1){
						Result.Add(temp[(ac+nt-1)%nt]);
						Result.Add(temp[ac]);
						Result.Add(temp[(ac+1)%nt]);
						temp.RemoveAt(ac);
					}else{
						temp.Clear();
					}
				}
			}while(temp.Count > 0);
			//delone improvement
			List<uniibdw> fone = new List<uniibdw>();
			for(int i=0;i<Result.Count;i+=3){
				int v1=Result[i  ];
				int v2=Result[i+1];
				int v3=Result[i+2];
				int i2=i+2;
				fone.Add(new uniibdw(i2, (uint)v1,(uint)v2));
				fone.Add(new uniibdw(i, (uint)v2,(uint)v3));
				i2=i+1;
				fone.Add(new uniibdw(i2, (uint)v3,(uint)v1));
			}

			bool chn;
			int ppp=0;
			do{
				chn=false;
				int fui = -1;
				while(fui < fone.Count-1){
					fui++;
					uniibdw uit = fone[fui];
					uniItem b = uit.Value;
					int v1f=(int)b.Key;
					int v2n=(v1f+1)%npc2;


					int pf = uit.Key;

					if(b.Value!=v2n){
						uniItem bo = new uniItem(b.Value,b.Key);

						int op = -1;
						foreach(uniibdw itm in fone) if(itm.Value.Key == bo.Key && itm.Value.Value == bo.Value) op = itm.Key;

						if(op > -1){
							int op1=Result[pf];
							int op2=Result[op];
							float a1=GetAngle(op1,(int)b.Value,(int)b.Key);
							float a2=GetAngle(op2,(int)bo.Value,(int)bo.Key);
							a1+=a2;


							if(a1>Mathf.PI){//flip
								int[] v1 = new int[3];
								v1[0] = (int)b.Key;
								v1[1] = (int)b.Value;
								v1[2] = op1;
								int[] v2 = new int[3];
								v2[0] = (int)bo.Key;
								v2[1] = (int)bo.Value;
								v2[2] = op2;

								for(int k=0;k<3;k++){
									for(int fi = fone.Count-1; fi >= 0; fi--){
										if((fone[fi].Value.Key == v1[k] && fone[fi].Value.Value == v1[(k+1)%3])
										   || (fone[fi].Value.Key ==  v2[k] && fone[fi].Value.Value == v2[(k+1)%3])){
											fone.RemoveAt(fi);
//											if(fi <= fui) fui--;

										}
									}
								}
								int f0=(pf/3)*3;
								int f1=(op/3)*3;
								v1[1]=op2;
								v2[1]=op1;
								for(int k=0;k<3;k++){
									Result[f0+k]=v1[k];
									Result[f1+k]=v2[k];
									int e=f0+((k+2)%3);
									fone.Add(new uniibdw(e, (uint)v1[k],(uint)v1[(k+1)%3]));
									e=f1+((k+2)%3);
									fone.Add(new uniibdw(e, (uint)v2[k],(uint)v2[(k+1)%3]));
								}
								chn=true;
							}
						}
					}
				}
			}while(chn && ppp++<5);
//		} catch(IOException e){

//			if (e.Source != null)
//				Console.WriteLine("IOException source: {0}", e.Source);
//		}
	}

};


public class FaceInfo
{
	public FaceInfo(){
		ChangeStamp=0;
		TempFlag=0;
//		Points=null;
//		Backup=null;
		ComplexityLevel=0;
		CentralU=128;
		CentralV=128;
		FreezeState=0;
		Editable=true;
		RTIdx=0;
		DencityMod=1;
//		PlanePts=null;
	}
	public byte		SubdivLevel;
	public byte		UVEdgeMask;//1-L 2-T 3-R 4-B 
	public byte		FreezeState;//0-not freezed,1-mixed,2-fully freezed
	public uint       UpFace;
	public uint       DnFace;
	public uint       LFace;
	public uint       RFace;    
	public ushort		MtlID;
	public ushort		ObjID;
	public byte        TempFlag;
	public short		RTIdx;
	public short		uvIdx;
	public short       SubSizeX;
	public short       SubSizeY;
	public float       ChangeStamp;
	public bool		Visible;
	public bool		Editable;
	public byte		ComplexityLevel;
	public byte		CentralU;
	public byte		CentralV;
	public byte		DencityMod;
	public tri_DWORD   CellPos;
    
};



[Serializable]
public class Asset3DCoat : ScriptableObject
{


    [SerializeField]
    public int[] customLayersVisible = new int[0];
    [SerializeField]
    public bool[] customLayersVisibleValues = new bool[0];


    public string AssetPath;
    public string Vers3B;
    public int vCode = 0;
    public string AssetPrefabPath = "";
    public string getAssetPath()
    { 
        string pPath = AssetPath;
        if (!File.Exists(pPath)) pPath = AssetDatabase.GetAssetPath(this).Replace(".prefab", "");
         return pPath;  
    }

    
    private List<UnityEngine.Object> KeepAssets = new List<UnityEngine.Object>();

    public GameObject prefab;
    
    public List<GameObject> objects = new List<GameObject>();
    
    public UnityEngine.Object[] AssetObjects;


    public string unPackFolder = "";
    public int upOverrideFiles = 0;
    public int upOverrideFolders = 0;

    public bool thereTransparent = false;
    public bool thereAO = false;
    public bool thereCurvature = false;
    public bool thereEmission = false;
    public bool thereBump = false;
    public bool thereSpecular = false;

	// Glen Changed this to false
    public bool InteractiveUpdate = false;

    
    [SerializeField]
    bool _TrianglesWithAlpha = true;
    public bool TrianglesWithAlpha { get { return _TrianglesWithAlpha; } set { if (_TrianglesWithAlpha != value) { _TrianglesWithAlpha = value; if (InteractiveUpdate)updateAsset(); } } }

    [SerializeField]
    bool _joinObjects = true;
    public bool JoinObjects { get { return _joinObjects; } set { if (_joinObjects != value) { _joinObjects = value; if (InteractiveUpdate)updateAsset(); } } }

	[SerializeField]
	bool _ungroupMeshes = false;
	public bool UngroupMeshes { get { return _ungroupMeshes; } set { if (_ungroupMeshes != value) { _ungroupMeshes = value; if (InteractiveUpdate)updateAsset(); } } }

    [SerializeField]
    bool _SpecularSetupMode = false;
	public bool SpecularSetupMode { get { return _SpecularSetupMode; } set { if (_SpecularSetupMode != value) { _SpecularSetupMode = value; if (InteractiveUpdate)updateAsset(); } } }

    [SerializeField]
    bool _extractAO = true;
	public bool ExtractAO { get { return _extractAO; } set { if (_extractAO != value) { _extractAO = value; if (InteractiveUpdate)updateAsset(); } } }

    [SerializeField]
    bool _extractCurvature = false;
	public bool ExtractCurvature { get { return _extractCurvature; } set { if (_extractCurvature != value) { _extractCurvature = value; if (InteractiveUpdate)updateAsset(); } } }

    [SerializeField]
    bool _ImportEmission = true;
	public bool ImportEmission { get { return _ImportEmission; } set { if (_ImportEmission != value) { _ImportEmission = value; if (InteractiveUpdate)updateAsset(); } } }

    [SerializeField]
    bool _HideAOInAlbedo = true;
	public bool HideAOInAlbedo { get { return _HideAOInAlbedo; } set { if (_HideAOInAlbedo != value) { _HideAOInAlbedo = value; if (InteractiveUpdate)updateAsset(); } } }

    [SerializeField]
    bool _CreateNormalMap = true;
	public bool CreateNormalMap { get { return _CreateNormalMap; } set { if (_CreateNormalMap != value) { _CreateNormalMap = value; if (InteractiveUpdate)updateAsset(); } } }

    [SerializeField]
    bool _CreateHeightMap = false;
	public bool CreateHeightMap { get { return _CreateHeightMap; } set { if (_CreateHeightMap != value) { _CreateHeightMap = value; if (InteractiveUpdate)updateAsset(); } } }

    [SerializeField]
    bool _MargeAOAndMetallic = true;
	public bool MargeAOAndMetallic { get { return _MargeAOAndMetallic; } set { if (_MargeAOAndMetallic != value) { _MargeAOAndMetallic = value; if (InteractiveUpdate)updateAsset(); } } }

    [SerializeField]
    bool _MargeHMAndMetallic = false;
	public bool MargeHMAndMetallic { get { return _MargeHMAndMetallic; } set { if (_MargeHMAndMetallic != value) { _MargeHMAndMetallic = value; if (InteractiveUpdate)updateAsset(); } } }

    [SerializeField]
    float _NormalMapMultiply = 1.0f;
	public float NormalMapMultiply { get { return _NormalMapMultiply; } set { if (_NormalMapMultiply != value) { _NormalMapMultiply = value; if (InteractiveUpdate)updateAsset(); } } }

    [SerializeField]
    float _HeightMapMultiply = 1.0f;
	public float HeightMapMultiply { get { return _HeightMapMultiply; } set { if (_HeightMapMultiply != value) { _HeightMapMultiply = value; if (InteractiveUpdate)updateAsset(); } } }

	[SerializeField]
	bool _CreatePadding = true;
	public bool CreatePadding { get { return _CreatePadding; } set { if (_CreatePadding != value) { _CreatePadding = value; if (InteractiveUpdate)updateAsset(); } } }

//    public OneUVSet oneUVSet = new OneUVSet();
	public Camera camera = new Camera();
	public Vector3 camCenter;

    public Vector3[] pos0;

    public Dictionary<int,OneLayerInfo> layersInfo = new Dictionary<int, OneLayerInfo>();

    public MRenderTarget[] RTS;
    
//    public List<RTUVSet> UVSets = new List<RTUVSet>();

    public OneSubObject[] SubObjects;


    public VertexUV[] VertsUV;
    public Vector3[] VertsN;
    public VertexPos[] Verts;

	public int[,] LinkedVerts; // buf for split subobjects
	public int[] LinkedVertsCnt; // buf for split subobjects
	public int[] VertsSubObjId; // buf for split subobjects
	public int UngroupObjCnt;

    public uniItem[] Faces;
    public uniItem[] SrcFaces;

	public int rtSizeX;
	public int rtSizeY;



	public float CellSize;
	public float Scale;
	public Vector3 StartShift;
	public float MeasureScale;
	public float UnitsScale;

    static public Color[] metallicGlossOcQuadEmpty = new Color[QuadSq];
    static public Color[] normalMapQuadEmpty = new Color[QuadSq];
    static public float[] bumpQuadEmpty = new float[QuadSq];
    static public Color[] occlusionQuadEmpty = new Color[QuadSq];
    static public Color[] emissionQuadEmpty = new Color[QuadSq];
    static public Color[] albedoQuadEmpty = new Color[QuadSq];



    public FaceInfo[] FacesInfo;

    

    public bool firstMapsDraw = false;

	public Asset3DCoat()
	{
		//
	}


	public void AddLinkedVert (int vertid, int targetVert){
		if (LinkedVertsCnt [vertid] > 9)
			return;

		for (int i = 0; i < LinkedVertsCnt [vertid]; i++)
			if (LinkedVerts [vertid,i] == targetVert)
				return;

		LinkedVerts [vertid, LinkedVertsCnt [vertid]] = targetVert;
		LinkedVertsCnt [vertid]++;
	}

	void findLinkedGroupByVert(int vertId){
		VertsSubObjId [vertId] = UngroupObjCnt;
		for (int i = 0; i < LinkedVertsCnt [vertId]; i++) {
			if (VertsSubObjId [LinkedVerts [vertId, i]] < 0) {
				findLinkedGroupByVert (LinkedVerts [vertId, i]);
			}
		}

	}

	public void findSubObjectsInMesh(){
		UngroupObjCnt = 0;
		for (int i = 0; i < Verts.Length; i++) {
			if (VertsSubObjId[i] < 0) {
				findLinkedGroupByVert (i);
				UngroupObjCnt++;
			}

		}
//		Debug.Log ("SubObjects"+UngroupObjCnt);
	}


    public void assignDataFrom(Asset3DCoat srcAsset)
    {
        Vers3B = srcAsset.Vers3B;
        vCode = srcAsset.vCode;
        objects = srcAsset.objects;
        AssetObjects = srcAsset.AssetObjects;

        thereAO = srcAsset.thereAO;
        thereCurvature = srcAsset.thereCurvature;
        thereEmission = srcAsset.thereEmission;
        thereBump = srcAsset.thereBump;
        thereSpecular = srcAsset.thereSpecular;

//        oneUVSet = srcAsset.oneUVSet;
        pos0 = srcAsset.pos0;
        layersInfo.Clear();
        foreach (var voli in srcAsset.layersInfo)
        {
            OneLayerInfo oli = new OneLayerInfo();

	        oli.Name             = voli.Value.Name;
	        oli.LayerID          = voli.Value.LayerID;
	        oli.Visible          = voli.Value.Visible;
	        oli.IsFolder         = voli.Value.IsFolder;
	        oli.EndOfSection     = voli.Value.EndOfSection; 
	        oli.IsOpen           = voli.Value.IsOpen;
	        oli.LockTransparency = voli.Value.LockTransparency;
	        oli.UseAsWeightmap   = voli.Value.UseAsWeightmap;
	        oli.ColorOp          = voli.Value.ColorOp;
	        oli.DepthOp          = voli.Value.DepthOp;
	        oli.EmbossPower      = voli.Value.EmbossPower;
	        oli.DepthTransparency= voli.Value.DepthTransparency;
	        oli.ColorTransparency= voli.Value.ColorTransparency;
	        oli.Contrast         = voli.Value.Contrast;
	        oli.Brightness       = voli.Value.Brightness;
	        oli.SpecContrast     = voli.Value.SpecContrast;
	        oli.SpecularMod      = voli.Value.SpecularMod;
	        oli.SpecBrightness   = voli.Value.SpecBrightness;
	        oli.GlossMod         = voli.Value.GlossMod;
	        oli.RoughMod         = voli.Value.RoughMod;
	        oli.MetalnessOpacity = voli.Value.MetalnessOpacity;
	        oli.MetalBrightness  = voli.Value.MetalBrightness;
	        oli.LinkedLayer      = voli.Value.LinkedLayer;
	        oli.InverseLinkage   = voli.Value.InverseLinkage;
	        oli.HiddenMaskOwner  = voli.Value.HiddenMaskOwner;

            layersInfo.Add(voli.Key, oli);

        }

        RTS = new MRenderTarget[srcAsset.RTS.Length];
        int RTSi = 0;
        foreach (MRenderTarget mrt in srcAsset.RTS)
        {
            MRenderTarget nMrt = new MRenderTarget();
    	    nMrt.Name = mrt.Name;
            nMrt.asset3DCoat = this;
	        nMrt.rtSizeX            = mrt.rtSizeX;
	        nMrt.rtSizeY            = mrt.rtSizeY;
	        nMrt.CTextureID         = mrt.CTextureID; 
	        nMrt.NTextureID         = mrt.NTextureID; 
	        nMrt.STextureID         = mrt.STextureID; 
	        nMrt.SubPatchLevel      = mrt.SubPatchLevel;
	        nMrt.DrawMicrovertices  = mrt.DrawMicrovertices;
	        nMrt.SkipNormalmap      = mrt.SkipNormalmap;
            nMrt.UseExternalTexture = mrt.UseExternalTexture;
            nMrt.Lx = mrt.Lx;
            nMrt.Ly = mrt.Ly;
            nMrt.MatID = mrt.MatID;
            nMrt.rawLayersBlocks = mrt.rawLayersBlocks;
            RTS[RTSi] = nMrt;
            RTSi++;

        }

  //      UVSets = srcAsset.UVSets;
        SubObjects = srcAsset.SubObjects;
        VertsUV = srcAsset.VertsUV;
        VertsN = srcAsset.VertsN;
        Verts = srcAsset.Verts;
        Faces = srcAsset.Faces;
        SrcFaces = srcAsset.SrcFaces;
        FacesInfo = srcAsset.FacesInfo;


        firstMapsDraw = false;
        restoreCustomLayersVisible();
    }

	static uint MakeMagic(string aStr) {
		byte[] Magic = System.Text.Encoding.ASCII.GetBytes (aStr);
		int l = Magic.Length;
		int i;
		uint c, M = 0;
		for(i = l - 1; i >= 0; i--) {
			c = Magic[i];
			M += c << (8 * (3 - i));
		}
		return M;
	}

    static public List<Asset3DCoat> findLinkedAssets(string aLinkedPath)
    {

        List<Asset3DCoat> result = new List<Asset3DCoat>();

        EditorUtility.DisplayProgressBar("findLinkedAssets", "GetGetAllAssetPaths", 0.0f);

        string[] pAssets = AssetDatabase.GetAllAssetPaths();

        for (int i = 0; i < pAssets.Length; i++)
        {
            string sPath = pAssets[i];
            if (i % 100 == 0) EditorUtility.DisplayProgressBar("findLinkedAssets", "findLinkedAssets", (float)i / (float)pAssets.Length);

            if (Path.GetExtension(sPath) == ".prefab")
            {

                Asset3DCoat asset = AssetDatabase.LoadAssetAtPath<Asset3DCoat>(sPath);

                if (asset != null && asset.AssetPath == aLinkedPath) result.Add(asset);

            }
        }

        EditorUtility.ClearProgressBar();
        return result;
    }


    public List<Asset3DCoat> findLinkedAssets()
    {

        List<Asset3DCoat> result = new List<Asset3DCoat>();

        EditorUtility.DisplayProgressBar("findLinkedAssets", "GetGetAllAssetPaths", 0.0f);

        string[] pAssets = AssetDatabase.GetAllAssetPaths();

        for (int i = 0; i < pAssets.Length; i++)
        {
            string sPath = pAssets[i];
            if(i % 100 == 0)EditorUtility.DisplayProgressBar("findLinkedAssets", "findLinkedAssets", (float)i / (float)pAssets.Length);

            if (Path.GetExtension(sPath) == ".prefab")
            {

                Asset3DCoat asset = AssetDatabase.LoadAssetAtPath<Asset3DCoat>(sPath);

                if (asset != null && asset != this && asset.AssetPath == AssetPath) result.Add(asset);

            }
        }

        EditorUtility.ClearProgressBar();
        return result;
    }

    public void restoreCustomLayersVisible()
    {

        for (int i = 0; i < customLayersVisible.Length; i++)
        {
            if (layersInfo.ContainsKey(customLayersVisible[i]))
            {
                layersInfo[customLayersVisible[i]].useCustomVisible = true;
                layersInfo[customLayersVisible[i]].CustomVisible = customLayersVisibleValues[i];

            }

        }

    }


    public static void SetupMaterialWithBlendMode(Material material, int blendMode)
    {
        switch (blendMode)
        {
            case 0:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                break;
            case 1:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 2450;
                break;
            case 2:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
            case 3:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
        }
    }


	public void makeMaterials(){
        restoreCustomLayersVisible();


        thereAO = false;
        thereCurvature = false;
        thereEmission = false;
        thereBump = false;
        thereSpecular = false;

		foreach (var oli in layersInfo) {
			oli.Value.LinkedLayerId = -1;
			foreach (var oli2 in layersInfo)
				if(oli.Value.LinkedLayer == oli2.Value.Name) oli.Value.LinkedLayerId = oli2.Value.LayerID;

            oli.Value.isOcclusion = oli.Value.Name.Contains("Occlusion");
            oli.Value.isCurvature = oli.Value.Name.Contains("Curvature");

		}



        for (int i = 0; i < QuadSq; i++)
        {
            normalMapQuadEmpty[i] = new Color(0.5f, 0.5f, 1.0f, 0.5f);
            if (SpecularSetupMode) metallicGlossOcQuadEmpty[i] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            if (!SpecularSetupMode) metallicGlossOcQuadEmpty[i] = new Color(0.0f, 1.0f, 1.0f, 0.0f);
            occlusionQuadEmpty[i] = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            bumpQuadEmpty[i] = 0;
            emissionQuadEmpty[i] = new Color(0.0f, 0.0f, 0.0f, 1.0f);
            albedoQuadEmpty[i] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }


		int mrtid = -1;		
		foreach (MRenderTarget mrt in RTS) {
            mrt.asset3DCoat = this;

            mrt.thereAO = false;
            mrt.thereBump = false;
            mrt.thereCurvature = false;
            mrt.thereEmission = false;
            mrt.thereSpecular = false;

            mrtid++;


            int QuadsX = (mrt.rtSizeX / QuadSize);
            int QuadsY = (mrt.rtSizeY / QuadSize);
            mrt.QuadBlocksMatrix = new MRenderTarget.LayersBlock[QuadsX * QuadsY];

            foreach (MRenderTarget.LayersBlock lb in mrt.rawLayersBlocks)
            {
                mrt.QuadBlocksMatrix[lb.posY * QuadsX + lb.posX] = lb;

            }

            mrt.bumpBuf = new float[mrt.rtSizeX * mrt.rtSizeY];
//            mrt.bumpMap = new Color32[mrt.rtSizeX * mrt.rtSizeY];

            mrt.Albedo = assetObject<Texture2D>(mrt.Name + "Albedo");
            if (mrt.Albedo == null)
            {
                mrt.Albedo = new Texture2D(mrt.rtSizeX, mrt.rtSizeY, TextureFormat.RGBA32, true);
                mrt.Albedo.name = mrt.Name + "Albedo";
                AssetDatabase.AddObjectToAsset(mrt.Albedo, prefab);
            }
            mrt.Albedo.Resize(mrt.rtSizeX, mrt.rtSizeY);

            mrt.MetallicGlossOc = assetObject<Texture2D>(mrt.Name + "MGO");
            if (mrt.MetallicGlossOc == null)
            {
                mrt.MetallicGlossOc = new Texture2D(mrt.rtSizeX, mrt.rtSizeY, TextureFormat.RGBA32, true);
                mrt.MetallicGlossOc.name = mrt.Name + "MGO";
                AssetDatabase.AddObjectToAsset(mrt.MetallicGlossOc, prefab);
            }
            mrt.MetallicGlossOc.Resize(mrt.rtSizeX, mrt.rtSizeY);

            mrt.Bump = assetObject<Texture2D>(mrt.Name + "Bump");
            if (mrt.Bump == null)
            {
                mrt.Bump = new Texture2D(mrt.rtSizeX, mrt.rtSizeY, TextureFormat.RGBA32, true);
                mrt.Bump.name = mrt.Name + "Bump";
                AssetDatabase.AddObjectToAsset(mrt.Bump, prefab);
            }
            mrt.Bump.Resize(mrt.rtSizeX, mrt.rtSizeY);

            mrt.Emission = assetObject<Texture2D>(mrt.Name + "Emission");
            if (mrt.Emission == null)
            {
                mrt.Emission = new Texture2D(mrt.rtSizeX, mrt.rtSizeY, TextureFormat.RGBA32, true);
                mrt.Emission.name = mrt.Name + "Emission";
                AssetDatabase.AddObjectToAsset(mrt.Emission, prefab);
            }
            mrt.Emission.Resize(mrt.rtSizeX, mrt.rtSizeY);

            for (int ix = 0; ix < mrt.rtSizeX / QuadSize; ix++)
            {
                for (int iy = 0; iy < mrt.rtSizeY / QuadSize; iy++)
                {
                    mrt.MetallicGlossOc.SetPixels(ix * QuadSize, iy * QuadSize, QuadSize, QuadSize, metallicGlossOcQuadEmpty);
                    mrt.Albedo.SetPixels(ix * QuadSize, iy * QuadSize, QuadSize, QuadSize, albedoQuadEmpty);
                    mrt.Emission.SetPixels(ix * QuadSize, iy * QuadSize, QuadSize, QuadSize, emissionQuadEmpty);
                }
            }

            if (!MargeAOAndMetallic)
            {
                mrt.Occlusion = assetObject<Texture2D>(mrt.Name + "Occlusion");
                if (mrt.Occlusion == null)
                {
                    mrt.Occlusion = new Texture2D(mrt.rtSizeX, mrt.rtSizeY, TextureFormat.RGBA32, true);
                    mrt.Occlusion.name = mrt.Name + "Occlusion";
                    AssetDatabase.AddObjectToAsset(mrt.Occlusion, prefab);
                }
                mrt.Occlusion.Resize(mrt.rtSizeX, mrt.rtSizeY);
            }

            if (ExtractCurvature)
            {
                mrt.Curvature = assetObject<Texture2D>(mrt.Name + "Curvature");
                if (mrt.Curvature == null)
                {
                    mrt.Curvature = new Texture2D(mrt.rtSizeX, mrt.rtSizeY, TextureFormat.RGBA32, true);
                    mrt.Curvature.name = mrt.Name + "Curvature";
                    AssetDatabase.AddObjectToAsset(mrt.Curvature, prefab);
                }
                mrt.Curvature.Resize(mrt.rtSizeX, mrt.rtSizeY);
            }


            mrt.material = assetObject<Material>(mrt.Name);
            if (mrt.material == null)
            {
                mrt.material = new Material(Shader.Find("Standard"));
                mrt.material.name = mrt.Name;
//                AssetDatabase.AddObjectToAsset(mrt.material, prefab);

            }



            if (!SpecularSetupMode && mrt.material.shader.name == "Standard (Specular setup)")
            {
                mrt.material.shader = Shader.Find("Standard");

            }

            if (SpecularSetupMode && mrt.material.shader.name == "Standard")
            {
                mrt.material.shader = Shader.Find("Standard (Specular setup)");

            }

            for (int ix = 0; ix < mrt.rtSizeX / QuadSize; ix++){
                for (int iy = 0; iy < mrt.rtSizeY / QuadSize; iy++)
                {
                    mrt.Bump.SetPixels(ix * QuadSize, iy * QuadSize, QuadSize, QuadSize, normalMapQuadEmpty);
                    mrt.Albedo.SetPixels(ix * QuadSize, iy * QuadSize, QuadSize, QuadSize, albedoQuadEmpty);
                    mrt.MetallicGlossOc.SetPixels(ix * QuadSize, iy * QuadSize, QuadSize, QuadSize, metallicGlossOcQuadEmpty);


                }
            }






                EditorUtility.DisplayProgressBar("Make Materials", "Find linked blocks", 0.0f);
            /// Find linked blocks
                foreach (MRenderTarget.LayersBlock mrb in mrt.rawLayersBlocks)
                    {
                        foreach (MRenderTarget.LayersBlock.RawBlock rb in mrb.rawBlockLayers)
                        {

                            if (layersInfo[rb.LayerID].LinkedLayerId > -1)
                            {
                                int iLinkedLayerId = layersInfo[rb.LayerID].LinkedLayerId;
                                foreach (MRenderTarget.LayersBlock.RawBlock rbll in mrb.rawBlockLayers)
                                {
                                    if (rbll.LayerID == iLinkedLayerId)
                                    {
                                        rb.linkedBlock = rbll;
                                    }
                                }
/////
                                if (firstMapsDraw)
                                {
                                    if (!layersInfo[rb.LayerID].InverseLinkage)
                                    {
                                        if (rb.linkedBlock == null || rb.linkedBlock.color == null)
                                        {
                                            if (rb.color != null) for (int bi = 0; bi < QuadSq; bi++) rb.color[bi].a = 0;
                                            if (rb.bump != null) for (int bi = 0; bi < QuadSq; bi++) rb.bump[bi] = 0;
                                            if (rb.SpecMask != null) for (int bi = 0; bi < QuadSq; bi++) rb.SpecMask[bi] = 0;
                                        }
                                        else
                                        {
                                            if (rb.color != null) for (int bi = 0; bi < QuadSq; bi++) rb.color[bi].a = (byte)(rb.color[bi].a * rb.linkedBlock.color[bi].a / 255);
                                            if (rb.bump != null) for (int bi = 0; bi < QuadSq; bi++) rb.bump[bi] = (rb.bump[bi] * rb.linkedBlock.color[bi].a / 255.0f);
                                            if (rb.SpecMask != null) for (int bi = 0; bi < QuadSq; bi++) rb.SpecMask[bi] = (byte)(rb.SpecMask[bi] * rb.linkedBlock.color[bi].a / 255);
                                        }
                                    }
                                    if (layersInfo[rb.LayerID].InverseLinkage && rb.linkedBlock != null && rb.linkedBlock.color != null)
                                    {
                                        if (rb.color != null) for (int bi = 0; bi < QuadSq; bi++) rb.color[bi].a = (byte)(rb.color[bi].a * (255 - rb.linkedBlock.color[bi].a) / 255);
                                        if (rb.bump != null) for (int bi = 0; bi < QuadSq; bi++) rb.bump[bi] = (rb.bump[bi] * (255 - rb.linkedBlock.color[bi].a) / 255.0f);
                                        if (rb.SpecMask != null) for (int bi = 0; bi < QuadSq; bi++) rb.SpecMask[bi] = (byte)(rb.SpecMask[bi] * (255 - rb.linkedBlock.color[bi].a) / 255);
                                    }
                                }
                            }

                        }
                    }


            //// make maps

                foreach (MRenderTarget.LayersBlock mrb in mrt.rawLayersBlocks)
                    {
                        foreach (MRenderTarget.LayersBlock.RawBlock irbl in mrb.rawBlockLayers)
                        {
                            layersInfo[irbl.LayerID].LayerBlocks.Add(mrb);
                            mrb.dirty = true;
                        }

                    }

                EditorUtility.DisplayProgressBar("Make Materials", "Draw Maps", 0.0f);


			meshMapRenderer mmrM = new meshMapRenderer();

			mmrM.DrawMRTMeshMask (mrt);

             mrt.UpdateDirtyBlocks();

            
            //// make maps
            EditorUtility.DisplayProgressBar("Make Materials", "Save maps to textures", 0.0f);

/*            for (int iy = 0; iy < mrt.rtSizeY; iy++)
                for (int ix = 0; ix < mrt.rtSizeX; ix++)
                {
                    mrt.Albedo.SetPixel(ix, iy, albedo[ix + iy * mrt.rtSizeX]);
                    mrt.Emission.SetPixel(ix, iy, emission[ix + iy * mrt.rtSizeX]);
                    mrt.Bump.SetPixel(ix, iy, bump[ix + iy * mrt.rtSizeX]);
                    mrt.MetallicGlossOc.SetPixel(ix, iy, metallicGlossOc[ix + iy * mrt.rtSizeX]);
                }
            */

//			Debug.Log(mrt.rtSizeX+"x"+ mrt.rtSizeY+" NTextureID:"+mrt.NTextureID+" name:"+mrt.Name+" CTextureID:"+mrt.CTextureID+" DrawMicrovertices:"+mrt.DrawMicrovertices);

            KeepAssets.Add(mrt.Albedo);
            mrt.Albedo.Apply();



            if (mrt.thereSpecular)
            {
                KeepAssets.Add(mrt.MetallicGlossOc);
                mrt.MetallicGlossOc.Apply();
            }



            if (mrt.thereBump && CreateNormalMap)
            {
                KeepAssets.Add(mrt.Bump);
//                mrt.Bump.SetPixels32(mrt.bumpMap);
                mrt.Bump.Apply();
            }
            
            if (mrt.thereEmission && ImportEmission)
            {
                KeepAssets.Add(mrt.Emission);
                mrt.Emission.Apply();
            }

            if (mrt.thereAO && ExtractAO && !MargeAOAndMetallic)
            {
                KeepAssets.Add(mrt.Occlusion);
                mrt.Occlusion.Apply();
            }

            if (mrt.thereCurvature && ExtractCurvature)
            {
                KeepAssets.Add(mrt.Curvature);
                mrt.Curvature.Apply();
            }



			mrt.material.mainTexture = mrt.Albedo;
            if (mrt.thereSpecular && !SpecularSetupMode) mrt.material.SetTexture("_MetallicGlossMap", mrt.MetallicGlossOc);
            if (mrt.thereSpecular && SpecularSetupMode) mrt.material.SetTexture("_SpecGlossMap", mrt.MetallicGlossOc);
            if (mrt.thereBump && CreateNormalMap) mrt.material.SetTexture("_BumpMap", mrt.Bump);
            if (mrt.thereAO && ExtractAO && MargeAOAndMetallic) mrt.material.SetTexture("_OcclusionMap", mrt.MetallicGlossOc);
            if (mrt.thereAO && ExtractAO && !MargeAOAndMetallic) mrt.material.SetTexture("_OcclusionMap", mrt.Occlusion);
            if (mrt.thereEmission && ImportEmission) mrt.material.SetTexture("_EmissionMap", mrt.Emission);
            if (mrt.thereEmission && ImportEmission) mrt.material.SetColor("_EmissionColor", Color.white);
            else mrt.material.SetColor("_EmissionColor", Color.black);

            EditorUtility.SetDirty(mrt.material);

            EditorUtility.DisplayProgressBar("Make Materials", "RTS Finished", 0.0f);


			if (assetObject<Material>(mrt.Name) == null)
			{
				AssetDatabase.AddObjectToAsset(mrt.material, prefab);

			}

			KeepAssets.Add(mrt.material);

      //      if (TrianglesWithAlpha)
     //       {


                meshMapRenderer mmr = new meshMapRenderer();


			if (mmr.AnalyzeMRTAlpha(mrt))
            {

                    thereTransparent = true;
                    mrt.materialAlpha = new Material(mrt.material);
                    mrt.materialAlpha.SetFloat("_Mode", 3);
                    SetupMaterialWithBlendMode(mrt.materialAlpha, 3);
                    mrt.materialAlpha.name = mrt.Name + "Alpha";

                    AssetDatabase.AddObjectToAsset(mrt.materialAlpha, prefab);
                    KeepAssets.Add(mrt.materialAlpha);
            }


       //     }
        }

        AssetDatabase.SaveAssets();
        
	}

    public T assetObject<T>(string aname) where T : UnityEngine.Object
    {
        foreach (UnityEngine.Object ob in AssetObjects)
            if (ob is T && ob.name == aname) return ob as T;

        return null;

    }

    public void updatePrefabPath()
    {

        string apPath = AssetDatabase.GetAssetPath(this);

        if (apPath == null || apPath == "" || Path.GetExtension(apPath).ToLower() != ".prefab")
        {
            AssetPrefabPath = getAssetPath() + ".prefab";
        }
        else
        {
            AssetPrefabPath = AssetDatabase.GetAssetPath(this);
        }

    }

    public void updateAsset()
    {
		try{
        updatePrefabPath();

        if (SubObjects == null || SubObjects.Length < 1)
        {

            bool fileExist = false;
            string pPath = getAssetPath();
            fileExist = File.Exists(pPath);
            if (fileExist)
            {
                    LoadFromFile(pPath);
            }
            else
            {
                Debug.LogError("Lost 3b data");
                return;
            }
        }
/*        if (vCode < 4005018)
        {
            Debug.LogError("Requires version 4.5.18 or later of 3D Coat");
            EditorUtility.DisplayDialog("Error importing 3b file", "Error importing " + AssetPath +
            ". \r\nRequires version 4.5.18 or later of 3D Coat", "Close");
            return;
        }
*/



        KeepAssets.Clear();
        EditorUtility.DisplayProgressBar("UpdateAsset", "MakeMesh", 0.5f);
        makeMesh();


        List<OneLayerInfo> iLayerStack = new List<OneLayerInfo>();

        List<OneLayerInfo> iLayersList = new List<OneLayerInfo>();

        foreach (var item in layersInfo)
        {
            iLayersList.Add(item.Value);
        }

        for (int lid = iLayersList.Count - 1; lid >= 0; lid--)
        {
            if (iLayerStack.Count > 0)
            {
                iLayersList[lid].ParentLayer = iLayerStack[iLayerStack.Count - 1];
            }
            else
            {
                iLayersList[lid].ParentLayer = null;
            }

            if (iLayersList[lid].IsFolder) iLayerStack.Add(iLayersList[lid]);
            if (iLayersList[lid].EndOfSection && iLayerStack.Count > 0) iLayerStack.RemoveAt(iLayerStack.Count - 1);
            iLayersList[lid].revisionId = -1;
        }


        restoreCustomLayersVisible();

        for (int lid = iLayersList.Count - 1; lid >= 0; lid--)
        {
            bool vihResult = true;
            OneLayerInfo hl = iLayersList[lid];
            while (hl != null && hl.revisionId != lid && vihResult)
            {
                hl.revisionId = lid;
                vihResult = hl.CustomVisible;
                hl = hl.ParentLayer;
            }
            iLayersList[lid].VisibleInHierarchy = vihResult;
        }


        EditorUtility.DisplayProgressBar("UpdateAsset", "MakeMaterials", 0.75f);
        makeMaterials();
        EditorUtility.DisplayProgressBar("UpdateAsset", "SetMaterials", 1.0f);

        if (!_TrianglesWithAlpha) thereTransparent = false;

        Material[] mts = new Material[thereTransparent ? RTS.Length * 2 : RTS.Length];
        for (int i = 0; i < RTS.Length; i++)
        {
            mts[i] = RTS[i].material;
        }
        if (thereTransparent)
        {
            for (int i = 0; i < RTS.Length; i++)
            {
                mts[i + RTS.Length] = RTS[i].materialAlpha;
            }
        }
		if(prefab.GetComponent<MeshRenderer>() != null) prefab.GetComponent<MeshRenderer>().sharedMaterials = mts;

//			Debug.Log ("SubMatCountR"+prefab.transform.childCount);
			for (int i = 0; i < prefab.transform.childCount;  i++)
			{
				if(prefab.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>() != null)prefab.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().sharedMaterials = mts;

				int isoCnt = prefab.transform.GetChild (i).childCount;
//				Debug.Log ("SubMatCount"+isoCnt);
				for (int iso2 = 0; iso2 < isoCnt; iso2++)
				{
//					Debug.Log ("SetSubMat"+iso2);
					if(prefab.transform.GetChild(i).GetChild(iso2).gameObject.GetComponent<MeshRenderer>() != null) prefab.transform.GetChild(i).GetChild(iso2).gameObject.GetComponent<MeshRenderer>().sharedMaterials = mts;

				}
			}

        EditorUtility.ClearProgressBar();

        AssetDatabase.SaveAssets();

        AssetObjects = AssetDatabase.LoadAllAssetsAtPath(AssetPrefabPath);

        KeepAssets.Add(prefab);
        foreach (UnityEngine.Object uo in AssetObjects)
        {
            if (!KeepAssets.Contains(uo) && !(uo is Component) && !(uo is GameObject)) DestroyImmediate(uo, true);
        }
        AssetDatabase.SaveAssets();
		}catch {
			EditorUtility.ClearProgressBar ();

		}
    }

    public void updateCustomLayersVisible()
    {

        Dictionary<int, bool> iCustomLayersVisible = new Dictionary<int, bool>();
        foreach (var oli in layersInfo)
        {

            if (oli.Value.useCustomVisible)
            {
                iCustomLayersVisible.Add(oli.Key,oli.Value.CustomVisible);

            }

        }
        customLayersVisibleValues = new bool[iCustomLayersVisible.Count];
        customLayersVisible = new int[iCustomLayersVisible.Count];

        int i = 0;
        foreach (var sv in iCustomLayersVisible)
        {
            customLayersVisible[i] = sv.Key;
            customLayersVisibleValues[i] = sv.Value;
            i++;
        }
        updateVisibleInHierearhy();
    }

    public void updateVisibleInHierearhy()
    {

        foreach (var oli in layersInfo)
        {
            bool vihResult = true;
            OneLayerInfo hl = oli.Value;
            while (hl != null/* && hl.revisionId != oli.Key*/ && vihResult)
            {
                hl.revisionId = oli.Key;
                vihResult = hl.CustomVisible;
                hl = hl.ParentLayer;
            }
            oli.Value.VisibleInHierarchy = vihResult;
        }

    }

    public void updateMaterials()
    {

        updatePrefabPath();

        if (SubObjects.Length < 1)
        {
            updateAsset();
            return;
        }
        AssetObjects = AssetDatabase.LoadAllAssetsAtPath(AssetPrefabPath);

        List<OneLayerInfo> iLayerStack = new List<OneLayerInfo>();
        List<OneLayerInfo> iLayersList = new List<OneLayerInfo>();

        foreach (var item in layersInfo)
        {
            iLayersList.Add(item.Value);
        }

        for (int lid = iLayersList.Count - 1; lid >= 0; lid--)
        {
            if (iLayerStack.Count > 0)
            {
                iLayersList[lid].ParentLayer = iLayerStack[iLayerStack.Count - 1];
            }
            else
            {
                iLayersList[lid].ParentLayer = null;
            }

            if (iLayersList[lid].IsFolder) iLayerStack.Add(iLayersList[lid]);
            if (iLayersList[lid].EndOfSection && iLayerStack.Count > 0) iLayerStack.RemoveAt(iLayerStack.Count - 1);
            iLayersList[lid].revisionId = -1;
        }

        updateVisibleInHierearhy();

        KeepAssets.Clear();
        EditorUtility.DisplayProgressBar("UpdateAsset", "MakeMaterials", 0.75f);
        makeMaterials();

        EditorUtility.DisplayProgressBar("UpdateAsset", "SetMaterials", 1.0f);

        Material[] mts = new Material[RTS.Length];
        for (int i = 0; i < RTS.Length; i++)
        {
            mts[i] = RTS[i].material;
        }
		if(prefab.GetComponent<MeshRenderer>() != null) prefab.GetComponent<MeshRenderer>().sharedMaterials = mts;


//		Debug.Log ("SubMatCountR"+prefab.transform.childCount);
		for (int i = 0; i < prefab.transform.childCount;  i++)
        {
			if(prefab.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>() != null)prefab.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().sharedMaterials = mts;

			int isoCnt = prefab.transform.GetChild (i).childCount;
//			Debug.Log ("SubMatCount"+isoCnt);
			for (int iso2 = 0; iso2 < isoCnt; iso2++)
			{
//				Debug.Log ("SetSubMat"+iso2);
				if(prefab.transform.GetChild(i).GetChild(iso2).gameObject.GetComponent<MeshRenderer>() != null) prefab.transform.GetChild(i).GetChild(iso2).gameObject.GetComponent<MeshRenderer>().sharedMaterials = mts;

			}
        }

        EditorUtility.ClearProgressBar();

        AssetDatabase.SaveAssets();

        AssetObjects = AssetDatabase.LoadAllAssetsAtPath(AssetPrefabPath);

        foreach (UnityEngine.Object uo in AssetObjects)
        {
            if (!KeepAssets.Contains(uo) && (uo is Texture2D)) DestroyImmediate(uo, true);
        }
        AssetDatabase.SaveAssets();
    }


	public class ObjMeshData
	{
		public List<ObjMeshData> subObjects = new List<ObjMeshData>();
		public List<int> grpObjIds = new List<int>();
		public List<int> iTriangles = new List<int>();
		public List<int> iTriangleFaceIds = new List<int>();
		public List<Vector3> verts = new List<Vector3>();
		public int[] vertsLinkBuf; // buf for split subobjects
		public List<Vector3> norms = new List<Vector3>();
		public List<Vector2> uvs = new List<Vector2>();
		public List<Vector2> uvs2 = new List<Vector2>();
		public List<Vector4> tangets = new List<Vector4>();
		public string objectName;
		public GameObject gameObject;
		public Mesh mesh;

	}


	public ObjMeshData[] objectsMeshData;

	public void makeMesh(){
//        Debug.Log("AssetPath: " + AssetPath);
        updatePrefabPath();

		if (JoinObjects && VertsUV.Length >= 65000) {
			_joinObjects = false;
			Debug.Log ("it is impossible to join objects.\nThe number of vertices more than 65000");
		}




////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////// Triangulate faces /////////////////////////////////////////////////////////////

            PlaneTriangulator pl = new PlaneTriangulator();
            List<int> faceVuvs = new List<int>();

			List<int> iTriangles = new List<int>();
			List<int> iTriangleFaceIds = new List<int>();

            int faceId;


            for (int i = 0; i < Faces.Length; i++) if (Faces[i].Key < FacesInfo.Length)
            {
				faceId = (int)Faces[i].Key;

                faceVuvs.Add((int)Faces[i].Value);
                Vector3 iPos = Verts[(int)VertsUV[(int)Faces[i].Value].PosIndex].Pos;
                pl.AddPoint(iPos);

				if (i >= Faces.Length-1 || faceId != Faces[i+1].Key)
				{
					pl.Triangulate(true);

					for (int vid = 0; (float)vid < Math.Truncate(pl.Result.Count / 3.0) * 3; vid++)
					{
						iTriangles.Add((int)faceVuvs[pl.Result[vid]]);
						iTriangleFaceIds.Add(faceId);
					}

					pl = new PlaneTriangulator();
					faceVuvs.Clear();
				}

            }



/////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////// Ungroup Meshes ////////////////////////////////////////////////////////////////////////////////

		LinkedVerts = new int[Verts.Length, 10];
		LinkedVertsCnt = new int[Verts.Length];
		VertsSubObjId = new int[Verts.Length];

		for (int i = 0; i < Verts.Length; i++) {
			LinkedVertsCnt [i] = 0;
			VertsSubObjId [i] = -1;
		}

		for (int triId = 0; triId < iTriangles.Count / 3; triId++) {
			AddLinkedVert ((int)VertsUV[iTriangles [triId * 3]].PosIndex, (int)VertsUV[iTriangles [triId * 3 + 1]].PosIndex);
			AddLinkedVert ((int)VertsUV[iTriangles [triId * 3]].PosIndex, (int)VertsUV[iTriangles [triId * 3 + 2]].PosIndex);

			AddLinkedVert ((int)VertsUV[iTriangles [triId * 3 + 1]].PosIndex, (int)VertsUV[iTriangles [triId * 3]].PosIndex);
			AddLinkedVert ((int)VertsUV[iTriangles [triId * 3 + 1]].PosIndex, (int)VertsUV[iTriangles [triId * 3 + 2]].PosIndex);

			AddLinkedVert ((int)VertsUV[iTriangles [triId * 3 + 2]].PosIndex, (int)VertsUV[iTriangles [triId * 3]].PosIndex);
			AddLinkedVert ((int)VertsUV[iTriangles [triId * 3 + 2]].PosIndex, (int)VertsUV[iTriangles [triId * 3 + 1]].PosIndex);
		}


		findSubObjectsInMesh ();


//////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////// copy vertices data to arrays /////////////////////////////////////////////////////////////////

            Vector3[] verts = new Vector3[VertsUV.Length];
            Vector3[] norms = new Vector3[VertsUV.Length];
            Vector2[] uvs = new Vector2[VertsUV.Length];
            Vector2[] uvs2 = new Vector2[VertsUV.Length];

            Vector4[] tangets = new Vector4[VertsUV.Length];

		Debug.Log ("Scale: "+Scale);
		Debug.Log ("StartShift" + StartShift);
		Debug.Log ("UnitsScale: "+UnitsScale);
		Debug.Log ("MeasureScale: "+MeasureScale);
            for (int i = 0; i < VertsUV.Length; i++)
            {

				int vpi = (int)VertsUV[i].PosIndex;
//			verts[i] = Verts[(int)VertsUV[i].PosIndex].Pos*Scale/UnitsScale/MeasureScale+StartShift;
			verts[i] = Verts[vpi].Pos+StartShift;
                verts[i].x *= -1.0f;
                norms[i] = VertsN[(int)VertsUV[i].NIndex];
                norms[i].x *= -1.0f;
//                norms[i].z *= -1.0f;
                uvs[i].x = VertsUV[i].u;
                uvs[i].y = VertsUV[i].v;
                uvs2[i].x = VertsUV[i].u0;
                uvs2[i].y = VertsUV[i].v0;

                tangets[i] = VertsUV[i].T;
//                tangets[i].z *= -1.0f;
  //              tangets[i].y *= -1.0f;
                tangets[i].x *= -1.0f;

                Vector3 vT = VertsUV[i].T;
  //              vT.x *= -1.0f;
                Vector3 vB = VertsUV[i].B;
    //            vB.x *= -1.0f;

                Vector3 tcross = Vector3.Cross(vT, vB);
                if (Vector3.Distance(tcross, VertsN[(int)VertsUV[i].NIndex]) > 1.414213f)
                {
                    tangets[i].w = -1.0f;

                }
                else tangets[i].w = 1.0f;

            }

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////// Split by objects ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			int[] SubObjVertsLinks = new int[VertsUV.Length];

			for(int i = 0; i < VertsUV.Length; i++){
				SubObjVertsLinks[i] = -1;
			}


			int objCnt = 1;
			if (!JoinObjects) objCnt = SubObjects.Length;

			objectsMeshData = new ObjMeshData[objCnt];


			for(int SubObjId = 0; SubObjId < objCnt; SubObjId++){
				EditorUtility.DisplayProgressBar("Split by objects", "Split by objects", (float)SubObjId/(float)objCnt);

				objectsMeshData[SubObjId] = new ObjMeshData();
				if (SubObjects.Length > SubObjId) {
					objectsMeshData [SubObjId].objectName = SubObjects [SubObjId].Name;
				} else {
					objectsMeshData [SubObjId].objectName = Path.GetFileNameWithoutExtension(getAssetPath());
				}

				int triCnt = iTriangles.Count / 3;
				for(int iTriPId = triCnt*3-1; iTriPId >= 0; iTriPId--){
					if(JoinObjects || FacesInfo[iTriangleFaceIds[iTriPId]].ObjID == SubObjId){

						int iRawVertId = iTriangles[iTriPId];

						if(SubObjVertsLinks[iRawVertId] < 0){
							SubObjVertsLinks[iRawVertId] = objectsMeshData[SubObjId].verts.Count;
							objectsMeshData[SubObjId].grpObjIds.Add(VertsSubObjId[VertsUV[iRawVertId].PosIndex]);
							objectsMeshData[SubObjId].verts.Add(verts[iRawVertId]);
							objectsMeshData[SubObjId].tangets.Add(tangets[iRawVertId]);
							objectsMeshData[SubObjId].norms.Add(norms[iRawVertId]);
							objectsMeshData[SubObjId].uvs.Add(uvs[iRawVertId]);
							objectsMeshData[SubObjId].uvs2.Add(uvs2[iRawVertId]);

						}

						int iSubObjVertId = SubObjVertsLinks[iRawVertId];

						objectsMeshData[SubObjId].iTriangles.Add(iSubObjVertId);
						objectsMeshData[SubObjId].iTriangleFaceIds.Add(iTriangleFaceIds[iTriPId]);

					}
				}


				//// split by sub objects
			ObjMeshData omd = objectsMeshData[SubObjId];
			omd.subObjects.Clear ();
			if (UngroupMeshes || omd.verts.Count >= 65000) {


//				for (int triId = 0; triId < omd.iTriangles.Count / 3; triId++) {
//					if(triId % 100 == 1) EditorUtility.DisplayProgressBar ("Ungroup meshes "+tt, "Ungroup meshes "+tt, (float)triId / (float)(omd.iTriangles.Count / 3));
//				}

				Dictionary<int,ObjMeshData> iSubObjBuf = new Dictionary<int, ObjMeshData> ();

				omd.vertsLinkBuf = new int[omd.verts.Count];
				for (int i = 0; i < omd.vertsLinkBuf.Length; i++) {
					omd.vertsLinkBuf [i] = -1;
				}

				for (int triId = 0; triId < omd.iTriangles.Count / 3; triId++) {
					int vol = omd.grpObjIds [omd.iTriangles [triId * 3]];
					if (!iSubObjBuf.ContainsKey (vol)) {
						ObjMeshData newSubOmd = new ObjMeshData ();
						iSubObjBuf.Add (vol, newSubOmd);
						omd.subObjects.Add (newSubOmd);
						newSubOmd.objectName = omd.objectName;
					}
					ObjMeshData subOmd = iSubObjBuf [vol];

					for (int itp = 0; itp < 3; itp++) {
						int RawVIdx = omd.iTriangles [triId * 3 + itp];
						if (omd.vertsLinkBuf [RawVIdx] < 0) {
							omd.vertsLinkBuf [RawVIdx] = subOmd.verts.Count;
							subOmd.verts.Add (omd.verts [RawVIdx]);
							subOmd.tangets.Add (omd.tangets [RawVIdx]);
							subOmd.norms.Add (omd.norms [RawVIdx]);
							subOmd.uvs.Add (omd.uvs [RawVIdx]);
							subOmd.uvs2.Add (omd.uvs2 [RawVIdx]);

						}
						int vIdx = omd.vertsLinkBuf [RawVIdx];

						subOmd.iTriangles.Add (vIdx);
						subOmd.iTriangleFaceIds.Add (omd.iTriangleFaceIds [triId * 3 + itp]);

					}
				}
			}
		}



//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



		prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetPrefabPath);

		bool prefabAvailable = (prefab != null);

		GameObject prefabInstance;

		if (prefab == null)
		{
			prefabInstance = new GameObject();
		}
		else
		{
			prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
		}



		try
		{
			for (int i = prefabInstance.transform.childCount - 1; i >= 0; i--)
			{
				GameObject.DestroyImmediate(prefabInstance.transform.GetChild(i).gameObject);
			}


			objects.Clear();
			objects.Add(prefabInstance);

			List<ObjMeshData> usedOMDs = new List<ObjMeshData>();
			if (!JoinObjects)
			{
				for (int iOmdId = 1; iOmdId < objectsMeshData.Length; iOmdId++)
				{
					ObjMeshData iomd = objectsMeshData[iOmdId];
					GameObject subObj = new GameObject(iomd.objectName);
					subObj.transform.parent = prefabInstance.transform;
					objects.Add(subObj);
					iomd.gameObject = subObj;

					if(subObj.GetComponent<MeshRenderer>() == null) subObj.AddComponent<MeshRenderer>();
					if(subObj.GetComponent<MeshFilter>() == null) subObj.AddComponent<MeshFilter>();

					if(iomd.subObjects.Count > 0){
						for (int iSubOmdId = 1; iSubOmdId < iomd.subObjects.Count; iSubOmdId++)
						{
							ObjMeshData isubOmd = iomd.subObjects[iSubOmdId];
							isubOmd.objectName = iomd.objectName+"_part_"+iSubOmdId;
							GameObject subSubObj = new GameObject(isubOmd.objectName);

							subSubObj.transform.parent = subObj.transform;
							objects.Add(subSubObj);
							isubOmd.gameObject = subSubObj;

							if(subSubObj.GetComponent<MeshRenderer>() == null) subSubObj.AddComponent<MeshRenderer>();
							if(subSubObj.GetComponent<MeshFilter>() == null) subSubObj.AddComponent<MeshFilter>();

							usedOMDs.Add(isubOmd);

						}

					} else {
						usedOMDs.Add(iomd);
					}
				}

			} else {
				if(prefabInstance.GetComponent<MeshRenderer>() == null) prefabInstance.AddComponent<MeshRenderer>();
				if(prefabInstance.GetComponent<MeshFilter>() == null) prefabInstance.AddComponent<MeshFilter>();
				objectsMeshData[0].gameObject = prefabInstance;
				usedOMDs.Add(objectsMeshData[0]);

			}



			AssetObjects = AssetDatabase.LoadAllAssetsAtPath(AssetPrefabPath);

			for (int iomdId = 0; iomdId < usedOMDs.Count; iomdId++)
			{

				ObjMeshData iomd = usedOMDs[iomdId];

				//
				if (iomd.gameObject.GetComponent<MeshRenderer>() == null) iomd.gameObject.AddComponent<MeshRenderer>();

				MeshFilter mfi = iomd.gameObject.GetComponent<MeshFilter>();
				if(mfi == null) mfi = iomd.gameObject.AddComponent<MeshFilter>();

				Mesh imesh = null;

				imesh = assetObject<Mesh>(iomd.objectName);
				if (imesh == null)
				{
					imesh = new Mesh();
					imesh.name = iomd.objectName;
					//                AssetDatabase.AddObjectToAsset(mesh, prefab);
				}

				imesh.subMeshCount = RTS.Length*2;

				imesh.vertices = iomd.verts.ToArray();
				imesh.normals =  iomd.norms.ToArray();
				imesh.uv =  iomd.uvs.ToArray();
				imesh.tangents =  iomd.tangets.ToArray();
				imesh.triangles =  iomd.iTriangles.ToArray();

				iomd.mesh = imesh;
				iomd.gameObject.GetComponent<MeshFilter>().sharedMesh = imesh;
				iomd.gameObject.GetComponent<MeshFilter>().mesh = imesh;

            }



			if (prefabAvailable)
			{
				prefab = PrefabUtility.ReplacePrefab(prefabInstance, prefab);
			}
			else
			{
				prefab = PrefabUtility.CreatePrefab(AssetPrefabPath, prefabInstance);
				AssetDatabase.AddObjectToAsset(this, prefab);
			}
			KeepAssets.Add(this);



//			for (int iomdId = 0; iomdId < usedOMDs.Count; iomdId++)
//			{

//				ObjMeshData iomd = usedOMDs[iomdId];
//				iomd.gameObject.GetComponent<MeshFilter>().sharedMesh = iomd.mesh;
//				iomd.gameObject.GetComponent<MeshFilter>().mesh = iomd.mesh;
//			}


			for (int iomdId = 0; iomdId < usedOMDs.Count; iomdId++)
			{

				ObjMeshData iomd = usedOMDs[iomdId];

				MeshFilter mfi = iomd.gameObject.GetComponent<MeshFilter>();

				if (assetObject<Mesh>(mfi.sharedMesh.name) == null) AssetDatabase.AddObjectToAsset(iomd.mesh, prefab);
				KeepAssets.Add(iomd.mesh);
	//			mfi.sharedMesh = iomd.mesh;
	//			mfi.mesh = iomd.mesh;
            }

			if(objectsMeshData.Length == 1){
//				Debug.Log("Is single mesh");
				prefab.GetComponent<MeshFilter>().mesh = objectsMeshData[0].mesh;
			}

			if(objectsMeshData.Length > 1){
//				Debug.Log("Is multi-mesh");

				for(int ci = 0; ci < objectsMeshData.Length-1; ci++){
					if(objectsMeshData[ci+1].subObjects.Count > 0){
			//			Debug.Log("FFFFFFFFFFFfff"+objectsMeshData[ci+1].subObjects.Count);
						for(int ci2 = 0; ci2 < objectsMeshData[ci+1].subObjects.Count; ci2++){
			//				Debug.Log("RR"+ci2);
							if(prefab.transform.GetChild(ci).childCount > ci2) prefab.transform.GetChild(ci).GetChild(ci2).gameObject.GetComponent<MeshFilter>().mesh = objectsMeshData[ci+1].subObjects[ci2].mesh;
						}
					} else {
						prefab.transform.GetChild(ci).gameObject.GetComponent<MeshFilter>().mesh = objectsMeshData[ci+1].mesh;
					}
				}

			}
//			AssetDatabase.Refresh();

        }
        finally
        {

            GameObject.DestroyImmediate(prefabInstance);
            EditorUtility.ClearProgressBar();
        }


	}

	public const int QuadSize = 16;
	public const int QuadSh = 4;
	public const int QuadSq = (QuadSize * QuadSize);

	public const int TexQuadSize = 16;
	public const int TexQuadSh = 4;


/*	[MenuItem("Assets/Import 3DCoat File")]
	public static void Import3DCoatFile () {


        Asset3DCoat asset = CreateFromFile(AssetDatabase.GetAssetPath(Selection.activeObject));
        asset.makeMesh("");

    }*/

    public static Asset3DCoat CreateFromFile(string path)
    {
        

        UnityEngine.Object[] tmpAssetObjects = AssetDatabase.LoadAllAssetsAtPath(path + ".prefab");

        Asset3DCoat asset = null;
        if (tmpAssetObjects != null)
        {
            for (int ito = 0; ito < tmpAssetObjects.Length; ito++)
            {
                if (tmpAssetObjects[ito].name == "Settings") asset = tmpAssetObjects[ito] as Asset3DCoat;
            }
        }

        if (asset == null) asset = CreateInstance<Asset3DCoat>();

        asset.AssetPath = path;

        asset.LoadFromFile(path);

        asset.name = "Settings";

        asset.updateAsset();

//        AssetDatabase.CreateAsset(asset, path + ".asset");

        AssetDatabase.SaveAssets();


        return asset;
    }

    public void LoadFromFile(string path){
        EditorUtility.DisplayProgressBar("LoadFromFile", "LoadFromFile", 0.0f);
            //        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path + ".prefab");





            Asset3DCoat asset = this;


            FileStream fs = new FileStream(path, FileMode.Open);


            try
            {
                objects = new List<GameObject>();
                layersInfo = new Dictionary<int, OneLayerInfo>();
                firstMapsDraw = true;

            BinaryReader br = new BinaryReader(fs);
            fs.Position = 0;


            asset.firstMapsDraw = true;

            /////////////////////////////


            uint rtX = 2048;
            uint rtY = 2048;
            int TotalMiniVerts = 0;
            int MaxRT = 0;
//            bool oldspec = true;
//            bool HealMode = false;

            List<FaceInfo> iFacesInfo = new List<FaceInfo>();

            ////////////////////////////////
            uint magic = br.ReadUInt32();

   //         Debug.Log("Magic is " + magic);
            if (magic != MakeMagic("MESH"))
                return;

            /*uint v = */
            br.ReadUInt32();


            do
            {


                uint Mg = br.ReadUInt32();
                uint blockSize = br.ReadUInt32();
                long oldPos = fs.Position;
                /*
                if (Mg == MakeMagic("UVS0"))
                {
                    EditorUtility.DisplayProgressBar("LoadFromFile", "Load UV0", 0.0f);

                    uint nuv = br.ReadUInt32();
                    asset.oneUVSet.uv.Clear();
                    for (int i = 0; i < nuv; i++)
                    {
                        float iu = br.ReadSingle();
                        float iv = br.ReadSingle();
                        asset.oneUVSet.uv.Add(new Vector2(iu, iv));
                    }

                    uint ni = br.ReadUInt32();
                    asset.oneUVSet.Poly.Clear();
                    for (int i = 0; i < ni; i++)
                    {
                        cVec3i iv;
                        iv.x = br.ReadInt32();
                        iv.y = br.ReadInt32();

iv.z = br.ReadInt32();
                        asset.oneUVSet.Poly.Add(iv);
                    }

                }
                else*/
                    if (Mg == MakeMagic("PBF1"))
                    {
                        EditorUtility.DisplayProgressBar("LoadFromFile", "Load PFB", 0.0f);
                        int n = (int)br.ReadUInt32();
                        for (int i = 0; i < n; i++)
                        {
                            /*int f = (int)*/br.ReadUInt32();
//                            FaceInfo fi = asset.FacesInfo[f];
                            int sz = (int)br.ReadUInt32();
  //                          fi.PlanePts = new ShortPtArray();//alloc(sz);
                            for (int spai = 0; spai < sz; spai++)
                            {
                                ShortPt2 sp2 = new ShortPt2();
                                sp2.Pos.x = br.ReadSingle();
                                sp2.Pos.y = br.ReadSingle();
                                sp2.Pos.z = br.ReadSingle();
                                sp2.N.x = br.ReadSingle();
                                sp2.N.y = br.ReadSingle();
                                sp2.N.z = br.ReadSingle();
                                sp2.x = br.ReadInt16();
                                sp2.y = br.ReadInt16();
                            }

                        }
                        n = 0;
                    }
                    else
					if(Mg==MakeMagic("MSSK")){
						Scale = br.ReadSingle();
						StartShift.x = br.ReadSingle();
						StartShift.y = br.ReadSingle();
						StartShift.z = br.ReadSingle();
						CellSize = br.ReadSingle();
					}else
						if(Mg==MakeMagic("MEAS")){
							MeasureScale = br.ReadSingle();
							UnitsScale = br.ReadSingle();
						}else
                        if (Mg == MakeMagic("VERS"))
                        {
//                            int vsSize = (int)br.ReadUInt32();
                            EditorUtility.DisplayProgressBar("LoadFromFile", "Load vertices", 0.0f);
                            byte[] sVerBin = br.ReadBytes((int)blockSize);
                            asset.Vers3B = System.Text.Encoding.UTF8.GetString(sVerBin);
                            int dpos = asset.Vers3B.IndexOf("(");

                            string vnstr = asset.Vers3B.Substring(8, dpos-8);


                            string[] vsblocks = vnstr.Split('.');

                            vCode = 0;
                            if (vsblocks.Length > 0)
                            {
                                int bBuf = 0;
                                int.TryParse(vsblocks[0], out bBuf);
                                vCode += bBuf * 1000000;
                            }
                            if (vsblocks.Length > 1)
                            {
                                int bBuf = 0;
                                int.TryParse(vsblocks[1], out bBuf);
                                vCode += bBuf * 1000;
                            }
                            if (vsblocks.Length > 2)
                            {
                                int bBuf = 0;
                                int.TryParse(vsblocks[2], out bBuf);
                                vCode += bBuf;
                            }
						vCode = 5005018;
                        }
                        else
                            if (Mg == MakeMagic("MAQ3"))
                        {

                            int iMatId = (int)br.ReadUInt32();
                     //       Debug.Log("iMatId" + iMatId);
                            MRenderTarget mrl = asset.RTS[iMatId];
                            mrl.MatID = iMatId;
                            mrl.Lx = (int)br.ReadUInt16();
                            mrl.Ly = (int)br.ReadUInt16();

                            int nq = (int)br.ReadUInt32();
                            mrl.rawLayersBlocks = new MRenderTarget.LayersBlock[nq];
                            for (int iq = 0; iq < nq; iq++)
                            {
                                if(iq % 1000 == 1)EditorUtility.DisplayProgressBar("LoadFromFile", "Load textures data", (float)iq/(float)nq);
                                MRenderTarget.LayersBlock lb = new MRenderTarget.LayersBlock();

                                lb.posX = br.ReadUInt16(); 
                                lb.posY = br.ReadUInt16();

                                ushort F = br.ReadUInt16();
                                if (F > 0)
                                {
                                    lb.freeze = new ushort[QuadSq];
                                    for (int j = 0; j < QuadSq; j++)
                                    {
                                        lb.freeze[j] = br.ReadUInt16();
                                    }
                                }

                                int nn = br.ReadUInt16();
                                lb.rawBlockLayers = new MRenderTarget.LayersBlock.RawBlock[nn];
                                for (int j = 0; j < nn; j++)
                                {
                                    MRenderTarget.LayersBlock.RawBlock rlb = new MRenderTarget.LayersBlock.RawBlock();

                                    rlb.LayerID = (int)br.ReadUInt16();
                                    ushort opt = br.ReadUInt16();
                                    if ((opt & 1) > 0)
                                    {// bump
                                        rlb.bump = new float[QuadSq];
                                        for (int i = 0; i < QuadSq; i++)
                                        {
                                            rlb.bump[i] = br.ReadSingle();
                                        }
                                    }
                                    if ((opt & 2) > 0)
                                    {
                                        rlb.Specular = new byte[QuadSq];
                                        rlb.SpecMask = new byte[QuadSq];
                                        rlb.Metalness = new byte[QuadSq];
                                        for (int i = 0; i < QuadSq; i++)
                                        {
                                            rlb.Specular[i] = br.ReadByte();
                                            rlb.SpecMask[i] = br.ReadByte();
                                            rlb.Metalness[i] = br.ReadByte();
                                        }
                                    }
                                    if ((opt & 4) > 0)
                                    {
                                        rlb.color = new Color32[QuadSq];
                                        for (int i = 0; i < QuadSq; i++)
                                        {
                                            rlb.color[i].b = br.ReadByte();
                                            rlb.color[i].g = br.ReadByte();
                                            rlb.color[i].r = br.ReadByte();
                                            rlb.color[i].a = br.ReadByte();
                                        }
                                    }

                                    lb.rawBlockLayers[j] = rlb;

                                }
                                mrl.rawLayersBlocks[iq] = lb;
                            }
                            blockSize = (uint)(fs.Position - oldPos);
                        }
                        else
                            if (Mg == MakeMagic("POS0"))
                            {
                                EditorUtility.DisplayProgressBar("LoadFromFile", "Load pos0", 0.0f);
                                int cnt = System.Convert.ToInt32(blockSize) / 12;
                                asset.pos0 = new Vector3[cnt];
                                for (int i = 0; i < cnt; i++)
                                {
                                    Vector3 pv;
                                    pv.x = br.ReadSingle();
                                    pv.y = br.ReadSingle();
                                    pv.z = br.ReadSingle();
                                    asset.pos0[i] = pv;
                                }
                            }
                            else
                                if (Mg == MakeMagic("LR01"))
                                {
                                    EditorUtility.DisplayProgressBar("LoadFromFile", "Load layers info", 0.0f);

                                    /*int L=(int)*/
                                    br.ReadUInt32();
                                    int ls = (int)br.ReadUInt32();
                                    byte[] xmlData = br.ReadBytes(ls);
                                    string xmlSource = System.Text.Encoding.UTF8.GetString(xmlData, 0, ls);

                                    xmlSource = xmlSource.Replace("&lt", "_lt_");
                                    xmlSource = xmlSource.Replace("&gt", "_gt_");
                                    XmlDocument xmlDocument = new XmlDocument();
                                    xmlDocument.LoadXml(xmlSource);

                                    asset.layersInfo.Clear();
                                    foreach (XmlNode rootNode in xmlDocument.ChildNodes)
                                    {
                                        foreach (XmlNode layerNode in rootNode.ChildNodes)
                                        {
                                            if (layerNode.LocalName == "OneLayerInfo")
                                            {
                                                OneLayerInfo oli = new OneLayerInfo();
                                                foreach (XmlNode attr in layerNode.ChildNodes)
                                                {
                                                    if (attr.LocalName == "Name") oli.Name = attr.InnerText;
                                                    if (attr.LocalName == "LayerID") int.TryParse(attr.InnerText, out oli.LayerID);
                                                    if (attr.LocalName == "Visible") oli.Visible = (attr.InnerText == "true");
                                                    if (attr.LocalName == "IsFolder") oli.IsFolder = (attr.InnerText == "true");
                                                    if (attr.LocalName == "EndOfSection") oli.EndOfSection = (attr.InnerText == "true");
                                                    if (attr.LocalName == "IsOpen") oli.IsOpen = (attr.InnerText == "true");
                                                    if (attr.LocalName == "LockTransparency") oli.LockTransparency = (attr.InnerText == "true");
                                                    if (attr.LocalName == "UseAsWeightmap") oli.UseAsWeightmap = (attr.InnerText == "true");
                                                    if (attr.LocalName == "ColorOp") int.TryParse(attr.InnerText, out oli.ColorOp);
                                                    if (attr.LocalName == "DepthOp") int.TryParse(attr.InnerText, out oli.DepthOp);
                                                    if (attr.LocalName == "EmbossPower") float.TryParse(attr.InnerText, out oli.EmbossPower);
                                                    if (attr.LocalName == "DepthTransparency") float.TryParse(attr.InnerText, out oli.DepthTransparency);
                                                    if (attr.LocalName == "ColorTransparency") float.TryParse(attr.InnerText, out oli.ColorTransparency);
                                                    if (attr.LocalName == "Contrast") float.TryParse(attr.InnerText, out oli.Contrast);
                                                    if (attr.LocalName == "Brightness") float.TryParse(attr.InnerText, out oli.Brightness);
                                                    if (attr.LocalName == "SpecContrast") float.TryParse(attr.InnerText, out oli.SpecContrast);
                                                    if (attr.LocalName == "SpecularMod") float.TryParse(attr.InnerText, out oli.SpecularMod);
                                                    if (attr.LocalName == "SpecBrightness") float.TryParse(attr.InnerText, out oli.SpecBrightness);
                                                    if (attr.LocalName == "GlossMod") float.TryParse(attr.InnerText, out oli.GlossMod);
                                                    if (attr.LocalName == "RoughMod") float.TryParse(attr.InnerText, out oli.RoughMod);
                                                    if (attr.LocalName == "MetalnessOpacity") float.TryParse(attr.InnerText, out oli.MetalnessOpacity);
                                                    if (attr.LocalName == "MetalBrightness") float.TryParse(attr.InnerText, out oli.MetalBrightness);
                                                    if (attr.LocalName == "LinkedLayer") oli.LinkedLayer = attr.InnerText;
                                                    if (attr.LocalName == "InverseLinkage") oli.InverseLinkage = (attr.InnerText == "true");
                                                    if (attr.LocalName == "HiddenMaskOwner") oli.HiddenMaskOwner = attr.InnerText;

                                                }
                                                if (asset.layersInfo.ContainsKey(oli.LayerID)) asset.layersInfo[oli.LayerID] = oli;
                                                else asset.layersInfo.Add(oli.LayerID, oli);
                                            }
                                        }
                                    }
                                }
                                else
                                    if (Mg == MakeMagic("RNTS"))
                                    {
                                        EditorUtility.DisplayProgressBar("LoadFromFile", "Load RTS", 0.0f);

                                        int ls = (int)br.ReadUInt32();
                                        byte[] xmlData = br.ReadBytes(ls);
                                        string xmlSource = System.Text.Encoding.UTF8.GetString(xmlData, 0, ls);

                                        XmlDocument xmlDocument = new XmlDocument();
                                        xmlDocument.LoadXml(xmlSource);

                                        List<MRenderTarget> iRTS = new List<MRenderTarget>();
                                        foreach (XmlNode rootNode in xmlDocument.ChildNodes)
                                        {
                                            foreach (XmlNode layerNode in rootNode.ChildNodes)
                                            {
                                                if (layerNode.LocalName == "MRenderTarget")
                                                {
                                                    MRenderTarget mrt = new MRenderTarget();
                                                    foreach (XmlNode attr in layerNode.ChildNodes)
                                                    {
                                                        if (attr.LocalName == "Name") mrt.Name = attr.InnerText;
                                                        if (attr.LocalName == "rtSizeX") int.TryParse(attr.InnerText, out mrt.rtSizeX);
                                                        if (attr.LocalName == "rtSizeY") int.TryParse(attr.InnerText, out mrt.rtSizeY);
                                                        if (attr.LocalName == "SubPatchLevel") int.TryParse(attr.InnerText, out mrt.SubPatchLevel);
                                                        if (attr.LocalName == "DrawMicrovertices") mrt.DrawMicrovertices = (attr.InnerText == "true");
                                                        if (attr.LocalName == "SkipNormalmap") mrt.SkipNormalmap = (attr.InnerText == "true");
                                                        if (attr.LocalName == "UseExternalTexture") mrt.UseExternalTexture = (attr.InnerText == "true");
                                                        //								if(attr.LocalName == "ExtTextureName") mrt.ExtTextureName = (attr.InnerText == "true");rue");
                                                    }
                                                    mrt.asset3DCoat = asset;
                                                    iRTS.Add(mrt);
                                                }
                                            }
                                        }
                                        asset.RTS = iRTS.ToArray();

                                    }
                                    else /*
                                        if (Mg == MakeMagic("UVST"))
                                        {
                                            EditorUtility.DisplayProgressBar("LoadFromFile", "Load UV Sets", 0.0f);

                                            int ls = (int)br.ReadUInt32();
                                            byte[] xmlData = br.ReadBytes(ls);
                                            string xmlSource = System.Text.Encoding.UTF8.GetString(xmlData, 0, ls).Replace("<MtlsList", "<!--MtlsList").Replace("/MtlsList>", "/MtlsList-->").Replace("<&RTUVSet::AddMaterial", "<!--&RTUVSet::AddMaterial").Replace("/&RTUVSet::AddMaterial>", "/&RTUVSet::AddMaterial-->");


                                            XmlDocument xmlDocument = new XmlDocument();
                                            xmlDocument.LoadXml(xmlSource);
                                            
                                            asset.UVSets.Clear();
                                            foreach (XmlNode rootNode in xmlDocument.ChildNodes)
                                            {
                                                foreach (XmlNode layerNode in rootNode.ChildNodes)
                                                {
                                                    if (layerNode.LocalName == "RTUVSet")
                                                    {
                                                        RTUVSet rtuvs = new RTUVSet();
                                                        foreach (XmlNode attr in layerNode.ChildNodes)
                                                        {
                                                            if (attr.LocalName == "Name") rtuvs.Name = attr.InnerText;
                                                            if (attr.LocalName == "uvTexSizeX") int.TryParse(attr.InnerText, out rtuvs.TexSizeX);
                                                            if (attr.LocalName == "uvTexSizeX") int.TryParse(attr.InnerText, out rtuvs.TexSizeY);
                                                        }
                                                        asset.UVSets.Add(rtuvs);
                                                    }
                                                }
                                            }
                                        }
                                        else*/
                                            if (Mg == MakeMagic("OBJS"))
                                            {
                                                EditorUtility.DisplayProgressBar("LoadFromFile", "Load Objects", 0.0f);

                                                int ls = (int)br.ReadUInt32();
                                                byte[] xmlData = br.ReadBytes(ls);
                                                string xmlSource = System.Text.Encoding.UTF8.GetString(xmlData, 0, ls).Replace("<&OneSubObject::DeleteElm>", "<!--&OneSubObject::DeleteElm>").Replace("</&OneSubObject::DeleteElm>", "</&OneSubObject::DeleteElm-->");

                                                XmlDocument xmlDocument = new XmlDocument();
                                                xmlDocument.LoadXml(xmlSource);

                                                List<OneSubObject> iSubObjects = new List<OneSubObject>();
                                                foreach (XmlNode rootNode in xmlDocument.ChildNodes)
                                                {
                                                    foreach (XmlNode layerNode in rootNode.ChildNodes)
                                                    {
                                                        if (layerNode.LocalName == "OneSubObject")
                                                        {
                                                            OneSubObject oso = new OneSubObject();
                                                            foreach (XmlNode attr in layerNode.ChildNodes)
                                                            {
                                                                if (attr.LocalName == "Name") oso.Name = attr.InnerText;
                                                                if (attr.LocalName == "Visible") oso.Visible = (attr.InnerText == "true");
                                                                if (attr.LocalName == "Locked") oso.Locked = (attr.InnerText == "true");
                                                            }
                                                            iSubObjects.Add(oso);
                                                        }
                                                    }
                                                }
                                                asset.SubObjects = iSubObjects.ToArray();

                                            }
                                            else
                                                if ((Mg >= MakeMagic("SUR2") && Mg <= MakeMagic("SUR9")) || Mg == MakeMagic("SAR5") || Mg == MakeMagic("SAR6") || Mg == MakeMagic("SAR7") || Mg == MakeMagic("SAR8"))
                                                {

                                                    if (Mg == MakeMagic("SAR5")) Mg = MakeMagic("SUR5");
                                                    if (Mg == MakeMagic("SAR6")) Mg = MakeMagic("SUR6");
                                                    if (Mg == MakeMagic("SAR7")) Mg = MakeMagic("SUR7");
                                                    if (Mg == MakeMagic("SAR8")) Mg = MakeMagic("SUR8");

//                                                    bool usemet = (Mg == MakeMagic("SUR8"));

                                                    EditorUtility.DisplayProgressBar("LoadFromFile", "Load mesh",0.0f);

                                                    if (rtX != asset.rtSizeX || rtY != asset.rtSizeY)
                                                    {
                                                        asset.rtSizeX = (int)rtX;
                                                        asset.rtSizeY = (int)rtY;
                                                    }


                                                    if (asset.SubObjects.Length == 0)
                                                    {
                                                        OneSubObject SO = new OneSubObject();
                                                        SO.Name = Path.GetFileNameWithoutExtension(path);
                                                        asset.SubObjects = new OneSubObject[1];
                                                        asset.SubObjects[0] = SO;
                                                    }


                                                    int SubSurfaceSize = (int)br.ReadUInt32();
                                                    /*int nfaces=(int)*/
                                                    br.ReadUInt32();
                                                    int nuv = (int)br.ReadUInt32();
                                                    int nv = (int)br.ReadUInt32();
                                                    int nvn = (int)br.ReadUInt32();


                                                    VertexUV v_uv = new VertexUV();
                                                    asset.VertsUV = new VertexUV[nuv];
                                                    for (int i = 0; i < nuv; i++) asset.VertsUV[i] = v_uv;

                                                    if (Mg >= MakeMagic("SUR4"))
                                                    {
                                                        for (int i = 0; i < nuv; i++)
                                                        {
                                                            VertexUV iv_uv = new VertexUV();

                                                            iv_uv.PosIndex = br.ReadUInt32();
                                                            iv_uv.NIndex = br.ReadUInt32();
                                                            iv_uv.u = br.ReadSingle();
                                                            iv_uv.v = br.ReadSingle();
                                                            iv_uv.T.x = br.ReadSingle();
                                                            iv_uv.T.y = br.ReadSingle();
                                                            iv_uv.T.z = br.ReadSingle();
                                                            iv_uv.B.x = br.ReadSingle();
                                                            iv_uv.B.y = br.ReadSingle();
                                                            iv_uv.B.z = br.ReadSingle();
                                                            iv_uv.u0 = br.ReadSingle();
                                                            iv_uv.v0 = br.ReadSingle();

                                                            asset.VertsUV[i] = iv_uv;

                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < nuv; i++)
                                                        {
                                                            VertexUV iv_uv = new VertexUV();

                                                            iv_uv.PosIndex = br.ReadUInt32();
                                                            iv_uv.NIndex = br.ReadUInt32();
                                                            iv_uv.u = br.ReadSingle();
                                                            iv_uv.v = br.ReadSingle();
                                                            iv_uv.T.x = br.ReadSingle();
                                                            iv_uv.T.y = br.ReadSingle();
                                                            iv_uv.T.z = br.ReadSingle();
                                                            iv_uv.B.x = br.ReadSingle();
                                                            iv_uv.B.y = br.ReadSingle();
                                                            iv_uv.B.z = br.ReadSingle();

                                                            asset.VertsUV[i] = iv_uv;
                                                        }
                                                    }


                                                    asset.VertsN = new Vector3[nvn];
                                                    for (int i = 0; i < nvn; i++)
                                                    {
                                                        Vector3 vn = new Vector3();
                                                        vn.x = br.ReadSingle();
                                                        vn.y = br.ReadSingle();
                                                        vn.z = br.ReadSingle();
                                                        asset.VertsN[i] = vn;
                                                    }

                                                    asset.Verts = new VertexPos[nv];
                                                    for (int i = 0; i < nv; i++)
                                                    {
                                                        VertexPos vp = new VertexPos();
                                                        vp.Pos.x = br.ReadSingle();
                                                        vp.Pos.y = br.ReadSingle();
                                                        vp.Pos.z = br.ReadSingle();
                                                        vp.W = br.ReadSingle();
                                                        vp.SelectionDegree = br.ReadByte();
                                                        vp.SubdivLevel = br.ReadByte();
                                                        vp.TempSelection = br.ReadByte();
                                                        br.ReadByte();// for anign
                                                        asset.Verts[i] = vp;
                                                    }



                                                    uint sz = br.ReadUInt32();
                                                    long nextPos = fs.Position + sz;

                                                    int fcnt = br.ReadInt32();
                                                    asset.Faces = new uniItem[fcnt];
                                                    for (int i = 0; i < fcnt; i++)
                                                    {
                                                        uint fk = br.ReadUInt32();
                                                        uint fv = br.ReadUInt32();

                                                        asset.Faces[i] = new uniItem(fk, fv);
                                                    }
                                                    fs.Position = nextPos;


                                                    /*uint sz2=*/
                                                    br.ReadUInt32();
                                                    long nextPos2 = fs.Position + sz;

                                                    int fcnt2 = br.ReadInt32();
                                                    asset.SrcFaces = new uniItem[fcnt2];
                                                    for (int i = 0; i < fcnt2; i++)
                                                    {
                                                        uint fk = br.ReadUInt32();
                                                        uint fv = br.ReadUInt32();

                                                        asset.SrcFaces[i] = new uniItem(fk, fv);
                                                    }
                                                    fs.Position = nextPos2;


                                                    int nfc = (int)br.ReadUInt32();
                                                    for (int i = 0; i < nfc; i++)
                                                    {
                                                        FaceInfo fi = new FaceInfo();
  //                                                      fi.Points = null;
                                                        fi.SubSizeX = 0;
                                                        fi.SubSizeY = 0;


                                                        fi.SubdivLevel = br.ReadByte();
                                                        fi.LFace = br.ReadUInt32();
                                                        fi.RFace = br.ReadUInt32();
                                                        fi.UpFace = br.ReadUInt32();
                                                        fi.DnFace = br.ReadUInt32();
                                                        fi.ChangeStamp = 0;
                                                        fi.RTIdx = 0;


                                                        if (Mg >= MakeMagic("SUR7"))
                                                        {
                                                            fi.RTIdx = (short)br.ReadUInt16();
                                                            fi.uvIdx = (short)br.ReadUInt16();
                                                            if (fi.RTIdx != -1 && fi.RTIdx < asset.RTS.Length)
                                                            {
                                                                fi.uvIdx = fi.RTIdx;
                                                            }
                                                        }
                                                        MaxRT = Mathf.Max(MaxRT, (int)fi.RTIdx);
                                                        int SzX = SubSurfaceSize;
                                                        int SzY = SubSurfaceSize;
                                                        if (Mg >= MakeMagic("SUR6"))
                                                        {
                                                          //  Debug.Log("CSS");
                                                            SzX = (int)br.ReadUInt16();
                                                            SzY = (int)br.ReadUInt16();
                                                        }
                                                        fi.SubSizeX = (short)SzX;
                                                        fi.SubSizeY = (short)SzY;
                                                        TotalMiniVerts += SzX * SzY;
                                                        if (TotalMiniVerts > 0)
                                                        {
                                                            Debug.LogError("Error load 3b file. Please use perPixel mode only.");
                                                            return;
                                                        }
//                                                        Debug.Log("ss" + SzX + " " + SzY);

                                                        if (Mg >= MakeMagic("SUR3"))
                                                        {
                                                            uint ID = br.ReadUInt32();
                                                            fi.ObjID = (ushort)(ID & 0x0FFFFFFF);
                                                            fi.MtlID = (ushort)(br.ReadUInt32());
                                                            if (fi.ObjID > asset.SubObjects.Length) fi.ObjID = 0;
                                                            if (Mg >= MakeMagic("SUR5"))
                                                            {
                                                                fi.ChangeStamp = br.ReadSingle();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            fi.ObjID = 0;
                                                            fi.MtlID = 0;
                                                            fi.Visible = true;
                                                        }
                                                        int n = SzX * SzY;
            //                                            if (n > 0)
          //                                              {
    //                                                        fi.Points = new List<SurfPoint3D>(n);
        //                                                }
      //                                                  else fi.Points = null;
                                                        fi.SubSizeX = (short)SzX;
                                                        fi.SubSizeY = (short)SzY;

                                                        for (int p = 0; p < n; p++)
                                                        {
                                        //                    SurfPoint3D sp = fi.Points[p];
                                                            /*sp.Freeze =*/ br.ReadSingle();
                                                            /*sp.N0.x =*/ br.ReadSingle();
                                                            /*sp.N0.y =*/ br.ReadSingle();
                                                            /*sp.N0.z =*/ br.ReadSingle();
                                                            //sp.N = Vector3.zero;
                                                            //sp.Color = 0;
                                                            //sp.Pos = Vector3.zero;
                                                            //fi.Points[p] = sp;
                                                        }



                                                        iFacesInfo.Add(fi);
                                                    }



                                                    blockSize = (uint)(fs.Position - oldPos);

                                                }
                                                else
                                                {


                                                    //			Debug.Log("Unknow Magic: " + magic+" size: "+blockSize+" pos: "+fs.Position);

                                                }



                ///////////////////////////////////////////////////////////////////////////////////////////
                fs.Position = oldPos + blockSize;

            } while (fs.Position < fs.Length - 4);

            asset.FacesInfo = iFacesInfo.ToArray();

//            LayersInfoArray = new OneLayerInfo[asset.layersInfo.Count];
  /*              int lii = 0;
            foreach (var oli in asset.layersInfo)
            {
                LayersInfoArray[lii] = oli.Value;
                lii++;
            }
                */
        }
        finally
        {
            fs.Close();
            EditorUtility.ClearProgressBar();
        }
	}



}


class All3bPostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (var str in importedAssets)
        {


            if (Path.GetExtension(str).ToLower() == ".3b" && str.IndexOf("tempfile.") < 1)
            {
                bool moved = false;
                for (int i = 0; i < movedAssets.Length; i++){
                    if (movedAssets[i] == str) moved = true;
                }

                if (!moved)
                {
                    Asset3DCoat asset3b = Asset3DCoat.CreateFromFile(str);
                    List<Asset3DCoat> linkedList = asset3b.findLinkedAssets();
                    foreach (Asset3DCoat linkedAsset in linkedList)
                    {
                        linkedAsset.assignDataFrom(asset3b);
                        linkedAsset.updateAsset();
//                        linkedAsset.updateMaterials();

                    }

                }
            }

        }
//        foreach (var str in deletedAssets)
  //      {
   //         Debug.Log("Deleted Asset: " + str);
    //    }

        for (var i = 0; i < movedAssets.Length; i++)
        {
            if (Path.GetExtension(movedAssets[i]).ToLower() == ".3b")
            {
                if (File.Exists(movedFromAssetPaths[i] + ".prefab"))
                AssetDatabase.MoveAsset(movedFromAssetPaths[i] + ".prefab", movedAssets[i] + ".prefab");

                List<Asset3DCoat> linkedAssets = Asset3DCoat.findLinkedAssets(movedFromAssetPaths[i]);
                foreach (Asset3DCoat lAsset in linkedAssets)
                {
                    lAsset.AssetPath = movedAssets[i];


                }
                //           Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
            }
        }
    }
}

[CustomEditor(typeof(Asset3DCoat)), CanEditMultipleObjects]
public class Asset3DCoatEditor : Editor
{
    private static System.Type ProjectWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ProjectBrowser");
    private static EditorWindow projectWindow = null;

    public static int selectedPageId = 0;

    public int drawGUIPos = 0;
    public int drawGUILeft = 0;

    public static Texture2D LoadPNG(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
        }
        return tex;
    }

    public static void StartRenameSelectedAsset()
    {
        if (projectWindow == null)
        {
            projectWindow = EditorWindow.GetWindow(ProjectWindowType);
        }

        if (projectWindow != null)
        {
            var e = new Event();
            e.keyCode = KeyCode.F2;
            e.type = EventType.KeyDown;
            projectWindow.SendEvent(e);
        }
    }

    public static string GetSelectedPathOrFallback()
    {
        string path = "Assets";

        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }



    public bool checkBox(string label, bool value)
    {

        drawGUIPos = drawGUIPos + 20;
        return EditorGUI.Toggle(new Rect(drawGUILeft, drawGUIPos - 20, Screen.width - drawGUILeft - 10, EditorGUIUtility.singleLineHeight), label, value);
    }
    public bool checkBoxLeft(string label, bool value)
    {

        drawGUIPos = drawGUIPos + 20;
        return EditorGUI.ToggleLeft(new Rect(drawGUILeft, drawGUIPos - 20, Screen.width - drawGUILeft - 10, EditorGUIUtility.singleLineHeight), label, value);
    }

    public float floatField(string label, float value)
    {

        drawGUIPos = drawGUIPos + 20;
        return EditorGUI.FloatField(new Rect(drawGUILeft, drawGUIPos - 20, Screen.width - drawGUILeft - 10, EditorGUIUtility.singleLineHeight), label, value);
    }


    void showSubLayers(OneLayerInfo layer)
    {

        Asset3DCoat asset3b = target as Asset3DCoat;
        drawGUILeft += 25;
        foreach (var oli in asset3b.layersInfo)
            if (!oli.Value.EndOfSection && oli.Value.ParentLayer == layer)
            {
                bool oldCV = oli.Value.CustomVisible;
                if (oli.Value.useCustomVisible && GUI.Button(new Rect(drawGUILeft - 40, drawGUIPos, 30, 18), "def")) {
                    oli.Value.useCustomVisible = false;
//                    asset3b.updateCustomLayersVisible();
                    if (asset3b.InteractiveUpdate && oli.Value.Visible != oldCV)
                    {
                        oli.Value.SetLayerDirty();
                        //                        asset3b.updateMaterials(); 
                    }
                }
                oli.Value.CustomVisible = checkBoxLeft(oli.Value.Name, oli.Value.CustomVisible);
                showSubLayers(oli.Value);
            }
        drawGUILeft -= 25;

    }
    void showLayersSettings()
    {
        Asset3DCoat asset3b = target as Asset3DCoat;

        drawGUIPos = 100;

        drawGUILeft = 10;
        asset3b.InteractiveUpdate = checkBox("Interactive update", asset3b.InteractiveUpdate);

        drawGUIPos += 20;


        drawGUILeft = 30;

        OneLayerInfo.changeInLayers = false;

        showSubLayers(null);

        if (OneLayerInfo.changeInLayers)
        {
            asset3b.updateCustomLayersVisible();
//            asset3b.updateVisibleInHierearhy();
            foreach (MRenderTarget mrt in asset3b.RTS)
            {
//                Debug.Log("ddddddddddddddddddddddd");
                mrt.UpdateDirtyBlocks();
            }
  //          AssetDatabase.SaveAssets();
    //        AssetDatabase.Refresh();
//            asset3b.updateMaterials();
        }

        if (GUI.Button(new Rect(116 + 10, drawGUIPos + 20, 112, 20), "Apply"))
        {
			asset3b.updateAsset();
        }
        if (GUI.Button(new Rect(10, drawGUIPos + 20, 112, 20), "Reset"))
        {

            foreach (var oli in asset3b.layersInfo) { 
                oli.Value.useCustomVisible = false;
                
            }
            asset3b.updateCustomLayersVisible();
			asset3b.updateAsset();
        }

    }

    void showImportSettings()
    {

        Asset3DCoat asset3b = target as Asset3DCoat;

/*        if (asset3b.LayersInfoArray.Length != asset3b.layersInfo.Count)
        {
            asset3b.layersInfo.Clear();
            foreach (OneLayerInfo oli in asset3b.LayersInfoArray)
            {
                asset3b.layersInfo.Add(oli.LayerID, oli);

            }
        }*/
        asset3b.AssetPath = EditorGUI.TextField(new Rect(10, 100, Screen.width - 20, 18), "3b asset path", asset3b.AssetPath);

        drawGUIPos = 130;
        drawGUILeft = 10;

		if (asset3b.SubObjects.Length > 1) asset3b.JoinObjects = checkBox("Join objects", asset3b.JoinObjects);
		if (asset3b.SubObjects.Length <= 1 || !asset3b.JoinObjects) asset3b.UngroupMeshes = checkBox("Ungroup meshes", asset3b.UngroupMeshes);
        asset3b.SpecularSetupMode = checkBox("(Specular setup) mode", asset3b.SpecularSetupMode);
        if (asset3b.thereAO) asset3b.HideAOInAlbedo = checkBox("Hide AO in albedo", asset3b.HideAOInAlbedo);
        if (asset3b.thereAO) asset3b.ExtractAO = checkBox("Extract AO", asset3b.ExtractAO);
        if (asset3b.thereAO && asset3b.ExtractAO && !asset3b.SpecularSetupMode) asset3b.MargeAOAndMetallic = checkBox("Marge AO and metallic", asset3b.MargeAOAndMetallic);
        if (asset3b.thereCurvature) asset3b.ExtractCurvature = checkBox("Extract curvature", asset3b.ExtractCurvature);
        if (asset3b.thereEmission) asset3b.ImportEmission = checkBox("Import emission", asset3b.ImportEmission);
        if (asset3b.thereBump) asset3b.CreateNormalMap = checkBox("Create normal map", asset3b.CreateNormalMap);
        if (asset3b.thereBump) asset3b.CreateHeightMap = checkBox("Create height map", asset3b.CreateHeightMap);
        if (asset3b.CreateHeightMap && asset3b.thereBump && !asset3b.SpecularSetupMode && (!asset3b.thereAO || !asset3b.MargeAOAndMetallic)) asset3b.MargeHMAndMetallic = checkBox("Marge HM and metallic", asset3b.MargeHMAndMetallic);
        asset3b.TrianglesWithAlpha = checkBox("Triangles with alpha", asset3b.TrianglesWithAlpha);

        if (asset3b.SpecularSetupMode)
        {
            asset3b.MargeAOAndMetallic = false;
            asset3b.MargeHMAndMetallic = false;
        }

        if (asset3b.thereBump && asset3b.CreateNormalMap) asset3b.NormalMapMultiply = floatField("Normal map multiply", asset3b.NormalMapMultiply);
        if (asset3b.thereBump && asset3b.CreateHeightMap) asset3b.HeightMapMultiply = floatField("Height map multiply", asset3b.HeightMapMultiply);

		asset3b.CreatePadding = checkBox("Create padding", asset3b.CreatePadding);

        drawGUIPos += 10;

        asset3b.InteractiveUpdate = checkBox("Interactive update", asset3b.InteractiveUpdate);

        drawGUIPos += 10;

        if (GUI.Button(new Rect(116 + 10, drawGUIPos, 112, 20), "Apply"))
        {
            asset3b.updateAsset();
        }

        bool fileExist = false;
        string pPath = asset3b.getAssetPath();
        fileExist = File.Exists(pPath);
        if (fileExist)
        {
            if (GUI.Button(new Rect(10, drawGUIPos, 112, 20), "Reload"))
            {

                asset3b.LoadFromFile(pPath);
                asset3b.updateAsset();
            }
        }

//        if (GUI.Button(new Rect(10, drawGUIPos+ 60, 112, 20), "RT"))
  //      {

//            RT3DCoatMaker rt = new RT3DCoatMaker();
  //          rt.CreateFrom3B(asset3b);
    //    }


        showUnpackSettings();
    }

 
    void showUnpackSettings()
    {
        Asset3DCoat asset3b = target as Asset3DCoat;


        bool unpackNow = false;
        bool unpackSep = false;

        drawGUIPos += 40;

        GUI.Box(new Rect(10, drawGUIPos-10, Screen.width - 20, 40),"");

//        if(GUI.Button(new Rect(20, 130, 120, 20), "Unpack here")) unpackNow = true;
        if (GUI.Button(new Rect(20, drawGUIPos, 120, 20), "Unpack to"))
        {
            unpackNow = true;
            unpackSep = true;
        }

        string fName = Path.GetFileName(AssetDatabase.GetAssetPath(asset3b)).Replace(".3b.prefab", "_data").Replace(".prefab", "_data");
        if (asset3b.unPackFolder == "") asset3b.unPackFolder = fName;
        asset3b.unPackFolder = GUI.TextField(new Rect(144, drawGUIPos, Screen.width - 165, 20), asset3b.unPackFolder);


        if (unpackNow)
        {
            fName = asset3b.unPackFolder;
            string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(asset3b));
            if (unpackSep)
            {
//                string fName = Path.GetFileName(AssetDatabase.GetAssetPath(asset3b)).Replace(".3b.prefab", "_data").Replace(".prefab", "_data");

                Application.dataPath.Substring(0, Application.dataPath.Length - 6);
                if(!Directory.Exists(Application.dataPath.Substring(0, Application.dataPath.Length - 6)+path+"/"+fName)) AssetDatabase.CreateFolder(path, fName);
                path += "/" + fName;
            }

            Debug.Log("unpack " + Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(asset3b)) + " to: "+path);

            UnityEngine.Object[] assetObjects = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset3b));

            List<string> unpackedTxtNames = new List<string>();

            string[] txtProps = new string[] { "_MainTex", "_MetallicGlossMap", "_BumpMap", "_ParallaxMap", "_OcclusionMap", "_EmissionMap", "_DetailMask", "_DetailAlbedoMap", "_DetailNormalMap", 
                        "_SpecGlossMap"};

            foreach (UnityEngine.Object iobj in assetObjects)
            {
                if (iobj is Material){
                    Material imtl = new Material((Material)iobj);
                    foreach(string txtProp in txtProps){

                        if (imtl.HasProperty(txtProp) && imtl.GetTexture(txtProp) is Texture2D)
                        {
                            Texture2D itxt = imtl.GetTexture(txtProp) as Texture2D;

                            Byte[] encodedTxt;
                            encodedTxt = itxt.EncodeToPNG();
                            string exportFileNameAsset = path + "/" + itxt.name + ".png";
                            string exportFileName = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + exportFileNameAsset;
                            Debug.Log(exportFileName);

                            FileStream fs = new FileStream(exportFileName, FileMode.OpenOrCreate);
                            fs.Write(encodedTxt,0,encodedTxt.Length);
                            fs.Close();
                            unpackedTxtNames.Add(itxt.name);

                            AssetDatabase.Refresh();
                            AssetDatabase.SaveAssets();

                            Texture2D t = AssetDatabase.LoadAssetAtPath(exportFileNameAsset, typeof(Texture2D)) as Texture2D;
                            imtl.SetTexture(txtProp, t);
                        }
                    }

                    AssetDatabase.CreateAsset(imtl, path + "/" + imtl.name + ".mat");
                }
            }


            foreach (UnityEngine.Object iobj in assetObjects)
            {

                if (iobj is Texture2D && !unpackedTxtNames.Contains(iobj.name))
                {

                    Texture2D itxt = (Texture2D)iobj;

                    Byte[] encodedTxt;
                    encodedTxt = itxt.EncodeToPNG();
                    string exportFileName = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + path + "/" + itxt.name + ".png";

                    File.WriteAllBytes(exportFileName, encodedTxt);

                }

            }
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

        }



    }

    public override void OnInspectorGUI()
    {

        if (Event.current.type == EventType.Layout)
        {
            return;
        }

        Asset3DCoat asset3b = target as Asset3DCoat;

        if (asset3b.SubObjects == null || asset3b.SubObjects.Length == 0) asset3b.updateAsset();

        selectedPageId = GUI.Toolbar(new Rect(20, 60,  Screen.width-50, 22), selectedPageId, new string[] { "Import", "Layers" });

        if (selectedPageId == 0) showImportSettings();
        if (selectedPageId == 1) showLayersSettings();


    }



    public override bool HasPreviewGUI()
    {
        return true;
    }

    /*
      public override void OnPreviewGUI(Rect tarRect, GUIStyle background)
      {
    //	Asset3DCoat exShape = target as Asset3DCoat;
        EditorGUI.DrawRect(tarRect, new Color(0.0f, 1.0f, 0.0f, 1.0f));
      }

        */
    public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int TexWidth, int TexHeight)
    {
        Texture2D staticPreview = LoadPNG(Application.dataPath + "/plugins/3DCoat/3DCoatSettings.png");
        return staticPreview;
    }

}



//////////////// Draw Mesh Maps //////////////////////////////////////


public class meshMapRenderer
{
    Vector3[,] threadsTriBounds1 = new Vector3[10,17000];
    Vector3[,] threadsTriBounds2 = new Vector3[10,17000];

//    Color32[] wnormMap;
    Color32[] albedoMap;
//    Vector2[] uv;
//    Vector3[] normals;

    byte[] meshMask;

//    Asset3DCoat glm;
    MRenderTarget mrt;
//    Mesh mesh;

	Asset3DCoat.ObjMeshData gOMD;

	int iUVID = 0;


	public void drawTrisMask(Asset3DCoat.ObjMeshData omd){
		if (omd.mesh == null)
			return;
//		List<int> nTri1 = new List<int>();
//		List<int> nTri2 = new List<int>();

		omd.mesh.subMeshCount = mrt.asset3DCoat.RTS.Length * 2;

		gOMD = omd;
//		bool resultThereAlpha = false;
		int tcnt = omd.iTriangles.Count / 3;
		for (int tid = 0; tid < tcnt; tid++) if(mrt.asset3DCoat.FacesInfo[omd.iTriangleFaceIds[tid * 3]].RTIdx == iUVID)
		{
			DrawMeshMask(omd.iTriangles[tid*3], omd.iTriangles[tid*3+1], omd.iTriangles[tid*3+2], 0);
		}
			
	}

	public bool analizeTris(Asset3DCoat.ObjMeshData omd){
		if (omd.mesh == null)
			return false;
		List<int> nTri1 = new List<int>();
		List<int> nTri2 = new List<int>();

		omd.mesh.subMeshCount = mrt.asset3DCoat.RTS.Length * 2;

		gOMD = omd;
		bool resultThereAlpha = false;
		int tcnt = omd.iTriangles.Count / 3;
		for (int tid = 0; tid < tcnt; tid++) if(mrt.asset3DCoat.FacesInfo[omd.iTriangleFaceIds[tid * 3]].RTIdx == iUVID)
		{
			if(tid % 1000 == 1)EditorUtility.DisplayProgressBar("Make materials", "Find triangles with alpha", (float)tid/(float)tcnt);
			// bool thereAlpha = getThereAlpha(triangles[tid*3], triangles[tid*3+1], triangles[tid*3+2], 0);
			bool thereAlpha = mrt.asset3DCoat.TrianglesWithAlpha ? getThereAlpha2(omd.iTriangles[tid*3], omd.iTriangles[tid*3+1], omd.iTriangles[tid*3+2], 0) : false;
			//     bool thereAlpha = DrawTriangle(triangles[tid*3], triangles[tid*3+1], triangles[tid*3+2], 0);
			if (thereAlpha) resultThereAlpha = true;
//			DrawMeshMask(omd.iTriangles[tid*3], omd.iTriangles[tid*3+1], omd.iTriangles[tid*3+2], 0);
			if (thereAlpha)
			{
				nTri2.Add(omd.iTriangles[tid * 3]);
				nTri2.Add(omd.iTriangles[tid * 3 + 1]);
				nTri2.Add(omd.iTriangles[tid * 3 + 2]);
			}
			else
			{
				nTri1.Add(omd.iTriangles[tid * 3]);
				nTri1.Add(omd.iTriangles[tid * 3 + 1]);
				nTri1.Add(omd.iTriangles[tid * 3 + 2]);
			}
		}



		omd.mesh.SetTriangles(nTri1.ToArray(), iUVID);
		omd.mesh.SetTriangles(nTri2.ToArray(), mrt.asset3DCoat.RTS.Length + iUVID);

		return resultThereAlpha;

	}

	public bool AnalyzeMRTAlpha(MRenderTarget amrt){
		mrt = amrt;
//		glm = mrt.asset3DCoat;

		bool objThereAlpha = false;

		//        meshMask = new byte[mrt.rtSizeX * mrt.rtSizeY];
		//       for (int i = 0; i < meshMask.Length; i++) meshMask[i] = 0;
		albedoMap = mrt.Albedo.GetPixels32();
		iUVID = 0;

		for (int i = 0; i < mrt.asset3DCoat.RTS.Length; i++) if (mrt.asset3DCoat.RTS[i] == mrt) iUVID = i; 

		for (int oid = 0; oid < mrt.asset3DCoat.objectsMeshData.Length; oid++) {
			if (mrt.asset3DCoat.objectsMeshData [oid].subObjects.Count > 0) {
				for (int soid = 0; soid < mrt.asset3DCoat.objectsMeshData[oid].subObjects.Count; soid++) {
					if (analizeTris (mrt.asset3DCoat.objectsMeshData [oid].subObjects [soid]))
						objThereAlpha = true;
				}

			} else {
				if (analizeTris (mrt.asset3DCoat.objectsMeshData [oid]))
					objThereAlpha = true;
			}
		}




		//            mrt.meshMask = meshMask;

		return objThereAlpha;

	}

	public bool DrawMRTMeshMask(MRenderTarget amrt){
		mrt = amrt;
//		glm = mrt.asset3DCoat;

		bool objThereAlpha = false;

		meshMask = new byte[mrt.rtSizeX * mrt.rtSizeY];
		for (int i = 0; i < meshMask.Length; i++) meshMask[i] = 0;
		iUVID = 0;

		for (int i = 0; i < mrt.asset3DCoat.RTS.Length; i++) if (mrt.asset3DCoat.RTS[i] == mrt) iUVID = i; 

		for (int oid = 0; oid < mrt.asset3DCoat.objectsMeshData.Length; oid++) {
			if (mrt.asset3DCoat.objectsMeshData [oid].subObjects.Count > 0) {
				for (int soid = 0; soid < mrt.asset3DCoat.objectsMeshData[oid].subObjects.Count; soid++) {
					drawTrisMask(mrt.asset3DCoat.objectsMeshData [oid].subObjects [soid]);
				}

			} else {
				drawTrisMask (mrt.asset3DCoat.objectsMeshData [oid]);
			}
		}




        mrt.meshMask = meshMask;

		return objThereAlpha;

	}

    void Draw2DTriBound(Vector2 p1, Vector2 p2, Vector3 n1, Vector3 n2, int threadId)
    {


        Vector3[,] pxBounds1 = threadsTriBounds1;
        Vector3[,] pxBounds2 = threadsTriBounds2;

        Vector2 ip1;
        Vector2 ip2;
        Vector3 in1;
        Vector3 in2;


        if (p1.y > p2.y){
            ip1 = p2;
            ip2 = p1;
            in1 = n2;
            in2 = n1;
        }
        else {
            ip1 = p1;
            ip2 = p2;
            in1 = n1;
            in2 = n2;
        }

        float posX = ip1.x;
        Vector3 posN = in1;
        float yDist = Mathf.Abs(ip2.y - ip1.y);
        Vector3[,] ixBounds;
        if (p2.y > p1.y) ixBounds = pxBounds1;
        else ixBounds = pxBounds2;

        int stepCount = (int)ip2.y;
        float xStep = (ip2.x - posX) / yDist;
        Vector3 nStep = (in2 - posN) / yDist;

        for (int posY = Mathf.FloorToInt(ip1.y); posY < stepCount; posY++) if(posY >= 0 && posY < 64000){
            ixBounds[threadId,posY] = posN;
            ixBounds[threadId,posY].z = posX;
            posX += xStep;
            posN = posN + nStep;
        }

    }


    void DrawMeshMask(int iP1, int iP2, int iP3, int threadId)
    {

        int iTxtWidth = mrt.rtSizeX;
        int iTxtHeight = mrt.rtSizeY;
        float fTxtWidth = iTxtWidth;
        float fTxtHeight = iTxtHeight;

        Vector2 trianglep1 = new Vector2(gOMD.uvs[iP1].x*fTxtWidth, (1.0f - gOMD.uvs[iP1].y)*fTxtHeight);
        Vector2 trianglep2 = new Vector2(gOMD.uvs[iP2].x*fTxtWidth, (1.0f - gOMD.uvs[iP2].y)*fTxtHeight);
        Vector2 trianglep3 = new Vector2(gOMD.uvs[iP3].x*fTxtWidth, (1.0f - gOMD.uvs[iP3].y)*fTxtHeight);
     


//        int boundsMinX = Math.Max(1, Math.Min(Math.Min((int)trianglep1.x, (int)trianglep2.x), (int)trianglep3.x) - 1);
        int boundsMinY = Math.Max(1, Math.Min(Math.Min((int)trianglep1.y, (int)trianglep2.y), (int)trianglep3.y) - 1);
//        int boundsMaxX = Math.Min(iTxtWidth - 2, Math.Max(Math.Max((int)trianglep1.x, (int)trianglep2.x), (int)trianglep3.x) + 1);
        int boundsMaxY = Math.Min(iTxtHeight - 2, Math.Max(Math.Max((int)trianglep1.y, (int)trianglep2.y), (int)trianglep3.y) + 1);

        if(boundsMaxY < 0 || boundsMinY > fTxtHeight) return;

        Draw2DTriBound(trianglep1, trianglep2, Vector3.zero, Vector3.right, threadId);
        Draw2DTriBound(trianglep2, trianglep3, Vector3.right, Vector3.up, threadId);
        Draw2DTriBound(trianglep3, trianglep1, Vector3.up, Vector3.zero, threadId);

        Vector3[,] iBoundsLeft;
        Vector3[,] iBoundsRight;
        int iCenterY = (boundsMinY + boundsMaxY) / 2;
        if (threadsTriBounds1[threadId, iCenterY].z < threadsTriBounds2[threadId, iCenterY].z){
            iBoundsLeft = threadsTriBounds1;
            iBoundsRight = threadsTriBounds2;
        }
        else {
            iBoundsLeft = threadsTriBounds2;
            iBoundsRight = threadsTriBounds1;
        }



        for (int iy = boundsMinY + 1; iy < boundsMaxY - 1; ++iy){
            int yidx = (iTxtHeight-1-iy)*iTxtWidth;

            int xFrom;
            int xTo;

            xFrom = (int)iBoundsLeft[threadId,iy].z;
            xTo = (int)iBoundsRight[threadId,iy].z;

            for (int ix = xFrom; ix < xTo; ix++){
                int idx = yidx + ix;
                meshMask[idx] = 1;
 //               wnormMap[idx].r = 255;
            }
        }

    }

    bool DrawTriangle(int iP1, int iP2, int iP3, int threadId)
    {

        bool thereAlpha = false;

//		Vector3 cNorm = (gOMD.norms[iP1] + gOMD.norms[iP2] + gOMD.norms[iP3]) / 3.0f;



        int iTxtWidth = mrt.rtSizeX;
        int iTxtHeight = mrt.rtSizeY;
        float fTxtWidth = iTxtWidth;
        float fTxtHeight = iTxtHeight;

		Vector2 trianglep1 = new Vector2(gOMD.uvs[iP1].x * fTxtWidth, (1.0f - gOMD.uvs[iP1].y) * fTxtHeight);
		Vector2 trianglep2 = new Vector2(gOMD.uvs[iP2].x * fTxtWidth, (1.0f - gOMD.uvs[iP2].y) * fTxtHeight);
		Vector2 trianglep3 = new Vector2(gOMD.uvs[iP3].x * fTxtWidth, (1.0f - gOMD.uvs[iP3].y) * fTxtHeight);



  //      int boundsMinX = Math.Max(1, Math.Min(Math.Min((int)trianglep1.x, (int)trianglep2.x), (int)trianglep3.x) - 1);
        int boundsMinY = Math.Max(1, Math.Min(Math.Min((int)trianglep1.y, (int)trianglep2.y), (int)trianglep3.y) - 1);
//        int boundsMaxX = Math.Min(iTxtWidth - 2, Math.Max(Math.Max((int)trianglep1.x, (int)trianglep2.x), (int)trianglep3.x) + 1);
        int boundsMaxY = Math.Min(iTxtHeight - 2, Math.Max(Math.Max((int)trianglep1.y, (int)trianglep2.y), (int)trianglep3.y) + 1);

        if (boundsMaxY < 0 || boundsMinY > fTxtHeight) return false;

        Draw2DTriBound(trianglep1, trianglep2, Vector3.zero, Vector3.right, threadId);
        Draw2DTriBound(trianglep2, trianglep3, Vector3.right, Vector3.up, threadId);
        Draw2DTriBound(trianglep3, trianglep1, Vector3.up, Vector3.zero, threadId);

        Vector3[,] iBoundsLeft;
        Vector3[,] iBoundsRight;
        int iCenterY = (boundsMinY + boundsMaxY) / 2;
        if (threadsTriBounds1[threadId, iCenterY].z < threadsTriBounds2[threadId, iCenterY].z)
        {
            iBoundsLeft = threadsTriBounds1;
            iBoundsRight = threadsTriBounds2;
        }
        else
        {
            iBoundsLeft = threadsTriBounds2;
            iBoundsRight = threadsTriBounds1;
        }



        for (int iy = boundsMinY + 1; iy < boundsMaxY - 1; ++iy)
        {
            int yidx = (iTxtHeight - 1 - iy) * iTxtWidth;

            int xFrom;
            int xTo;
            Vector3 nPos;
            Vector3 nStep;

            xFrom = (int)iBoundsLeft[threadId, iy].z;
            xTo = (int)iBoundsRight[threadId, iy].z;

            nPos = iBoundsLeft[threadId, iy];
            nStep = (iBoundsRight[threadId, iy] - iBoundsLeft[threadId, iy]) / (float)(xTo - xFrom);

//            Vector3 halfV3 = new Vector3(0.5f, 0.5f, 0.5f);
            for (int ix = xFrom; ix <= xTo; ix++)
            {

                int idx = yidx + ix;

                float ippX = nPos.x;
                float ippY = nPos.y;
                float ve3 = 1.0f - (ippX + ippY);

				Vector3 vnorm = (gOMD.norms[iP1] * ve3
					+ gOMD.norms[iP2] * ippX
					+ gOMD.norms[iP3] * ippY);///2.0f+halfV3;

                vnorm = Vector3.Cross(vnorm, new Vector3(vnorm.x, vnorm.y + 0.5f, vnorm.z));
//                wnormMap[idx] = new Color(vnorm.x / 2.0f + 0.5f, vnorm.y / 2.0f + 0.5f, vnorm.z / 2.0f + 0.5f);
                if (albedoMap[idx].a < 128) thereAlpha = true;

                nPos = nPos + nStep;
            }
        }

        return thereAlpha;

    }

	bool getThereAlpha2(int iP1, int iP2, int iP3, int threadId)
	{

		bool thereAlpha = false;

//		Vector3 cNorm = (gOMD.norms[iP1] + gOMD.norms[iP2] + gOMD.norms[iP3]) / 3.0f;



		int iTxtWidth = mrt.rtSizeX;
		int iTxtHeight = mrt.rtSizeY;
		float fTxtWidth = iTxtWidth;
		float fTxtHeight = iTxtHeight;

		Vector2 trianglep1 = new Vector2(gOMD.uvs[iP1].x * fTxtWidth, (1.0f - gOMD.uvs[iP1].y) * fTxtHeight);
		Vector2 trianglep2 = new Vector2(gOMD.uvs[iP2].x * fTxtWidth, (1.0f - gOMD.uvs[iP2].y) * fTxtHeight);
		Vector2 trianglep3 = new Vector2(gOMD.uvs[iP3].x * fTxtWidth, (1.0f - gOMD.uvs[iP3].y) * fTxtHeight);



//		int boundsMinX = Math.Max(1, Math.Min(Math.Min((int)trianglep1.x, (int)trianglep2.x), (int)trianglep3.x) - 1);
		int boundsMinY = Math.Max(1, Math.Min(Math.Min((int)trianglep1.y, (int)trianglep2.y), (int)trianglep3.y) - 1);
//		int boundsMaxX = Math.Min(iTxtWidth - 2, Math.Max(Math.Max((int)trianglep1.x, (int)trianglep2.x), (int)trianglep3.x) + 1);
		int boundsMaxY = Math.Min(iTxtHeight - 2, Math.Max(Math.Max((int)trianglep1.y, (int)trianglep2.y), (int)trianglep3.y) + 1);

		if (boundsMaxY < 0 || boundsMinY > fTxtHeight) return false;

		Draw2DTriBound(trianglep1, trianglep2, Vector3.zero, Vector3.right, threadId);
		Draw2DTriBound(trianglep2, trianglep3, Vector3.right, Vector3.up, threadId);
		Draw2DTriBound(trianglep3, trianglep1, Vector3.up, Vector3.zero, threadId);

		Vector3[,] iBoundsLeft;
		Vector3[,] iBoundsRight;
		int iCenterY = (boundsMinY + boundsMaxY) / 2;
		if (threadsTriBounds1[threadId, iCenterY].z < threadsTriBounds2[threadId, iCenterY].z)
		{
			iBoundsLeft = threadsTriBounds1;
			iBoundsRight = threadsTriBounds2;
		}
		else
		{
			iBoundsLeft = threadsTriBounds2;
			iBoundsRight = threadsTriBounds1;
		}



		for (int iy = boundsMinY + 1; iy < boundsMaxY - 1; ++iy)
		{
			int yidx = (iTxtHeight - 1 - iy) * iTxtWidth;

			int xFrom;
			int xTo;
//			Vector3 nPos;
//			Vector3 nStep;

			xFrom = (int)iBoundsLeft[threadId, iy].z;
			xTo = (int)iBoundsRight[threadId, iy].z;

//			nPos = iBoundsLeft[threadId, iy];
//			nStep = (iBoundsRight[threadId, iy] - iBoundsLeft[threadId, iy]) / (float)(xTo - xFrom);

//			Vector3 halfV3 = new Vector3(0.5f, 0.5f, 0.5f);
			for (int ix = xFrom; ix <= xTo; ix++)
			{

				int idx = yidx + ix;
				if (albedoMap[idx].a < 128) thereAlpha = true;
			}
		}

		return thereAlpha;

	}

    bool getThereAlpha(int iP1, int iP2, int iP3, int threadId)
    {


  

        int iTxtWidth = mrt.rtSizeX;
        int iTxtHeight = mrt.rtSizeY;
        float fTxtWidth = iTxtWidth;
        float fTxtHeight = iTxtHeight;

        Vector2 trianglep1 = new Vector2(gOMD.uvs[iP1].x * fTxtWidth, gOMD.uvs[iP1].y * fTxtHeight);
        Vector2 trianglep2 = new Vector2(gOMD.uvs[iP2].x * fTxtWidth, gOMD.uvs[iP2].y * fTxtHeight);
        Vector2 trianglep3 = new Vector2(gOMD.uvs[iP3].x * fTxtWidth, gOMD.uvs[iP3].y * fTxtHeight);
        Vector2 trianglepc = (trianglep1 + trianglep2 + trianglep3)/3.0f;

        Vector2 trianglep1c = (trianglep1 * 3.0f + trianglep2 + trianglep3) / 5.0f;
        Vector2 trianglep2c = (trianglep1 + trianglep2 * 3.0f + trianglep3) / 5.0f;
        Vector2 trianglep3c = (trianglep1 + trianglep2 + trianglep3 * 3.0f) / 5.0f;

	    if (albedoMap[(Mathf.FloorToInt(trianglep1c.y) % iTxtHeight) * iTxtWidth + (Mathf.FloorToInt(trianglep1c.x) % iTxtWidth)].a < 128) return true;
        if (albedoMap[(Mathf.FloorToInt(trianglep2c.y) % iTxtHeight) * iTxtWidth + (Mathf.FloorToInt(trianglep2c.x) % iTxtWidth)].a < 128) return true;
        if (albedoMap[(Mathf.FloorToInt(trianglep3c.y) % iTxtHeight) * iTxtWidth + (Mathf.FloorToInt(trianglep3c.x) % iTxtWidth)].a < 128) return true;
        if (albedoMap[(Mathf.FloorToInt(trianglepc.y) % iTxtHeight) * iTxtWidth + (Mathf.FloorToInt(trianglepc.x) % iTxtWidth)].a < 128) return true;


        return false;

    }


}


//////////////////////////////////////////////////////////////////////

/*

public class Link3DCoat : MonoBehaviour
{

    
    public Asset3DCoat asset3DCoat;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
*/

