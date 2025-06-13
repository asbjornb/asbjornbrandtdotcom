# Wsl

Cheatsheet:

wsl --list                     # List installed WSL distros
wsl -d Ubuntu                 # Launch the 'Ubuntu' distro
wsl                          # Launch default WSL distro
wsl --shutdown                # Shut down all running WSL distros

cd /mnt/c/Users/<YourUsername>  # Access your Windows files from inside WSL
code .                         # Open current WSL folder in VS Code (requires Remote - WSL extension)

sudo dpkg --configure -a      # Repair broken or interrupted package installations
sudo apt-get install -f       # Fix and install missing dependencies
sudo apt update               # Update package list from Ubuntu mirrors
