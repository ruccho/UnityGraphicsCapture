#pragma once

class Capture
{
public:
	Capture(
		winrt::Windows::Graphics::DirectX::Direct3D11::IDirect3DDevice const& device,
		winrt::Windows::Graphics::Capture::GraphicsCaptureItem const& item,
		winrt::Windows::Graphics::DirectX::DirectXPixelFormat pixelFormat
	);
	void StartCapture();
	void Close();
	int GetWidth();
	int GetHeight();
	void* GetTexturePtr();

private:
	void OnFrameArrived(
		winrt::Windows::Graphics::Capture::Direct3D11CaptureFramePool const& sender,
		winrt::Windows::Foundation::IInspectable const& args);

	winrt::Windows::Graphics::Capture::GraphicsCaptureItem m_item{ nullptr };
	winrt::Windows::Graphics::DirectX::Direct3D11::IDirect3DDevice m_device{ nullptr };
	winrt::Windows::Graphics::DirectX::DirectXPixelFormat m_pixelFormat;

	winrt::Windows::Graphics::Capture::Direct3D11CaptureFramePool m_framePool{ nullptr };
	winrt::Windows::Graphics::Capture::GraphicsCaptureSession m_session{ nullptr };
	winrt::Windows::Graphics::SizeInt32 m_lastSize;

	winrt::com_ptr<ID3D11Texture2D> m_latestTexture;
};

