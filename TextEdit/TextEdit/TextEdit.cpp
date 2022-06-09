#define STRICT
#include <windows.h>
#include <windowsx.h>
#include <commctrl.h>
#include <richedit.h>
#include "resource.h"
#include "afxres.h"
#include "rtfpad.h"
#include <winresrc.h>
#include <tchar.h>

#define _CRT_SECURE_NO_WARNINGS

HINSTANCE hInst; //идентификатор приложения, полученный функцией WinMain
WCHAR szAppName[] = L"RtfEditApp";//Хранит имя приложения
WCHAR szAppTitle[] = L"MyNote Text Editor";//Хранит заголовок приложения
HWND hwndEdit;//используется для хранения идентификатора созданного органа управления Rich Edit
HINSTANCE hRTFLib;//Для инициализации DLL-библиотеки, отвечающей за работу органа управления Rich Edit, мы используем переменную hRTFLib (в нее записывается идентификатор загруженной библиотеки RICHED32.DLL). 

// -----------------------------------------------------
// Функция WinMain
// -----------------------------------------------------
int APIENTRY
WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance,
    LPSTR lpCmdLine, int nCmdShow)
{
    WNDCLASSEX wc;
    HWND hWnd;
    MSG msg;

    hInst = hInstance;

    // Преверяем, не было ли это приложение запущено ранее
    hWnd = FindWindow(szAppName, NULL);
    if (hWnd)
    {
        if (IsIconic(hWnd))
            ShowWindow(hWnd, SW_RESTORE);
        SetForegroundWindow(hWnd);
        return FALSE;
    }

    // Загружаем библиотеку RICHED32.DLL
    hRTFLib = LoadLibrary(L"RICHED32.DLL");
    if (!hRTFLib)
        return FALSE;

    // Регистрируем класс окна
    memset(&wc, 0, sizeof(wc));
    wc.cbSize = sizeof(WNDCLASSEX);
    wc.hIconSm =(HICON) LoadImage(hInst,
        MAKEINTRESOURCE(IDI_APPICONSM), IMAGE_ICON, 16, 16, 0);
    wc.style = 0;
    wc.lpfnWndProc = (WNDPROC)WndProc;
    wc.cbClsExtra = 0;
    wc.cbWndExtra = 0;
    wc.hInstance = hInst;
    wc.hIcon = (HICON)LoadImage(hInst,
        MAKEINTRESOURCE(IDI_APPICON), IMAGE_ICON, 32, 32, 0);
    wc.hCursor = LoadCursor(NULL, IDC_ARROW);
    wc.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
    wc.lpszMenuName = MAKEINTRESOURCE(IDR_APPMENU);
    wc.lpszClassName = szAppName;
    if (!RegisterClassEx(&wc))
        if (!RegisterClass((LPWNDCLASS)&wc.style))
            return FALSE;

    // Создаем главное окно приложения
    hWnd = CreateWindow(szAppName, szAppTitle,
        WS_OVERLAPPEDWINDOW, CW_USEDEFAULT, 0, CW_USEDEFAULT, 0,
        NULL, NULL, hInst, NULL);
    if (!hWnd) return(FALSE);

    // Отображаем окно и запускаем цикл обработки сообщений
    ShowWindow(hWnd, nCmdShow);
    UpdateWindow(hWnd);
    while (GetMessage(&msg, NULL, 0, 0))
    {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }
    return msg.wParam;
}

// -----------------------------------------------------
// Функция WndProc
// -----------------------------------------------------
LRESULT WINAPI
WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    switch (msg)
    {
        HANDLE_MSG(hWnd, WM_CREATE, WndProc_OnCreate);
        HANDLE_MSG(hWnd, WM_DESTROY, WndProc_OnDestroy);
        HANDLE_MSG(hWnd, WM_COMMAND, WndProc_OnCommand);
        HANDLE_MSG(hWnd, WM_SIZE, WndProc_OnSize);
        HANDLE_MSG(hWnd, WM_SETFOCUS, WndProc_OnSetFocus);
    case WM_CLOSE:
        if (MessageBox(hWnd, L"Хотите выйти?", L"MyNote", MB_OKCANCEL) == IDOK)
        {
            DestroyWindow(hWnd);
        }
        // Else: User canceled. Do nothing.
        return 0;

    default:
        return(DefWindowProc(hWnd, msg, wParam, lParam));
    }
}

// -----------------------------------------------------
// Функция WndProc_OnCreate
// -----------------------------------------------------
BOOL WndProc_OnCreate(HWND hWnd, LPCREATESTRUCT lpCreateStruct)
{
    RECT rc;

    // Определяем размеры внутренней области главного окна
    GetClientRect(hWnd, &rc);

    // Создаем орган управления Rich Edit
    hwndEdit = CreateWindowEx(0L, L"RICHEDIT", L"",
        WS_VISIBLE | WS_CHILD | WS_BORDER |
        WS_HSCROLL | WS_VSCROLL |
        ES_NOHIDESEL | ES_AUTOVSCROLL | ES_MULTILINE |
        ES_SAVESEL | ES_SUNKEN,
        0, 0, rc.right - rc.left, rc.bottom - rc.top,
        hWnd, (HMENU)IDC_RTFEDIT, hInst, NULL);

    if (hwndEdit == NULL)
        return FALSE;

    // Передаем фокус ввода органу управления Rich Edit
    SetFocus(hwndEdit);

    return TRUE;
}

// -----------------------------------------------------
// Функция WndProc_OnDestroy
// -----------------------------------------------------
#pragma warning(disable: 4098)
void WndProc_OnDestroy(HWND hWnd)
{
    // Уничтожаем орган управления Rich Edit
    if (hwndEdit)
        DestroyWindow(hwndEdit);

    // Освобождаем библиотеку RICHED32.DLL
    if (hRTFLib)
        FreeLibrary(hRTFLib);

    PostQuitMessage(0);
    return ;
}

// -----------------------------------------------------
// Функция WndProc_OnCommand
// -----------------------------------------------------
#pragma warning(disable: 4098)
void WndProc_OnCommand(HWND hWnd, int id,
    HWND hwndCtl, UINT codeNotify)
{
    CHARFORMAT cf;
    CHOOSEFONT chfnt;
    LOGFONT lf;
    HDC hDC;
    PARAFORMAT pf;

    switch (id)
    {
        // Изменяем жирность символов
    case ID_FORMAT_BOLD:
    {
        cf.cbSize = sizeof(cf);

        // Определяем формат символов
        SendMessage(hwndEdit, EM_GETCHARFORMAT, TRUE, (LPARAM)&cf);

        // Изменяем бит поля dwEffects, с помощью которого
        // можно выделить символы как bold (жирное начертание)
        cf.dwMask = CFM_BOLD;

        // Инвертируем бит, определяющий жирное начертание
        cf.dwEffects ^= CFE_BOLD;

        // Изменяем формат символов
        SendMessage(hwndEdit, EM_SETCHARFORMAT,
            SCF_SELECTION, (LPARAM)&cf);

        return ;
        break;
    }

    // Устанавливаем или отменяем наклонное
    // начертание символов
    case ID_FORMAT_ITALIC:
    {
        cf.cbSize = sizeof(cf);
        SendMessage(hwndEdit, EM_GETCHARFORMAT,
            TRUE, (LPARAM)&cf);

        cf.dwMask = CFM_ITALIC;
        cf.dwEffects ^= CFE_ITALIC;
        SendMessage(hwndEdit, EM_SETCHARFORMAT,
            SCF_SELECTION, (LPARAM)&cf);

        return;
        break;
    }

    // Устанавливаем или отменяем выделение
    // символов подчеркиванием
    case ID_FORMAT_UNDERLINE:
    {
        cf.cbSize = sizeof(cf);
        SendMessage(hwndEdit, EM_GETCHARFORMAT,
            TRUE, (LPARAM)&cf);

        cf.dwMask = CFM_UNDERLINE;
        cf.dwEffects ^= CFE_UNDERLINE;
        SendMessage(hwndEdit, EM_SETCHARFORMAT,
            SCF_SELECTION, (LPARAM)&cf);

        return ;
        break;
    }

    // Изменяем шрифт символов
    case ID_FORMAT_FONT:
    {
        cf.cbSize = sizeof(cf);

        // Определяем текущий формат символов
        SendMessage(hwndEdit, EM_GETCHARFORMAT,
            TRUE, (LPARAM)&cf);

        // Сбрасываем содержимое структур, которые будут
        // использоваться для выбора шрифта
        memset(&chfnt, 0, sizeof(chfnt));
        memset(&lf, 0, sizeof(lf));

        // Получаем контекст отображения
        hDC = GetDC(hWnd);

        // Если было задано выделение наклоном или жирным
        // шрифтом,подбираем шрифт с соответствующими атрибутами
        lf.lfItalic = (BOOL)(cf.dwEffects & CFE_ITALIC);
        lf.lfUnderline = (BOOL)(cf.dwEffects & CFE_UNDERLINE);

        // Преобразуем высоту из TWIPS-ов в пикселы.
        // Устанавливаем отрицательный знак, чтобы 
        // выполнялось преобразование и использовалось
        // абсолютное значение высоты символов
        lf.lfHeight = -cf.yHeight / 20;

        // Набор символов, принятый по умолчанию
        lf.lfCharSet = ANSI_CHARSET;

        // Качество символов, принятое по умолчанию
        lf.lfQuality = DEFAULT_QUALITY;

        // Выбираем семейство шрифтов
        lf.lfPitchAndFamily = cf.bPitchAndFamily;

        // Название начертания шрифта
        lstrcpy(lf.lfFaceName, cf.szFaceName);

        // Устанавливаем вес шрифта в зависимости от того,
        // было использовано выделение жирным шрифтом 
        // или нет
        if (cf.dwEffects & CFE_BOLD)
            lf.lfWeight = FW_BOLD;
        else
            lf.lfWeight = FW_NORMAL;

        // Заполняем структуру для функции выбора шрифта
        chfnt.lStructSize = sizeof(chfnt);
        chfnt.Flags = CF_SCREENFONTS | CF_INITTOLOGFONTSTRUCT;
        chfnt.hDC = hDC;
        chfnt.hwndOwner = hWnd;
        chfnt.lpLogFont = &lf;
        chfnt.rgbColors = RGB(0, 0, 0);
        chfnt.nFontType = SCREEN_FONTTYPE;

        // Выводим на экран диалоговую панель для
        // выбора шрифта
        if (ChooseFont(&chfnt))
        {
            // Можно использовать все биты поля dwEffects
            cf.dwMask = CFM_BOLD | CFM_FACE | CFM_ITALIC |
                CFM_UNDERLINE | CFM_SIZE | CFM_OFFSET;

            // Преобразование в TWIPS-ы
            cf.yHeight = -lf.lfHeight * 20;

            // Устанавливаем поле dwEffects 
            cf.dwEffects = 0;
            if (lf.lfUnderline)
                cf.dwEffects |= CFE_UNDERLINE;

            if (lf.lfWeight == FW_BOLD)
                cf.dwEffects |= CFE_BOLD;

            if (lf.lfItalic)
                cf.dwEffects |= CFE_ITALIC;

            // Устанавливаем семейство шрифта
            cf.bPitchAndFamily = lf.lfPitchAndFamily;

            // Устанавливаем название начертания шрифта
            lstrcpy(cf.szFaceName, lf.lfFaceName);

            // Изменяем шрифтовое оформление символов
            SendMessage(hwndEdit, EM_SETCHARFORMAT,
                SCF_SELECTION, (LPARAM)&cf);
        }

        // Освобождаем контекст отображения
        ReleaseDC(hWnd, hDC);

        return ;
        break;
    }

    // Устанавливаем выравнивание параграфа по левой границе
    // окна органа управления Rich Edit
    case ID_FORMAT_PARAGRAPH_LEFT:
    {
        pf.cbSize = sizeof(pf);
        pf.dwMask = PFM_ALIGNMENT;
        pf.wAlignment = PFA_LEFT;

        // Изменяем тип выравнивания текущего параграфа
        SendMessage(hwndEdit, EM_SETPARAFORMAT, 0, (LPARAM)&pf);

        return ;
        break;
    }

    // Устанавливаем выравнивание параграфа по правой границе
    // окна органа управления Rich Edit
    case ID_FORMAT_PARAGRAPH_RIGHT:
    {
        pf.cbSize = sizeof(pf);
        pf.dwMask = PFM_ALIGNMENT;
        pf.wAlignment = PFA_RIGHT;
        SendMessage(hwndEdit, EM_SETPARAFORMAT, 0, (LPARAM)&pf);
        return ;
        break;
    }

    // Выполняем центровку текущего параграфа
    case ID_FORMAT_PARAGRAPH_CENTER:
    {
        pf.cbSize = sizeof(pf);
        pf.dwMask = PFM_ALIGNMENT;
        pf.wAlignment = PFA_CENTER;
        SendMessage(hwndEdit, EM_SETPARAFORMAT, 0, (LPARAM)&pf);
        return ;
        break;
    }

    // Реализуем стандартные функции меню Edit
    case ID_EDIT_UNDO:
        SendMessage(hwndEdit, EM_UNDO, 0, 0L);
        return ;
        break;

    case ID_EDIT_CUT:
        SendMessage(hwndEdit, WM_CUT, 0, 0L);
        return ;
        break;

    case ID_EDIT_COPY:
        SendMessage(hwndEdit, WM_COPY, 0, 0L);
        return ;
        break;

    case ID_EDIT_PASTE:
        SendMessage(hwndEdit, WM_PASTE, 0, 0L);
        return ;
        break;

    case ID_EDIT_DELETE:
        SendMessage(hwndEdit, WM_CLEAR, 0, 0L);
        return ;
        break;

        // Выделяем весь текст, который есть в окне
        // органа управления Rich Edit
    case ID_EDIT_SELECTALL:
    {
        CHARRANGE charr;

        charr.cpMin = 0;  // от начала... 
        charr.cpMax = -1; // ... и до конца текста

        SendMessage(hwndEdit, EM_EXSETSEL, 0, (LPARAM)&charr);
        return ;
        break;
    }

    // При создании нового текста удаляем текущее
    // содержимое окна редактирования
    case ID_FILE_NEW:
        SetWindowText(hwndEdit, L"\0");
        return ;
        break;

    case ID_FILE_OPEN:
        FileOpen(hWnd);   // загружаем файл для редактирования
        return ;
        break;

    case ID_FILE_SAVEAS:
        FileSaveAs(hWnd); // сохраняем текст в файле
        return ;
        break;

    case ID_FILE_PRINT:
        FilePrint();      // печатаем текст
        return ;
        break;

    case ID_FILE_EXIT:
        PostQuitMessage(0); // завершаем работу приложения
        return ;
        break;

    case ID_HELP_ABOUT:
        MessageBox(hWnd,
            L"MyNote Text Editor, v.1.0\n"
            L"(C) Maxim Lapardin, 2002\n"
            L"Email: makelo2002@gmail.com",
            szAppTitle, MB_OK | MB_ICONINFORMATION);
        return ;
        break;

    default:
        break;
    }
    return FORWARD_WM_COMMAND(hWnd, id, hwndCtl, codeNotify,
        DefWindowProc);
}

// -----------------------------------------------------
// Функция WndProc_OnSize       
// -----------------------------------------------------
#pragma warning(disable: 4098)
void WndProc_OnSize(HWND hwnd, UINT state, int cx, int cy)
{
    MoveWindow(hwndEdit, 0, 0, cx, cy, TRUE);
    return FORWARD_WM_SIZE(hwnd, state, cx, cy, DefWindowProc);
}

// -----------------------------------------------------
// Функция WndProc_OnSetFocus
// -----------------------------------------------------
#pragma warning(disable: 4098)
void WndProc_OnSetFocus(HWND hwnd, HWND hwndOldFocus)
{
    // Когда главное окно нашего приложения получает 
    // фокус ввода, оно передает фокус ввода окну
    // органа управления Rich Edit
    SetFocus(hwndEdit);
    return FORWARD_WM_SETFOCUS(hwnd, hwndOldFocus,
        DefWindowProc);
}

// -----------------------------------------------------
// Функция FileSaveAs
// -----------------------------------------------------
void FileSaveAs(HWND hwnd)
{
    OPENFILENAME ofn;
    WCHAR szFile[256] = L"Unnamed.rtf";
    WCHAR szDirName[1024];
    WCHAR szFileTitle[256];
    // Фильтр допускает сохранение текста в файле с
    // расширением имени rtf, txt, или любым другим
    WCHAR szFilter[256] =
        L"Rich Text Files\0*.rtf\0Text Files\0*.txt\0"
        L"Any Files\0*.*\0";

    HFILE      hFile;
    OFSTRUCT   of;
    EDITSTREAM es;

    memset(&ofn, 0, sizeof(OPENFILENAME));

    // Определяем путь к текущему каталогу
    GetCurrentDirectory(sizeof(szDirName), szDirName);

    // Заполняем структуру для выбора выходного файла
    ofn.lStructSize = sizeof(OPENFILENAME);
    ofn.hwndOwner = hwnd;
    ofn.lpstrFilter = szFilter;
    ofn.lpstrInitialDir = szDirName;
    ofn.nFilterIndex = 2;
    ofn.lpstrFile = szFile;
    ofn.nMaxFile = sizeof(szFile);
    ofn.lpstrFileTitle = szFileTitle;
    ofn.nMaxFileTitle = sizeof(szFileTitle);
    ofn.lpstrDefExt = L"rtf";
    ofn.Flags = OFN_OVERWRITEPROMPT | OFN_HIDEREADONLY;

    // Выводим на экран диалоговую панель, предназначенную
    // для выбора выходного файла
    if (GetSaveFileName(&ofn))
    {
        // Если файл выбран, открываем его для записи или
        // создаем
        if (*ofn.lpstrFile)
        {
            hFile = OpenFile((LPCSTR)ofn.lpstrFile, &of, OF_CREATE);

            // Устанавливаем параметры функции обратного вызова,
            // которая будет выполнять запись
            es.dwCookie = (DWORD)hFile;
            es.dwError = 0;
            es.pfnCallback = SaveCallback;

            // Если расширение файла rtf, файл сохраняется как
            // rtf-файл. В противном случае он сохраняется как
            // обычный текстовый файл
            _strupr((char*)&ofn.lpstrFile[ofn.nFileExtension]);
            if (!strncmp((LPCSTR)&ofn.lpstrFile[ofn.nFileExtension], "RTF", 3))
                SendMessage(hwndEdit, EM_STREAMOUT, SF_RTF,
                    (LPARAM)&es);
            else
                SendMessage(hwndEdit, EM_STREAMOUT, SF_TEXT,
                    (LPARAM)&es);

            // Закрываем файл
            _lclose(hFile);

            // Сбрасываем признак изменения содержимого окна
            // редактора текста
            SendMessage(hwndEdit, EM_SETMODIFY, FALSE, 0L);
        }
    }
}

// -----------------------------------------------------
// Функция SaveCallback
// -----------------------------------------------------
DWORD CALLBACK
SaveCallback(DWORD dwCookie, LPBYTE pbBuff,
    LONG cb, LONG* pcb)
{
    // Выполняем запись блока данных длиной cb байт
    cb = _lwrite((HFILE)dwCookie, (LPCCH)pbBuff, cb);
    *pcb = cb;
    return 0;
}

// -----------------------------------------------------
// Функция FileOpen
// -----------------------------------------------------
void FileOpen(HWND hwnd)
{
    OPENFILENAME ofn;
    WCHAR szFile[256];
    WCHAR szDirName[256];
    WCHAR szFileTitle[256];
    WCHAR szFilter[256] =
        L"Rich Text Files\0*.rtf\0Text Files\0*.txt\0"
        L"Any Files\0*.*\0";

    HFILE      hFile;
    OFSTRUCT   of;
    EDITSTREAM es;

    memset(&ofn, 0, sizeof(OPENFILENAME));
    GetCurrentDirectory(sizeof(szDirName), szDirName);
    szFile[0] = '\0';

    // Подготавливаем структуру для выбора входного файла
    ofn.lStructSize = sizeof(OPENFILENAME);
    ofn.hwndOwner = hwnd;
    ofn.lpstrFilter = szFilter;
    ofn.lpstrInitialDir = szDirName;
    ofn.nFilterIndex = 1;
    ofn.lpstrFile = szFile;
    ofn.nMaxFile = sizeof(szFile);
    ofn.lpstrFileTitle = szFileTitle;
    ofn.nMaxFileTitle = sizeof(szFileTitle);
    ofn.Flags = OFN_PATHMUSTEXIST | OFN_FILEMUSTEXIST;

    // Выводим на экран диалоговую панель, предназначенную
    // для выбора входного файла
    if (GetOpenFileName(&ofn))
    {
        // Если файл выбран, открываем его для чтения
        if (*ofn.lpstrFile)
        {
            hFile = OpenFile((LPCSTR)ofn.lpstrFile, &of, OF_READ);

            // Устанавливаем параметры функции обратного вызова,
            // которая будет выполнять чтение
            es.dwCookie = (DWORD)hFile;
            es.dwError = 0;
            es.pfnCallback = OpenCallback;

            // Если расширение файла rtf, файл загружается как
            // rtf-файл. В противном случае он загружается как
            // обычный текстовый файл
            _strupr((char*)&ofn.lpstrFile[ofn.nFileExtension]);
            if (!strncmp((char*)&ofn.lpstrFile[ofn.nFileExtension], "RTF", 3))
                SendMessage(hwndEdit, EM_STREAMIN, SF_RTF, (LPARAM)&es);
            else
                SendMessage(hwndEdit, EM_STREAMIN, SF_TEXT, (LPARAM)&es);

            // Закрываем файл
            _lclose(hFile);

            // Сбрасываем признак изменения содержимого окна
            // редактора текста
            SendMessage(hwndEdit, EM_SETMODIFY, FALSE, 0L);
        }
    }
}

// -----------------------------------------------------
// Функция OpenCallback
// -----------------------------------------------------
DWORD CALLBACK
OpenCallback(DWORD dwCookie, LPBYTE pbBuff,
    LONG cb, LONG* pcb)
{
    // Выполняем чтение блока данных длиной cb байт
    *pcb = _lread((HFILE)dwCookie, pbBuff, cb);
    if (*pcb <= 0)
        *pcb = 0;
    return 0;
}

// -----------------------------------------------------
// Функция FilePrint
// -----------------------------------------------------
void FilePrint(void)
{
    FORMATRANGE fr;
    DOCINFO docInfo;
    LONG lLastChar, lTextSize;
    PRINTDLG pd;
    int nRc;
    HDC hPrintDC;

    // Инициализируем поля структуры PRITDLG
    memset(&pd, 0, sizeof(pd));
    pd.lStructSize = sizeof(PRINTDLG);
    pd.hwndOwner = hwndEdit;
    pd.hInstance = hInst;
    pd.Flags = PD_RETURNDC | PD_NOPAGENUMS |
        PD_NOSELECTION | PD_PRINTSETUP | PD_ALLPAGES;
    pd.nFromPage = 0xffff;
    pd.nToPage = 0xffff;
    pd.nMinPage = 0;
    pd.nMaxPage = 0xffff;
    pd.nCopies = 1;

    // Выводим на экран диалоговую панель, предназначенную
    // для печати документа
    if (PrintDlg(&pd) == TRUE)
    {
        hPrintDC = pd.hDC;

        // Инициализируем поля структуры FORMATRANGE 
        memset(&fr, 0, sizeof(fr));

        // Будем печатать с использованием контекста 
        // принтера, полученного от функции PrintDlg
        fr.hdc = fr.hdcTarget = hPrintDC;

        // Печатаем весь документ
        fr.chrg.cpMin = 0;
        fr.chrg.cpMax = -1;

        // Устанавливаем размеры страницы в TWIPS-ах
        fr.rcPage.top = 0;
        fr.rcPage.left = 0;
        fr.rcPage.right =
            MulDiv(GetDeviceCaps(hPrintDC, PHYSICALWIDTH),
                1440, GetDeviceCaps(hPrintDC, LOGPIXELSX));

        fr.rcPage.bottom = MulDiv(GetDeviceCaps(hPrintDC,
            PHYSICALHEIGHT), 1440,
            GetDeviceCaps(hPrintDC, LOGPIXELSY));
        fr.rc = fr.rcPage;

        // Оставляем поля
        if (fr.rcPage.right > 2 * 3 * 1440 / 4 + 1440)
            fr.rc.right -= (fr.rc.left = 3 * 1440 / 4);
        if (fr.rcPage.bottom > 3 * 1440)
            fr.rc.bottom -= (fr.rc.top = 1440);

        // Заполняем поля структуры DOCINFO
        memset(&docInfo, 0, sizeof(DOCINFO));
        docInfo.cbSize = sizeof(DOCINFO);
        docInfo.lpszOutput = NULL;
        docInfo.lpszDocName = L"RtfPad document";


        // Начинаем печать документа
        nRc = StartDoc(hPrintDC, &docInfo);

        // Если произошла ошибка, получаем и выводим на экран
        // код ошибки
        if (nRc < 0)
        {
            WCHAR szErr[128];
            DWORD dwErr = GetLastError();
            wsprintf(szErr, L"Print Error %ld", dwErr);

            MessageBox(NULL, szErr,
                szAppTitle, MB_OK | MB_ICONEXCLAMATION);

            DeleteDC(hPrintDC);
            return;
        }

        // Начинаем печать страницы
        StartPage(hPrintDC);

        lLastChar = 0;

        // Определяем длину текста в байтах
        lTextSize = SendMessage(hwndEdit, WM_GETTEXTLENGTH, 0, 0);

        // Цикл по всем страницам документа
        while (lLastChar < lTextSize)
        {
            // Форматируем данные для принтера и печатаем их
            lLastChar = SendMessage(hwndEdit, EM_FORMATRANGE, TRUE,
                (LPARAM)&fr);

            if (lLastChar < lTextSize)
            {
                // Завершаем печать очередной страницы
                EndPage(hPrintDC);

                // Начинаем новую страницу
                StartPage(hPrintDC);
                fr.chrg.cpMin = lLastChar;
                fr.chrg.cpMax = -1;
            }
        }

        // Удаляем информацию, которая хранится в 
        // органе управления Rich Edit
        SendMessage(hwndEdit, EM_FORMATRANGE, TRUE, (LPARAM)NULL);

        // Завершаем печать страницы
        EndPage(hPrintDC);

        // Завершаем печать документа
        EndDoc(hPrintDC);

        // Удаляем контекст принтера
        DeleteDC(hPrintDC);
    }
}