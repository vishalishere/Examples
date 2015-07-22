///////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////
// PictureCtrl.cpp
// 
// Author: Tobias Eiseler
//
// E-Mail: tobias.eiseler@sisternicky.com
// 
// Function: A MFC Picture Control to display
//           an image on a Dialog, etc.
///////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////

#include "StdAfx.h"
#include "PictureCtrl.h"
#include "math.h"

#include <GdiPlus.h>
using namespace Gdiplus;

//Macro to release COM Components

#ifdef SAFE_RELEASE
#undef SAFE_RELEASE
#endif
#define SAFE_RELEASE(x) do{\
							if((x) != NULL)\
							{\
								while((x)->Release() != 0);\
								(x) = NULL;\
							}\
						}while(0)

CPictureCtrl::CPictureCtrl(void)
	:CStatic()
	, m_pStream(NULL)
	, m_bIsPicLoaded(FALSE)
	, m_gdiplusToken(0)
	, m_bClearPic(false)
	, m_bEnableCursor(false)
	, m_CursorSlope(0.0)
	, m_ImageBitmap(NULL)
	, m_Scale(2)
	, m_bTrackCursor(false)
{
	m_CursorPos.x = -1;
	m_CursorPos.y = -1;
	m_CursorAngle = 0;

	GdiplusStartupInput gdiplusStartupInput;
	GdiplusStartup(&m_gdiplusToken, &gdiplusStartupInput, NULL);
}

CPictureCtrl::~CPictureCtrl(void)
{
	//Tidy up
	FreeData();
	GdiplusShutdown(m_gdiplusToken);
}

BOOL CPictureCtrl::LoadFromStream(IStream *piStream)
{
	//Set success error state
	SetLastError(ERROR_SUCCESS);

	FreeData();

	//Check for validity of argument
	if(piStream == NULL)
	{
		SetLastError(ERROR_INVALID_ADDRESS);
		return FALSE;
	}

	//Allocate stream
	DWORD dwResult = CreateStreamOnHGlobal(NULL, TRUE, &m_pStream);
	if(dwResult != S_OK)
	{
		SetLastError(dwResult);
		return FALSE;
	}

	//Rewind the argument stream
	LARGE_INTEGER lInt;
	lInt.QuadPart = 0;
	piStream->Seek(lInt, STREAM_SEEK_SET, NULL);

	//Read the length of the argument stream
	STATSTG statSTG;
	dwResult = piStream->Stat(&statSTG, STATFLAG_DEFAULT);
	if(dwResult != S_OK)
	{
		SetLastError(dwResult);
		SAFE_RELEASE(m_pStream);
		return FALSE;
	}

	//Copy the argument stream to the class stream
	piStream->CopyTo(m_pStream, statSTG.cbSize, NULL, NULL);

	//Mark as loaded
	m_bIsPicLoaded = TRUE;

	Invalidate();
	RedrawWindow();

	return TRUE;
}

BOOL CPictureCtrl::LoadFromStream(BYTE* pData, size_t nSize)
{
	//Set success error state
	SetLastError(ERROR_SUCCESS);
	FreeData();

	//Allocate stream
	DWORD dwResult = CreateStreamOnHGlobal(NULL, TRUE, &m_pStream);
	if(dwResult != S_OK)
	{
		SetLastError(dwResult);
		return FALSE;
	}

	//Copy argument data to the stream
	dwResult = m_pStream->Write(pData, (ULONG)nSize, NULL);
	if(dwResult != S_OK)
	{
		SetLastError(dwResult);
		SAFE_RELEASE(m_pStream);
		return FALSE;
	}

	//Mark as loaded
	m_bIsPicLoaded = TRUE;

	Invalidate();
	RedrawWindow();

	return TRUE;
}

BOOL CPictureCtrl::LoadFromFile(CString &szFilePath)
{
	//Set success error state
	SetLastError(ERROR_SUCCESS);
	FreeData();

	if (szFilePath.GetLength() <= 0)
	{
		m_bClearPic = true;
		Invalidate();
		RedrawWindow();
	}
	else
	{
		m_bClearPic = false;

		//Allocate stream
		DWORD dwResult = CreateStreamOnHGlobal(NULL, TRUE, &m_pStream);
		if (dwResult != S_OK)
		{
			SetLastError(dwResult);
			return FALSE;
		}

		//Open the specified file
		CFile cFile;
		CFileException cFileException;
		if (!cFile.Open(szFilePath, CStdioFile::modeRead | CStdioFile::typeBinary, &cFileException))
		{
			SetLastError(cFileException.m_lOsError);
			SAFE_RELEASE(m_pStream);
			return FALSE;
		}

		//Copy the specified file's content to the stream
		BYTE pBuffer[1024] = { 0 };
		while (UINT dwRead = cFile.Read(pBuffer, 1024))
		{
			dwResult = m_pStream->Write(pBuffer, dwRead, NULL);
			if (dwResult != S_OK)
			{
				SetLastError(dwResult);
				SAFE_RELEASE(m_pStream);
				cFile.Close();
				return FALSE;
			}
		}

		//Close the file
		cFile.Close();

		//Mark as Loaded
		m_bIsPicLoaded = TRUE;

//		m_ImageBitmap = new Bitmap(m_pStream);
//		m_ImageBitmap = Bitmap::FromStream(m_pStream);
		m_ImageBitmap = new Bitmap(szFilePath);
		m_XCenter = m_ImageBitmap->GetWidth() / (2 * m_Scale);
		m_YCenter = m_ImageBitmap->GetHeight() / (2 * m_Scale);

		if ((m_CursorPos.x == -1) && (m_CursorPos.y == -1))
		{
			m_CursorPos.x = m_XCenter;
			m_CursorPos.y = m_YCenter;
		}

		Invalidate();
		RedrawWindow();
	}

	return TRUE;
}

void CPictureCtrl::EnableCursor(bool bEnable)
{
	m_bEnableCursor = bEnable;

	Invalidate();
	RedrawWindow();
}

CString CPictureCtrl::MoveCursor(int Direction)
{
	switch (Direction)
	{
	case MOVE_CENTER:
		m_CursorPos.x = m_XCenter;
		m_CursorPos.y = m_YCenter;
		break;
	case MOVE_LEFT:
		m_CursorPos.x = m_CursorPos.x - 1;
		break;
	case MOVE_RIGHT:
		m_CursorPos.x = m_CursorPos.x + 1;
		break;
	case MOVE_UP:
		m_CursorPos.y = m_CursorPos.y - 1;
		break;
	case MOVE_DOWN:
		m_CursorPos.y = m_CursorPos.y + 1;
		break;
	}

	Invalidate();
	RedrawWindow();

	CString csValue;
	csValue.Format(_T("(%d,%d)"), m_CursorPos.x * m_Scale, m_CursorPos.y * m_Scale);

	return csValue;
}

CString CPictureCtrl::RotateCursor(int Direction)
{
	switch (Direction)
	{
	case ROTATE_CENTER:
		m_CursorAngle = 0.0;
		break;
	case ROTATE_CW:
		if (m_CursorAngle<45.0)
			m_CursorAngle = m_CursorAngle + 1.0;
		break;
	case ROTATE_CCW:
		if (m_CursorAngle > -45.0)
			m_CursorAngle = m_CursorAngle - 1.0;
		break;
	}

	Invalidate();
	RedrawWindow();

	CString csValue;
	csValue.Format(_T("(%5.2f°,%5.2f%%)"), -m_CursorAngle, -m_CursorSlope);

	return csValue;
}

CString CPictureCtrl::GetCursorPosition()
{
	CString csValue;
	csValue.Format(_T("(%d,%d)"), m_CursorPos.x, m_CursorPos.y);

	return csValue;
}

CString CPictureCtrl::GetCursorRotation()
{
	CString csValue;
	csValue.Format(_T("(%5.2f°,%5.2f%%)"), -m_CursorAngle, -m_CursorSlope);

	return csValue;
}

// BOOL CPictureCtrl::LoadFromResource(HMODULE hModule, LPCTSTR lpName, LPCTSTR lpType)
// {
// 	//Set success error state
// 	SetLastError(ERROR_SUCCESS);
// 	FreeData();
// 
// 	//Locate the resource
// 	HRSRC hResource = FindResource(hModule, lpName, lpType);
// 	if(hResource == NULL)
// 	{
// 		return FALSE;
// 	}
// 
// 	//Get the size of the resource
// 	DWORD dwResourceSize = SizeofResource(hModule, hResource);
// 	if(dwResourceSize == 0)
// 	{
// 		return FALSE;
// 	}
// 
// 	//Load the Resource
// 	HGLOBAL hGlobalResource = LoadResource(hModule, hResource);
// 	if(hGlobalResource == NULL)
// 	{
// 		return FALSE;
// 	}
// 
// 	//Lock the resource and get the read pointer
// 	BYTE* pRecource = (BYTE*)LockResource(hGlobalResource);
// 	if(pRecource == NULL)
// 	{
// 		return FALSE;
// 	}
// 
// 	//Allocate the Stream
// 	DWORD dwResult =  CreateStreamOnHGlobal(NULL, TRUE, &m_pStream);
// 	if(dwResult != S_OK)
// 	{
// 		FreeResource(hGlobalResource);
// 		SetLastError(dwResult);
// 		pRecource = NULL;
// 		return FALSE;
// 	}
// 
// 	//Copy the resource data to the stream
// 	dwResult = m_pStream->Write(pRecource, dwResourceSize, NULL);
// 	if(dwResult != S_OK)
// 	{
// 		FreeResource(hGlobalResource);
// 		SAFE_RELEASE(m_pStream);
// 		SetLastError(dwResult);
// 		return FALSE;		
// 	}
// 
// 	//Tidy up
// //	FreeResource(hGlobalResource);
// 	
// 	//Mark as loaded
// 	m_bIsPicLoaded = TRUE;
// 
// 	Invalidate();
// 	RedrawWindow();
// 
// 	return TRUE;
// }

//Overload - Single load function
BOOL CPictureCtrl::Load(CString &szFilePath)
{
	return LoadFromFile(szFilePath);
}

BOOL CPictureCtrl::Load(IStream* piStream)
{
	return LoadFromStream(piStream);
}

BOOL CPictureCtrl::Load(BYTE* pData, size_t nSize)
{
	return LoadFromStream(pData, nSize);
}

// BOOL CPictureCtrl::Load(HMODULE hModule, LPCTSTR lpName, LPCTSTR lpType)
// {
// 	return LoadFromResource(hModule, lpName, lpType);
// }

void CPictureCtrl::FreeData()
{
	m_bIsPicLoaded = FALSE;
	SAFE_RELEASE(m_pStream);
}

void CPictureCtrl::PreSubclassWindow()
{
	CStatic::PreSubclassWindow();
	ModifyStyle(0, SS_OWNERDRAW);
}

void CPictureCtrl::DrawItem(LPDRAWITEMSTRUCT lpDrawItemStruct)
{
	//Check if pic data is loaded
	if(m_bIsPicLoaded)
	{

		//Get control measures
		RECT rc;
		this->GetClientRect(&rc);

		ImageAttributes imAtt;
		imAtt.SetWrapMode(Gdiplus::WrapModeTileFlipXY);

		Graphics graphics(lpDrawItemStruct->hDC);
		Image image(m_pStream);
//		graphics.DrawImage(&image, (INT)rc.left, (INT)rc.top, (INT)(rc.right-rc.left), (INT)(rc.bottom-rc.top));

		//graphics.DrawImage(&image,
		//	Rect(rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top),
		//	0, 0, 2 * image.GetWidth(), 2 * image.GetHeight(),
		//	UnitPixel, &imAtt);
		graphics.DrawImage(&image, (INT)rc.left, (INT)rc.top, (INT)image.GetWidth() / m_Scale, (INT)image.GetHeight() / m_Scale);

		if (m_bEnableCursor == true)
		{
			Pen redPen(Color(255, 255, 0, 0), 1);
			//LONG m_XCenter = rc.left + (rc.right - rc.left) / 2;
			//LONG m_YCenter = rc.top + (rc.bottom - rc.top) / 2;
			//m_XCenter = m_CursorPos.x + (rc.right - rc.left) / 2;
			//m_YCenter = m_CursorPos.y + (rc.bottom - rc.top) / 2;

			double h = 60.0;
			LONG xRotate = (LONG)(h * sin(m_CursorAngle*0.0174532925));
			LONG yRotate = (LONG)(h * cos(m_CursorAngle*0.0174532925));

			m_CursorSlope = tan(m_CursorAngle*0.0174532925)*100.0;

//			graphics.DrawLine(&redPen, (INT)(m_XCenter + xRotate), (INT)(m_YCenter - yRotate), (INT)(m_XCenter - xRotate), (INT)(m_YCenter + yRotate));		//horiz line
//			graphics.DrawLine(&redPen, (INT)(m_XCenter - yRotate), (INT)(m_YCenter - xRotate), (INT)(m_XCenter + yRotate), (INT)(m_YCenter + xRotate));		//vert line
			graphics.DrawLine(&redPen, (INT)(m_CursorPos.x + xRotate), (INT)(m_CursorPos.y - yRotate), (INT)(m_CursorPos.x - xRotate), (INT)(m_CursorPos.y + yRotate));		//horiz line
			graphics.DrawLine(&redPen, (INT)(m_CursorPos.x - yRotate), (INT)(m_CursorPos.y - xRotate), (INT)(m_CursorPos.x + yRotate), (INT)(m_CursorPos.y + xRotate));		//vert line
		}
		
	}
	else if (m_bClearPic)
	{
		Graphics graphics(lpDrawItemStruct->hDC);
		Color EraseColor(255, 255, 255, 255);

		graphics.Clear(EraseColor);
	}
}

BOOL CPictureCtrl::OnEraseBkgnd(CDC *pDC)
{
	if(m_bIsPicLoaded)
	{

		//Get control measures
		RECT rc;
		this->GetClientRect(&rc);

		Graphics graphics(pDC->GetSafeHdc());
		LARGE_INTEGER liSeekPos;
		liSeekPos.QuadPart = 0;
		m_pStream->Seek(liSeekPos, STREAM_SEEK_SET, NULL);

		ImageAttributes imAtt;
		imAtt.SetWrapMode(Gdiplus::WrapModeTileFlipXY);

		Image image(m_pStream);
//		graphics.DrawImage(&image, (INT)rc.left, (INT)rc.top, (INT)(rc.right-rc.left), (INT)(rc.bottom-rc.top));
		graphics.DrawImage(	&image, 
							Rect(rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top), 
							0, 0, 2 * image.GetWidth(), 2 * image.GetHeight(), 
							UnitPixel, &imAtt);
		return TRUE;
	}
	else
	{
		return CStatic::OnEraseBkgnd(pDC);
	}
}

COLORREF CPictureCtrl::GetPixel()
{
	Color PixelColor;
		m_ImageBitmap->GetPixel((INT)(m_CursorPos.x * m_Scale), (INT)(m_CursorPos.y * m_Scale), &PixelColor);
//		((INT)m_XCenter * 2, (INT)m_YCenter * 2, &PixelColor);

	COLORREF PixelColorRef = RGB(PixelColor.GetRed(),PixelColor.GetGreen(),PixelColor.GetBlue());

	return PixelColorRef;
}

BEGIN_MESSAGE_MAP(CPictureCtrl, CStatic)
	ON_WM_LBUTTONDOWN()
	ON_WM_LBUTTONUP()
	ON_WM_MOUSEMOVE()
END_MESSAGE_MAP()

void CPictureCtrl::OnLButtonDown(UINT nFlags, CPoint point)
{
	CStatic::OnLButtonDown(nFlags, point);
	m_bTrackCursor = true;
}

void CPictureCtrl::OnLButtonUp(UINT nFlags, CPoint point)
{
	CStatic::OnLButtonUp(nFlags, point);
	m_bTrackCursor = false;

	GetParent()->PostMessageW(WM_USER + 2, 0, 0);
}

void CPictureCtrl::OnMouseMove(UINT nFlags, CPoint point)
{
	if (m_bTrackCursor)
	{
		m_CursorPos.x = point.x;
		m_CursorPos.y = point.y;
		Invalidate();
	}
}
