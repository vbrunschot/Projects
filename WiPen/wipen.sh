#!/bin/bash

### Variables
basepath="/home/mystr0/wipen"
#project=`date +%s`

# Ansi color code variables
orange="\e[0;33m"
red="\e[0;31m"
blue="\e[0;94m"
expand_bg="\e[K"
lgreen="\e[0;47m"
white="\e[0;97m"
bold="\e[1m"
uline="\e[4m"
reset="\e[0m"

setup_interfaces() {
 print_banner
 echo -e "${orange}Setting up network interface..${reset}"
 sudo ifconfig wlan0 down
 sudo ifconfig wlan1 down
 echo -e "${orange}Change MAC addresses${reset}"
 sudo macchanger -r wlan0
 echo ""
 sudo macchanger -r wlan1
 echo -e "${orange}Change TX power..${reset}"
 sudo ifconfig wlan1 up
 sudo iw reg set BO
 sudo iwconfig wlan1 txpower 30
 echo -e "${orange}Check for intefering processes..${reset}"
 sudo airmon-ng check kill
 echo -e "${orange}Start monitor mode${reset}"
 sudo airmon-ng start wlan1
 echo -e "${orange}Network interfaces:${reset}"
 sudo ifconfig
}

start_gps() {
echo -e "${orange}Start gpsd service..${reset}"
sudo service gpsd start
sudo service gpsd status
gpsmon
}

stop_gps() {
echo -e "${orange}Stopping gpsd service..${reset}"
sudo service gpsd stop
sudo service gpsd status
}

create_new_project() {
  print_banner
  echo -e "${orange}"
  read -r -p "Set project name? [Y/n] " input
  case $input in
      [yY][eE][sS]|[yY])
  echo ""
  echo -e "Project name: ${reset}"
  read project
   ;;
      [nN][oO]|[nN])
	project=`date +%s`  # Set epoch as name.
         ;;
      *)
   echo -e "${orange}Invalid input...${reset}"
   exit 1
   ;;
  esac

  echo ""
  echo -e "${orange}Enter description:${reset}"
  read notes

  ### Create directories and files
  date=`date`
  mkdir $basepath/$project
  cd $basepath/$project
  echo $date > notes
  echo >> notes
  echo $notes >> notes
}

capture_pmkids() {
print_banner
mkdir $basepath/$project/hcxdumptool
cd $basepath/$project/hcxdumptool

  echo -e "${orange}"
  read -r -p "Log GPS data? [Y/n] " input
  case $input in
      [yY][eE][sS]|[yY])
  echo -e "${orange}Capturing PMKID's (with GPS)${reset}"
  sudo hcxdumptool -i wlan1mon -o hcxdumptool.pcapng --nmea=hcxdumptool.nmea --use_gpsd --enable_status=1 2>&1 | tee hcxdumptool.log
  echo -e "${orange}Grabbing PMKID's from pcapng file${reset}"
  sudo hcxpcapngtool -o pmkids.22000 --nmea=hcxpcapngtool.nmea hcxdumptool.pcapng
  echo -e "${orange}Creating gpx file${reset}"
  gpsbabel -i nmea,date=20201212 -f hcxdumptool.nmea -o gpx -F gps-locations.gpx
   ;;
      [nN][oO]|[nN])
           echo -e "${orange}Capturing PMKID's (without gps)${reset}"
           sudo hcxdumptool -i wlan1 -o hcxdumptool.pcapng --enable_status=1 2>&1 | tee hcxdumptool.log
	   echo -e "${orange}Grabbing PMKID's from pcapng file${reset}"
	   sudo hcxpcapngtool -o pmkids.22000  hcxdumptool.pcapng
         ;;
      *)
   echo -e "${orange}Invalid input...${reset}"
   # exit 1
   ;;
  esac
}

start_wifite() {
print_banner
mkdir $basepath/$project/wifite
mkdir $basepath/$project/wifite/hs
cd $basepath/$project/wifite
echo -e "${orange}Set scan time (s):${reset}"
read scan_time
sudo wifite -i wlan1mon -p $scan_time
}

start_kismet() {
mkdir $basepath/$project/kismet
cd $basepath/$project/kismet
sudo kismet
}

remove_project() {
print_banner
echo -e "${orange}Removing project files..${reset}"
sudo rm -r $basepath/$project
project=""
}


stop_monitor_mode() {
print_banner
echo -e "${orange}Stopping monitor mode${reset}"
sudo airmon-ng stop wlan1mon
}

press_enter() {
  echo -e "${orange}"
  echo -e -n "	Press Enter to continue.. ${reset}"
  read
  clear
}

incorrect_selection() {
  print_banner
  echo -e "${orange}	Incorrect selection!${reset}"
}

print_banner() {
  clear
  echo -e "${orange}${bold}"
  echo "	  WiPen - Wifi Pentesting Tool"
  echo -e "${reset}${orange}"
}

### Menu
until [ "$selection" = "0" ]; do
  print_banner
  echo -e "    Current project: ${red} $project ${reset}${orange}"
  echo ""
  echo -e "${bold}      *===========[ Setup ]=============*${reset}${orange}"
  echo "    	1  -  Setup network interfaces"
  echo "        2  -  Start GPS"
  echo "    	3  -  Create new project"
  echo ""
  echo -e "${bold}      *===========[ Pentest ]============*${reset}${orange}"
  echo "        4  -  Capture PMKID's"
  echo "	5  -  Start Wifite"
  echo "        6  -  Start Kismet"
  echo ""
  echo -e "${bold}      *===========[ Misc ]==============*${reset}${orange}"
  echo "        7  -  Remove project files"
  echo "	8  -  Stop GPS"
  echo "	9  -  Stop monitor mode"
  echo "    	0  -  Exit"
  echo ""
  echo -n "  Enter selection: "
  read selection
  echo -e "${reset}"
  case $selection in
    1 ) clear ; setup_interfaces ; press_enter ;;
    2 ) clear ; start_gps ; press_enter ;;
    3 ) clear ; create_new_project ; press_enter ;;
    4 ) clear ; capture_pmkids ; press_enter ;;
    5 ) clear ; start_wifite ; press_enter ;;
    6 ) clear ; start_kismet ; press_enter ;;
    7 ) clear ; remove_project ; press_enter ;;
    8 ) clear ; stop_gps ; press_enter ;;
    9 ) clear ; stop_monitor_mode ; press_enter ;;
    0 ) clear ; exit ;;
    * ) clear ; incorrect_selection ; press_enter ;;
  esac
done
