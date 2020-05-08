
# Thunderbolt3TryAndError
Just a simple Service I created to handle the Thunderbolt 3 eGPU Problems with my Laptop.
This can be useful for the *2015 Acer Aspire VN7-592g* and *VN7-792g* when in use with a NVIDIA eGPU.

# Installation

You need to disable the PCIe Root Port for the NVIDIA dGPU, follow the instructions here:
https://egpu.io/forums/builds/2015-15-acer-aspire-v15-vn7-592g-gtx-960m-6th4chq-rtx-2060-super-40gbps-tb3-razer-core-x-win10-v3n3

In order to install this Service, put it somewhere safe and start the .exe with "install" as argument (or "uninstall" to remove as service).

Now you can either start the Service by hand or simply reboot your Computer.

Sadly you´re not done yet, you need set some settings for the Service in Order to find your devices. Yes, I could´ve automate this, but its not really worth it to put all this time into that feature to save 10mins you have to do once.

First, open the Registry-Editor.
Navigate to 'HKEY_LOCAL_MACHINE\SOFTWARE\Thunderbolt3TryAndErrorService', where you should see 3 Empty Values.

In order to fill these fields you need to open the Device-Manager.
Go into the "Graphicscard" Category, go into the Properties of the NVIDIA eGPU (with Error 43), change into "Details" and look for the Property "Device instance path" (or something along those lines, its at the very top).
![enter image description here](https://i.imgur.com/9eoQbAm.png)
Copy the Path and insert it in the Registry as "graphicsCardPath".

Click "View" at the top and "Devices by Connection"
![enter image description here](https://i.imgur.com/Saz0lvb.png)

Search for the PCIe Root Port, where you find the Thunderbolt 3 Controller and problematic NVIDIA GPU under.
Do the same with the Root Port as you did with the GPU.
![enter image description here](https://i.imgur.com/5sIzmqG.png)
Copy the Path and insert it in the Registry as "pciRootPath".

Now the tricky part, look for the "Class-GUID"
![enter image description here](https://i.imgur.com/EMDSJiz.png)
The last step should seem familiar, copy the Path and insert it in the Registry as "pciRootClass".

The end product should look like this:
![enter image description here](https://i.imgur.com/wtpweoB.png)

"checkInterval" is the amount of miliseconds the Service waits until its searching for a GPU again.
