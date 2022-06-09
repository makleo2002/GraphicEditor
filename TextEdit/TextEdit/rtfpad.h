// -----------------------------------------------------
// Описание функций
// -----------------------------------------------------
LRESULT WINAPI
WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);
BOOL WndProc_OnCreate(HWND hWnd,
	LPCREATESTRUCT lpCreateStruct);
void WndProc_OnDestroy(HWND hWnd);
void WndProc_OnCommand(HWND hWnd, int id,
	HWND hwndCtl, UINT codeNotify);
LRESULT WndProc_OnNotify(HWND hWnd, int idFrom,
	NMHDR FAR* pnmhdr);
void WndProc_OnSize(HWND hwnd, UINT state, int cx, int cy);
void WndProc_OnSetFocus(HWND hwnd, HWND hwndOldFocus);
void FileSaveAs(HWND hwnd);
DWORD CALLBACK SaveCallback(DWORD dwCookie, LPBYTE   pbBuff,
	LONG cb, LONG* pcb);
void FileOpen(HWND hwnd);
DWORD CALLBACK
OpenCallback(DWORD dwCookie, LPBYTE   pbBuff,
	LONG cb, LONG* pcb);
void FilePrint(void);
#define IDC_RTFEDIT 1236