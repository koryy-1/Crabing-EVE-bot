import time
import datetime
import win32gui
import pyautogui
import win32api
import win32con
import json
import requests
import win32com.client
import ctypes

with open('Config.json') as f:
    config = json.load(f)
print('NickName from bot-launch: ', config["NickName"])


def clickLB(x1, y1, hWnd):
	lParam = win32api.MAKELONG(x1, y1)
	win32gui.PostMessage(hWnd, win32con.WM_MOUSEMOVE, None, lParam)
	time.sleep(0.2)
	win32gui.PostMessage(hWnd, win32con.WM_LBUTTONDOWN, win32con.MK_LBUTTON, lParam)
	time.sleep(0.1)
	win32gui.PostMessage(hWnd, win32con.WM_LBUTTONUP, win32con.MK_LBUTTON, lParam)

def queue_up(message):
	for i in range(20): # 10 min max
		res = requests.post('http://localhost:3000/', json={'NickName':config["NickName"], 'message':message + ' ' + str(i)})
		# res = requests.post('http://localhost:3000/', data={'NickName':config["NickName"], 'message':'release queue'})

		print(res.status_code)
		if res.status_code == 200:
			print('success, starting bot...')
			break
		time.sleep(30)

def login_to_account(hWnd = 0):
	if hWnd == 0:
		hWnd = win32gui.FindWindow("trinityWindow", "EVE")
	rect = win32gui.GetWindowRect(hWnd)
	x1 = rect[0]
	y1 = rect[1]
	x2 = rect[2]
	y2 = rect[3]
	# print('close window with loot')
	# clickLB(1805, 385-23)
	# time.sleep(1)
	# clickLB(1805, 385-23)
	# time.sleep(5)

	height = y2 - y1
	width = x2 - x1
	print('start twin acc')
	clickLB(int(width/2 - 280), int(height/2), hWnd)
	print(int(width/2 - 280), int(height/2))
	time.sleep(0.5)
	clickLB(int(width/2 - 280), int(height/2), hWnd)
	time.sleep(40)

	#control click
	pyautogui.click()
	time.sleep(5)
	pyautogui.moveTo(int(width - 150), int(height/2 + 150))


DownTimeH = 18
DownTimeM = 20
date = datetime.datetime.today()
CurrentTimeH = int(date.strftime('%H'))
CurrentTimeM = int(date.strftime('%M'))
if (CurrentTimeH == DownTimeH and CurrentTimeM < DownTimeM):
	print(f"wait {DownTimeM - CurrentTimeM} min for DT")
	time.sleep(60 * (DownTimeM - CurrentTimeM))


### login window
hWnd = win32gui.FindWindow("trinityWindow", f"EVE - {config['NickName']}")
hWndLoginWnd = win32gui.FindWindow("trinityWindow", "EVE")
print("hWnd: ", hWnd, "\n")
if hWnd == 0 and hWndLoginWnd != 0:
	print('login window')
	login_to_account(hWnd = hWndLoginWnd)
	exit(0)
###


### game window
hWnd = win32gui.FindWindow("trinityWindow", f"EVE - {config['NickName']}")
if hWnd != 0:
	# rect = win32gui.GetWindowRect(hWnd)
	# x1 = rect[0]
	# y1 = rect[1]
	# x2 = rect[2]
	# y2 = rect[3]
	# height = y2 - y1
	# width = x2 - x1
	# pyautogui.moveTo(int(width - 150), int(height/2 + 150))

	MAX_LEN = 256
	buffer_ = ctypes.create_unicode_buffer(MAX_LEN)
	consoleTitle = ctypes.windll.kernel32.GetConsoleTitleW(buffer_, MAX_LEN)
	print(buffer_.value)
	hwndConsole = win32gui.FindWindow(None, buffer_.value)
	rect = win32gui.GetWindowRect(hwndConsole)
	x1 = rect[0]
	y1 = rect[1]
	x2 = rect[2]
	y2 = rect[3]
	pyautogui.moveTo(int(x1 + 150), int(y1 + 150))

	exit(0)
###

# queue_up('queue up on launch game')

### game not start
hWndLauncher = win32gui.FindWindow("Qt5152QWindowIcon", None)
shell = win32com.client.Dispatch("WScript.Shell")
shell.SendKeys('%')
time.sleep(0.5)
win32gui.SetForegroundWindow(hWndLauncher)
time.sleep(0.5)
rect = win32gui.GetWindowRect(hWndLauncher)
x = rect[0]
y = rect[1]


print('start client')
pyautogui.moveTo(x+50, y+145)
time.sleep(0.5)
pyautogui.click()
time.sleep(2)

# queue_up('release queue for launch game')

time.sleep(40)
###

# exit if game not start
hWnd = win32gui.FindWindow("trinityWindow", None)
if hWnd == 0:
	exit(10)

login_to_account()
