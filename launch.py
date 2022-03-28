import time
import datetime
import win32gui
import pyautogui
import win32api
import win32con

def clickLB(x1,y1):
	lParam = win32api.MAKELONG(x1, y1)
	win32gui.PostMessage(hWnd, win32con.WM_MOUSEMOVE, None, lParam)
	time.sleep(0.2)
	win32gui.PostMessage(hWnd, win32con.WM_LBUTTONDOWN, win32con.MK_LBUTTON, lParam)
	time.sleep(0.1)
	win32gui.PostMessage(hWnd, win32con.WM_LBUTTONUP, win32con.MK_LBUTTON, lParam)

DownTimeH = 18
DownTimeM = 20
date = datetime.datetime.today()
CurrentTimeH = int(date.strftime('%H'))
CurrentTimeM = int(date.strftime('%M'))
if (CurrentTimeH == DownTimeH and CurrentTimeM < DownTimeM):
	print(f"wait {DownTimeM - CurrentTimeM} min for DT")
	time.sleep(60 * (DownTimeM - CurrentTimeM))

# hWnd = win32gui.FindWindow(None, "EVE")
# if hWnd != 0:
# 	print("Login window")
# 	#close window with loot
# 	clickLB(1805, 385-23)
# 	time.sleep(5)
# 	#start twin acc
# 	clickLB(1000, 600)
# 	time.sleep(40)
# 	#control click
# 	pyautogui.click()
# 	time.sleep(5)

hWnd = win32gui.FindWindow("trinityWindow", None)
print("hWnd: ", hWnd, "\n")
if hWnd != 0:
	exit(0)

hWndLauncher = win32gui.FindWindow("Qt5152QWindowIcon", None)
win32gui.SetForegroundWindow(hWndLauncher)
time.sleep(0.5)
rect = win32gui.GetWindowRect(hWndLauncher)
x = rect[0]
y = rect[1]


#start client
pyautogui.moveTo(x+50, y+145)
time.sleep(0.5)
pyautogui.click()
time.sleep(40)


hWnd = win32gui.FindWindow("trinityWindow", None)
if hWnd == 0:
	exit(0)

#close window with loot
clickLB(1805, 385-23)
time.sleep(5)

#start twin acc
clickLB(1000, 600)
time.sleep(40)

#control click
pyautogui.click()
time.sleep(5)

