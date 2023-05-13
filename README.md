# AreYouSleeping
Fancy sleep timer for Windows

## What is it?
If you often fall asleep while watching movies or series, this is for you. Similar to a sleep timer on a TV, this app will stop what you're watching or turn off your PC. You can set a desired timer period, and you are free to choose what happens when the time has passed. The app will first ask if you are actually sleeping (if you have not fallen asleep, you can postpone for later) and only then will stop what you're watching.

## Features
- Select desired timer duration (of course) 
- Select different action modes after timer has elapsed
  - Close browser tabs: in this mode, the app will find specific tab(s) opened in your browser and close them. Currently only supported in Chrome.
    - Predefined tabs (some popular streaming services)
    - Custom defined tabs (specify your own rules for tabs to close)
  - Close browser processes: in this mode, the app will close all browser windows (on Chrome, Edge, and Firefox)
  - Sleep: in this mode, the app will put your computer to sleep when the timer passes
  - Shutdown: in this mode, the app will turn off your computer
- UI Language support
  - English
  - Bulgarian

## How to install
### System reqirements
The app works only on Windows

### Installation steps
1. Download the zip archive of the latest release from the [Releases page](https://github.com/svetoslav-maksimov/AreYouSleeping/releases) named ```are-you-sleeping-<version>.zip```
2. Unzip somewhere on your computer where you can easily find it   
3. That's it! You can start the app by running ```AreYouSleeping.exe```

## How to use
![](/docs/img/main_window_01.png)

1. Start the app if it is not started
2. Select the timer duration from the "Sleep Timer" drop-down list
3. Select the action mode from the "Mode" drop-down list
   a. If you selected _Close brower tab_ mode, check which streaming service tabs to find and close
   b. If you don't see your streaming service, you can specify it by checking "Custom" and adding a search pattern (with an regular expression). For example: ```HBO.*```. Note: the ```.*``` symbols means "match any character after HBO", such as _HBO GO_ or _HBO MAX_
4. Click the "Start" button to start the timer. You can now safely close the main app window, and you can still show it from the app icon that is located in the Windows taskbar. If you want to close the app entirely, right click on the taskbar icon and select "Exit"
5. Start watching your movie or series and enjoy
6. When the time passes, the app will ask you if you're really asleep or not. If you don't answer for 30 seconds, the app will consider you asleep and will then turn off your movie in the way you specified in the "Mode" selection.
![](/docs/img/prompt_window_01.png)

## How to contribute
The app is build with .NET 6 in Visual Studio. If you want to run it from the source code, just clone the repo and open the solution in Visual Studio 2022. You can get the Community edition of VS for free if you don't have it, or you can use VSCode as well.