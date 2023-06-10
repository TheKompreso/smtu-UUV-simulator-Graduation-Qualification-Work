using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class CameraTexture : MonoBehaviour
{
	public Camera FrontCamera;
	public RawImage UIRawImg;
	private Texture2D cameraTexture2D;
	private int counter = 0; // Счётчик кадров

	void Start()
	{
        // Передаём текстуру камеры в наш плагин
        cameraTexture2D = new Texture2D(256, 256, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Point
        };
        cameraTexture2D.Apply(); // Помещаем текстуру в GPU
		UIRawImg.texture = cameraTexture2D;
		// Передача идентификатора текстуры в .dll 
		#if UNITY_GLES_RENDERER
			SetTextureOfCamera(cam1Tex.GetNativeTexturePtr(), cam1Tex.width, cam1Tex.height);
		#else
			SetTextureOfCamera(cameraTexture2D.GetNativeTexturePtr());
		#endif
	}

	private void LateUpdate()
    {
		RenderTexture texture = FrontCamera.targetTexture;
		RenderTexture.active = texture;
		cameraTexture2D.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
		cameraTexture2D.Apply(); // Помещаем текстуру в GPU
		RenderTexture.active = null;

		GL.IssuePluginEvent(GetRenderEventFunc(), counter++);
	}

	#if UNITY_IPHONE && !UNITY_EDITOR
		[DllImport ("__Internal")]
	#else
		[DllImport("OpenCVCode")] // Наш dll файл, созданный в Visual Studio
	#endif
	#if UNITY_GLES_RENDERER
		private static extern void SetTextureOfCamera(System.IntPtr texture, int w, int h);
	#else
	private static extern void SetTextureOfCamera(System.IntPtr texture);
	#endif

	#if UNITY_IPHONE && !UNITY_EDITOR
		[DllImport ("__Internal")]
	#else
		[DllImport("OpenCVCode")]
	#endif
	private static extern IntPtr GetRenderEventFunc();
}
