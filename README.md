# Split Single Phase Energy Meter to LED-digits

Allows CircuitSetup's ATM90E32 Split Single Phase Energy Meter (see https://github.com/CircuitSetup/Split-Single-Phase-Energy-Meter) 
total Amperage to be shown on serial addressable LED digits like the Velleman K8063

- Copy all the files

        count.dat
        count.exe (in this build, when run without paramaters, it simply displays whatever number is in count.dat)
        count.ini (update with the correct COM port and number of digits)
        update.bat

- Copy the following CURL files (you can get these at https://curl.haxx.se/windows/)

        curl.exe
        curl-ca-bundle.crt
        libcurl-x64.dll

- Create a scheduled task that runs update.bat as often as you like.

[End of Line]
