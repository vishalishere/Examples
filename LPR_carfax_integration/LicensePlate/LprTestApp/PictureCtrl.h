///////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////
// PictureCtrl.h
// 
// Author: Tobias Eiseler
//
// E-Mail: tobias.eiseler@sisternicky.com
// 
// Function: A MFC Picture Control to display
//           an image on a Dialog, etc.
///////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////

#pragma once
#include "afxwin.h"

enum {
	MOVE_CENTER,
	MOVE_LEFT,
	MOVE_UP,
	MOVE_RIGHT,
	MOVE_DOWN
};

enum {
	ROTATE_CENTER,
	ROTATE_CW,
	ROTATE_CCW
};

#include <GdiPlus.h>
using namespace Gdiplus;

class CPictureCtrl :
	public CStatic
{
public:

	//Constructor
	CPictureCtrl(void);

	//Destructor
	~CPictureCtrl(void);

public:

	//Loads an image from a file
	BOOL LoadFromFile(CString &szFilePath);

	//Loads an image from an IStream interface
	BOOL LoadFromStream(IStream* piStream);

	//Loads an image from a byte stream;
	BOOL LoadFromStream(BYTE* pData, size_t nSize);

	//Loads an image from a Resource
// 	BOOL LoadFromResource(HMODULE hModule, LPCTSTR lpName, LPCTSTR lpType);

	//Overload - Single load function
	BOOL Load(CString &szFilePath);
	BOOL Load(IStream* piStream);
	BOOL Load(BYTE* pData, size_t nSize);
// 	BOOL Load(HMODULE hModule, LPCTSTR lpName, LPCTSTR lpType);

	//Frees the image data
	void FreeData();

	CString MoveCursor(int Direction);
	CString RotateCursor(int Direction);
	void EnableCursor(bool bEnable);
	CString GetCursorPosition();
	CString GetCursorRotation();
	COLORREF GetPixel();

protected:
	virtual void PreSubclassWindow();

	//Draws the Control
	virtual void DrawItem(LPDRAWITEMSTRUCT lpDrawItemStruct);
	virtual BOOL OnEraseBkgnd(CDC* pDC);

	CPoint m_CursorPos;
	double m_CursorAngle;
	bool m_bEnableCursor;
	double m_CursorSlope;

private:

	//Internal image stream buffer
	IStream* m_pStream;

	Bitmap *m_ImageBitmap;
	LONG m_XCenter;
	LONG m_YCenter;

	LONG m_Scale;

	//Control flag if a pic is loaded
	BOOL m_bIsPicLoaded;

	//GDI Plus Token
	ULONG_PTR m_gdiplusToken;

	bool m_bClearPic;
public:
	DECLARE_MESSAGE_MAP()
	afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
	afx_msg void OnLButtonUp(UINT nFlags, CPoint point);
	afx_msg void OnMouseMove(UINT nFlags, CPoint point);

	bool m_bTrackCursor;
};
