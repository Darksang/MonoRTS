sampler TextureSampler : register(s0);

float4 OutlineColor;
float2 TextureRes;

struct VSOutput {
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

float4 PixelShaderFunction(VSOutput Input) : COLOR0 {
    float4 Pixel = tex2D(TextureSampler, Input.TexCoord);

    float2 OffsetX = float2((1 / TextureRes.x), 0);
    float2 OffsetY = float2(0, (1 / TextureRes.y));

    Pixel.rgb *= Pixel.a;

    float4 Outline = OutlineColor;
    Outline.a *= ceil(Pixel.a);
    Outline.rgb *= Outline.a;

    float UpAlpha = tex2D(TextureSampler, Input.TexCoord + OffsetY).a;
    float DownAlpha = tex2D(TextureSampler, Input.TexCoord - OffsetY).a;
    float RightAlpha = tex2D(TextureSampler, Input.TexCoord + OffsetX).a;
    float LeftAlpha = tex2D(TextureSampler, Input.TexCoord - OffsetX).a;

    return lerp(Outline, Pixel, ceil(UpAlpha * DownAlpha * RightAlpha * LeftAlpha));
}

technique Outline {
    pass {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}