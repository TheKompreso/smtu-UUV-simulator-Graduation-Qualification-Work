Shader "Unlit/TextureGeneration"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _DeepColor ("Deep Color", Color) = (0, 0, 0.5, 1)
        _ShallowColor ("Shallow Color", Color) = (0.09, 0.09, 0.58, 1)
        _SandColor ("Sand Color", Color) = (0, 0, 0.5, 1)
        _GrassColor ("Grass Color", Color) = (0, 0, 0.5, 1)
        _ForestColor ("Forest Color", Color) = (0, 0, 0.5, 1)
        _RockColor ("Rock Color", Color) = (0, 0, 0.5, 1)
        _SnowColor ("Snow Color", Color) = (1, 1, 1, 1)

        _DeepHeight ("Deep Height", Range(0,50)) = 0.02
        _ShallowHeight ("Shallow Height", Range(0,50)) = 0.04
        _SandHeight ("Sand Height", Range(0,50)) = 0.06
        _GrassHeight ("Grass Height", Range(0,50)) = 0.4
        _ForestHeight ("Forest Height", Range(0,50)) = 0.6
        _RockHeight ("Rock Height", Range(0,50)) = 0.95

        _SmoothTransitionDeep      ("Smooth Transition Deep",Float) = 100
        _SmoothTransitionShallow   ("Smooth Transition Shallow",Float) =100
        _SmoothTransitionSand      ("Smooth Transition Sand",Float) =500
        _SmoothTransitionGrass     ("Smooth Transition Grass",Float) =100
        _SmoothTransitionForest    ("Smooth Transition Forest",Float) =1000
        _SmoothTransitionRock      ("Smooth Transition Rock",Float) =10

        _HeightScale("Height Scale", float) = 1
        _MinHeight("Min Height", float) = 1

        
		 // ƒиффузный
		_Diffuse ("Diffuse", COLOR) = (1,1,1,1)
 
		 // Ёффект обводки
		_OutlineColor ("Outline Color", COLOR) = (0,0,0,1)
		_OutlineScale ("Outline Scale", Range(0,1)) = 0.001
        _LamberdScale("Lamberd Scale", Range(0,1)) = 0.5

        [HideInInspector] _PreviousColor("_PreviousColor", COLOR) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue" = "Geometry+1000" "RenderType"="Opaque" }
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        
 
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
 
            struct v2f
            {
                float3 normal : NORMAL;
                float4 color : COLOR;
                float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldNormal:TEXCOORD1;
				float3 worldPos:TEXCOORD2;
            };
 
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Diffuse;

            float4 _DeepColor;
            float4 _ShallowColor;
            float4 _SandColor;
            float4 _GrassColor;
            float4 _ForestColor;
            float4 _RockColor;
            float4 _SnowColor;

            float _DeepHeight;
            float _ShallowHeight;
            float _SandHeight;
            float _GrassHeight;
            float _ForestHeight;
            float _RockHeight;

            float _HeightScale;
            float _MinHeight;
            float _LamberdScale;

            
            float _SmoothTransitionDeep;
            float _SmoothTransitionShallow;
            float _SmoothTransitionSand;
            float _SmoothTransitionGrass;
            float _SmoothTransitionForest;
            float _SmoothTransitionRock;

            float4 _PreviousColor;
            
            float4 GetColor(float height)
            {
                float4 res = float4(1,1,1,1);
                height -= _MinHeight;
                if(height < _DeepHeight) res = lerp(
                _ShallowColor,
                _DeepColor,
                clamp((_DeepHeight - height) * _SmoothTransitionDeep,0,1));

                else if(height <= _ShallowHeight) res = lerp(
                _SandColor,
                _ShallowColor,
                clamp((_ShallowHeight - height) * _SmoothTransitionShallow,0,1));

                else if(height <= _SandHeight) res = lerp(
                    _GrassColor,
                    _SandColor,
                    sqrt(clamp((_SandHeight - height) * _SmoothTransitionSand, 0, 1)));

                else if(height <= _GrassHeight) res = lerp(
                    _ForestColor,
                    _GrassColor,
                    (clamp((_GrassHeight - height) * _SmoothTransitionGrass, 0, 1)));

                else if(height <= _ForestHeight) res = lerp(
                    _RockColor,
                    _ForestColor,
                    (clamp((_ForestHeight - height) * _SmoothTransitionForest, 0, 1)));

                else if(height <= _RockHeight) res = lerp(
                    _SnowColor,
                    _RockColor,
                    (clamp((_RockHeight - height) * _SmoothTransitionRock, 0, 1)));

                else res = _SnowColor;
                return res;
            }
            v2f vert (appdata_base v)
            {

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld,v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
                //o.uv = v.uv;
                float4 newColor = GetColor(v.vertex.y / _HeightScale);
                o.color = newColor;
                _PreviousColor = newColor;
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target
            {
				 // окружающий свет
				float3 ambient = UNITY_LIGHTMODEL_AMBIENT;
               
				 // »стинный цвет текстуры
                fixed3 albedo = i.color.rgb;
 
				 // ƒиффузный
				fixed3 worldLightDir = UnityWorldSpaceLightDir(i.worldPos);
				float halfLambert = dot(worldLightDir,i.worldNormal) * _LamberdScale + (1 - _LamberdScale);
 
				 // финальное диффузное отражение
				fixed3 diffuse = _LightColor0.rgb * albedo  * /*_Diffuse.rgb */ halfLambert;
 
                //return i.color * _OutlineColor;
                return fixed4(ambient+diffuse,1);
            }
            ENDCG
        }
    }
    FallBack "DIFFUSE"
}