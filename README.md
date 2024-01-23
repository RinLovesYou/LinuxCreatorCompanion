<div align="center">
<h3 align="center">LinuxCreatorCompanion</h3>

  <p align="center">
    A Wrapper around VRChat's Creator Companion to make it work on Linux
    <br />
    <br />
    <a href="https://github.com/RinLovesYou/LinuxCreatorCompanion/issues">Report Bug</a>
    ·
    <a href="https://github.com/RinLovesYou/LinuxCreatorCompanion/issues">Request Feature</a>
  </p>
</div>

## Disclaimer
This is a *dirty* port. Things have been hastily patched for surface level functionality. <br>
In the future I will clean things up, make the install process more automated, etc.

### I HIGHLY RECOMMEND BACKING UP ALL YOUR PROJECTS BEFORE USING THIS

---

![img.png](Readme/img.png)

## Setup
You must install WINE. It is used for extracting VCC files from the installer.

You can simply run `./LinuxCreatorCompanion`. 

If any of the VCC files are missing,
it will automatically download the VCC version compatible with LCC, extract all needed files, and start itself.

The LinuxCreatorCompanion has *only* been tested on Arch Linux, and as such, might not be able to find
the paths for Unity/UnityHub. Currently the manual specifying of paths has not been properly implemented, so I would
appreciate if you opened an Issue.

If you find your windows flickering while resizing, This seems to be a WebKit issue. `WEBKIT_DISABLE_COMPOSITING_MODE=1` may fix this.

---

## What works?
The surface level functionality has been tested.

* Projects can be created and managed
* Packages can be installed
* Unity can be launched

If you find any problems, please open an Issue.

---

## Building
You will need to grab the following files from your LinuxCreatorCompanion directory, after running the setup above.
* CreatorCompanion.dll
* vcc-lib.dll
* vpm-core-lib.dll

And place them in a `Libs` folder in the Solution Directory.

## Credits
Innounp - https://innounp.sourceforge.net/

---

LinuxCreatorCompanion is not endorsed by VRChat and does not reflect the views or opinions of VRChat or anyone officially involved in producing or managing VRChat properties. VRChat and all associated properties are trademarks or registered trademarks of VRChat Inc. VRChat © VRChat Inc.