# IMD Lab - EEG Toolkit
## Usage

Dependencies 
[Python (tested in v3.12.9). Pip.]
Microsoft Visual C++ 14.0 or greater is required. Get it with "Microsoft C++ Build Tools": https://visualstudio.microsoft.com/visual-cpp-build-tools/
 - Tested on Windws 11 and Mac (use pip3 & python3 if on Mac)

```
 pip install hidapi pycryptodome pylsl
 pip install numpy
 pip install mne hypyp
 pip install pynput
 pip install python-osc
```
Connect EEG headset with USB receiver, wait for the light indicators (guide from [Emotiv](https://emotiv.gitbook.io/emotivpro-v3/emotivpro-menu/pairing-your-eeg-headset-with-a-usb-receiver-dongle)). 

```
  #first terminal
  python main.py
```

Run band analysis and sent via OSC.

```
  #second terminal
  python tools/band-OSC.py
```

## Other tools for visualizing data: 

Run and save raw files only:

```
  #second terminal
  python tools/singleCh_read_and_export_mne.py
```

Plot raw data (including all 14 channels):

```
  python tools/plot_raw_data.py
```

Read .csv from .fif file:

```
  python tools/read_csv.py
```
