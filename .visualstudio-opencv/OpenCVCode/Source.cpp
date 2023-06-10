#include <math.h>
#include <stdio.h>
#include <vector>
#include <string>

#include "Unity/IUnityGraphics.h"

#include <opencv2/opencv.hpp>

#include "Header.h"

#if SUPPORT_D3D11
#include <d3d11.h>
#include "Unity/IUnityGraphicsD3D11.h"
#endif
#if SUPPORT_D3D12
#include <d3d12.h>
#include "Unity/IUnityGraphicsD3D12.h"
#endif

#if SUPPORT_OPENGLES
#if UNITY_IPHONE
#include <OpenGLES/ES2/gl.h>
#elif UNITY_ANDROID
#include <GLES2/gl2.h>
#endif
#elif SUPPORT_OPENGL
#if UNITY_WIN || UNITY_LINUX
#include <GL/gl.h>
#else
#include <OpenGL/gl.h>
#endif
#endif

using namespace cv;

// Prints a string
static void DebugLog(const char* str)
{
#if UNITY_WIN
	OutputDebugStringA(str);
#else
	fprintf(stderr, "%s", str);
#endif
}



//переменные для хранения идентификатора текстуры
static void* g_Cam1TexturePointer = NULL;
#ifdef SUPPORT_OPENGLES
static int   g_TexWidth = 0;
static int   g_TexHeight = 0;
#endif

//Определение функции передачи идентификатора текстуры.
//Объявление находится в C# скрипте
#ifdef SUPPORT_OPENGLES
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetTextureOfCamera(void* texturePtr, int w, int h)
#else
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetTextureOfCamera(void* texturePtr)
#endif
{
	g_Cam1TexturePointer = texturePtr;
#ifdef SUPPORT_OPENGLES
	g_TexWidth = w;
	g_TexHeight = h;
#endif
}

static int counter = 0;

int ImageProcessing(Mat& I)
{
	const int channels = I.channels();
	const int rows = I.rows;
	const int cols = I.cols;

	int row;
	for (row = 0; row < rows; row++) // Обходим все строки. 
	{
		uchar* p = I.ptr<uchar>(row); // Указатель на начало i строки матрицы нужного типа 
		int col, r, g, b;
		for (col = 0; col < cols; col++)
		{
			r = col * channels + 0;
			g = col * channels + 1;
			b = col * channels + 2;
			
			if ((p[r] > p[g]) && (p[r] > p[b]))
			{
				if ((p[r] - p[g] < 22) && (p[r] - p[b]) < 22)
				{
					p[r] = 255;
					p[g] = 255;
					p[b] = 255;
				}
				else
				{
					p[r] = 0;
					p[g] = 0;
					p[b] = 255;
				}
			}
			else if (p[r] < 10 && p[g] < 10 && p[b] < 10) {
				p[r] = 255;
				p[g] = 0;
				p[b] = 0;
			}
			else
			{
				p[r] = 255-p[r] / 4;
				p[g] = 255-p[g] / 4;
				p[b] = 255-p[b] / 4;
			}
		}
	}
	return 0;
}

static void UNITY_INTERFACE_API OnRenderEvent(int eventID) {
	#if SUPPORT_OPENGL
		if (g_Cam1TexturePointer) {

			GLuint gltex = (GLuint)(size_t)(g_Cam1TexturePointer);
			glBindTexture(GL_TEXTURE_2D, gltex);
			int texWidth, texHeight;
			glGetTexLevelParameteriv(GL_TEXTURE_2D, 0, GL_TEXTURE_WIDTH, &texWidth);
			glGetTexLevelParameteriv(GL_TEXTURE_2D, 0, GL_TEXTURE_HEIGHT, &texHeight);

			// Матрица OpenCV, в которую будет считана текстура
			cv::Mat frame(texHeight, texWidth, CV_8UC4);

			// Считывание текстуры в матрицу -- ТУТ ПОЛУЧАЕТСЯ ПОЛУЧАЕМ ИЗОБРАЖЕНИЕ
			glGetTexImage(GL_TEXTURE_2D, 0, GL_RGBA, GL_UNSIGNED_BYTE, frame.data);

			// Обрабатываем изображение
			ImageProcessing(frame); 

			// Экспортируем изображение в файл
			if (counter++ == 50)
			{
				counter = 0;
				imwrite("output-for-debug.jpg", frame);
			}

			// Загружаем назад в память текстуру, чтобы отобразить ее в Unity
			glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, texWidth, texHeight, GL_RGBA, GL_UNSIGNED_BYTE, frame.data);
		}
	#endif
}

// --------------------------------------------------------------------------
// UnitySetInterfaces

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);

static IUnityInterfaces* s_UnityInterfaces = NULL;
static IUnityGraphics* s_Graphics = NULL;
static UnityGfxRenderer s_DeviceType = kUnityGfxRendererNull;

extern "C" void	UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces * unityInterfaces)
{
	DebugLog("--- UnityPluginLoad");
	s_UnityInterfaces = unityInterfaces;
	s_Graphics = s_UnityInterfaces->Get<IUnityGraphics>();
	s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);

	// Run OnGraphicsDeviceEvent(initialize) manually on plugin load
	OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
	s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

// --------------------------------------------------------------------------
// GraphicsDeviceEvent

// Actual setup/teardown functions defined below
#if SUPPORT_D3D11
static void DoEventGraphicsDeviceD3D11(UnityGfxDeviceEventType eventType);
#endif
#if SUPPORT_D3D12
static void DoEventGraphicsDeviceD3D12(UnityGfxDeviceEventType eventType);
#endif
#if SUPPORT_OPENGLES
static void DoEventGraphicsDeviceGLES(UnityGfxDeviceEventType eventType);
#endif

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
	UnityGfxRenderer currentDeviceType = s_DeviceType;

	switch (eventType)
	{
	case kUnityGfxDeviceEventInitialize:
	{
		DebugLog("OnGraphicsDeviceEvent(Initialize).\n");
		s_DeviceType = s_Graphics->GetRenderer();
		currentDeviceType = s_DeviceType;
		break;
	}

	case kUnityGfxDeviceEventShutdown:
	{
		DebugLog("OnGraphicsDeviceEvent(Shutdown).\n");
		s_DeviceType = kUnityGfxRendererNull;
		g_Cam1TexturePointer = NULL;
		break;
	}

	case kUnityGfxDeviceEventBeforeReset:
	{
		DebugLog("OnGraphicsDeviceEvent(BeforeReset).\n");
		break;
	}

	case kUnityGfxDeviceEventAfterReset:
	{
		DebugLog("OnGraphicsDeviceEvent(AfterReset).\n");
		break;
	}
	};


#if SUPPORT_D3D11
	if (currentDeviceType == kUnityGfxRendererD3D11)
		DoEventGraphicsDeviceD3D11(eventType);
#endif

#if SUPPORT_D3D12
	if (currentDeviceType == kUnityGfxRendererD3D12)
		DoEventGraphicsDeviceD3D12(eventType);
#endif

#if SUPPORT_OPENGLES
	if (currentDeviceType == kUnityGfxRendererOpenGLES20 ||
		currentDeviceType == kUnityGfxRendererOpenGLES30)
		DoEventGraphicsDeviceGLES(eventType);
#endif
}

void DoEventGraphicsDeviceD3D11(UnityGfxDeviceEventType eventType)
{
}

void DoEventGraphicsDeviceD3D12(UnityGfxDeviceEventType eventType)
{
}

void DoEventGraphicsDeviceGLES(UnityGfxDeviceEventType eventType)
{
}

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc()
{
	return OnRenderEvent;
}