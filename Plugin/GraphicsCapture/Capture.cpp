#include "pch.h"
#include "Capture.h"

namespace winrt
{
	using namespace Windows::Foundation;
	using namespace Windows::Graphics;
	using namespace Windows::Graphics::Capture;
	using namespace Windows::Graphics::DirectX;
	using namespace Windows::Graphics::DirectX::Direct3D11;
}

Capture::Capture(winrt::IDirect3DDevice const& device, winrt::GraphicsCaptureItem const& item, winrt::DirectXPixelFormat pixelFormat)
{
	m_item = item;
	m_device = device;
	m_pixelFormat = pixelFormat;

	m_framePool = winrt::Direct3D11CaptureFramePool::Create(m_device, m_pixelFormat, 2, m_item.Size());
	m_session = m_framePool.CreateCaptureSession(m_item);
	m_lastSize = m_item.Size();
	m_framePool.FrameArrived({ this, &Capture::OnFrameArrived });

	WINRT_ASSERT(m_session != nullptr);
}


void Capture::StartCapture()
{
	m_session.StartCapture();
}

void Capture::Close()
{
	auto expected = false;
	m_session.Close();
	m_framePool.Close();

	m_framePool = nullptr;
	m_session = nullptr;
	m_item = nullptr;
}
void Capture::OnFrameArrived(winrt::Direct3D11CaptureFramePool const& sender, winrt::IInspectable const&)
{
	auto frame = sender.TryGetNextFrame();

	auto tex = GetDXGIInterfaceFromObject<ID3D11Texture2D>(frame.Surface());

	auto const contentSize = frame.ContentSize();

	if ((contentSize.Width != m_lastSize.Width) ||
		(contentSize.Height != m_lastSize.Height))
	{
		m_lastSize = contentSize;
		m_framePool.Recreate(m_device, m_pixelFormat, 2, m_lastSize);
	}

	m_latestTexture = tex;
}

int Capture::GetWidth()
{
	return m_lastSize.Width;
}

int Capture::GetHeight()
{
	return m_lastSize.Height;
}

void* Capture::GetTexturePtr()
{
	return m_latestTexture.get();
}