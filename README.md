# Universal Telemetry Replay

***
[[_TOC_]]
***

## Getting Started
1. At first start, you'll be greeted with the 'Home' view UI. 
2. Click 'Settings' and make sure you're happy with the parsing limits. 
3. Click 'Message Configurations' to create a new configuration or to load an existing one. 
4. Go back to 'Replay Home', click 'Add Log to Replay'
5. On the added log item, Click 'Browse' for the file you want to replay. You may select '.dig', '.bin', or any other type. 
   - You will see an item color assigned to this particular log. This is not changeable. 
6. Set the IP and Port number you wish to broadcast the log over. 127.0.0.1 is default and will only send locally. 
   - NOTE: If sending locally, you will not be able to see the same packet via multiple application. To do so, you must broadcast to an actual endpoint, not loopback.
7. After adding any logs you wish to replay, click the 'Load' button location bottom center. The application will attempt to parse the data and match it to a configuration. 
8. Via the bottom panel, select a 'Playback Style' and 'Playback Speed'
9. Click Play. 
   - You may drag the slider left/right to access the desired area of the playback. 
10. You may 'Pause' or 'Stop' the replay at any moment.
11. Click 'Reset' to unlock the logs. 


## Home View
![home_view.png](/.attachments/image-8b12de3a-eb91-414f-828e-d6bcf80876db.png =500x)

### How to add a log file
Click "Add Log to Replay'. A new replay item will be appended to the list. 
![image.png](/.attachments/image-0012a87a-1db8-4872-bb5f-880c4d13b5e1.png =500x)
Click 'Browse' and select the desired file. 
   - File Types:
      - *.dig
      - *.bin
      - *.* (all)

Input the desired IP and Port for replay. This will replay the Telemetry over UDP via Broadcast.
   - Note: Default is 127.0.0.1 (loopback). Loopback can only be captured by one application, please keep this in mind. 

### Loading / Parsing
After you're done adding files to be replayed, click the 'Load' button located bottom center. The application will attempt to parse the files, indicating which file is being parsed via the files 'Status' indicator. If it successfully parses the file, finding a proper configuration, it will index the file and create a temporary copy. This allows for replay of duplicate telemetry files simultaneously. 
![image.png](/.attachments/image-0c624d8e-34cb-4bcd-ad4b-f942482775e7.png)

After file(s) are parsed, you'll see the status indicated and the matched configuration displayed. 
You will also see Start and End times for each telemetry file, as well as the total number of packets in the file, with an indication of the number of packets replayed. 
![image.png](/.attachments/image-9c8d6239-7e44-49c0-81e9-6fc736af5ee7.png =500x)

### Playback Style
There are two supported playback styles. Concurrent and Time Sync'd. You will be able to visually see the time scaler for each file move to the appropriate location on the time/percent slider. 

   - NOTE: If any parsed configurations do not have timestamp locations/size, the only replay mode available will be 'Concurrent'.

   - Concurrent - This will play each file immediately.
![image.png](/.attachments/image-62461f74-d537-452c-b8ba-1fc8e2054371.png)
   - Time Sync - This will play the files in a time synchronized manner based on the timestamp it found within the message packet.
![image.png](/.attachments/image-21439bab-e6b2-4796-bc56-72ab0c69bfe9.png)

### Playback Speed
Configurable before and during replay. Just click the dropdown and chose your desired setting.
   - Playback Speeds
      - 0.25x 
      - 0.5x
      - 1x
      - 1.5x
      - 2x
      - 3x
      - 5x

### Playback Location
You can adjust the thumb of the slider to any desired location you wish. The replay will adjust and continue replaying from the desired location. 

## Configuration View
![image.png](/.attachments/image-1c8f8b5c-d0c7-43f7-a11f-07d3cd0573b4.png =500x)

Any configurations entered are immediately saved to the programs files area for safe keeping. However, this also applies for removing of configurations. 

### How to add a configuration: 
Enter data into the input fields on the middle/left of the screen.

   - Mandatory Items: 
      - Configuration Name
      - Sync Byte 1
      - Sync Byte 2
      - Message Size
      - End Byte 1

### Timestamp Scaling
You may enter basic mathematical equations for the scaling. The field will perform the calculation and use the value. 
   - Example: Enter '2^14' and the field will display '16384'

### How to edit a configuration
Select the configuration you'd like to edit. The input fields will update to reflect the current data. Make any modifications needed and click 'Add/Update Item'

Click 'Reset Fields' if you'd like to undo any changes. 

### How to remove a configuration
Select the field you want to remove, and click 'Remove Item'. 

### How to share / save a config
Click the 'Export Configs' button and select a desired download location. This will print a JSON formatted file containing the config items. 

### How to load a config file
Click the 'Load Configs' button and select the desired file. The information will be parsed and the 'Configurations' table will update appropriately.

Note - if you upload a configuration file, this will wipe any current configurations you currently have. 

![image.png](/.attachments/image-b25a117a-04db-4892-b34a-d681cbf23bfc.png =600x)

## Settings View
![image.png](/.attachments/image-716acdd3-c207-4aaf-aa44-2b929b17192e.png =500x)

Note: Settings are saved immediately after edits. Application window size, position, and location is also saved for convenience. 

* Available Setting:
  - Theme: Simple enough to understand, pick a color theme to match your taste. 
  - Log Limit: The maximum number of logs that can be added/replayed at once. 
     - One - One log is the max replay items.
     - Five - DEFAULT - One log is the max replay items.
     - Ten - One log is the max replay items.
  - Parse Limit: The maximum amount of the file to parse for a valid message. 
     - None - No parse limit, will parse full file to find a valid message for any configuration.
     - Percent10 - 10% of the file size.
     - Percent25 - 25% of the file size.
     - One Message - DEFAULT - One message worth of data based on configuration message size.
     - Five Messages - Five message worth of data based on configuration message size.
     - Ten Messages - Ten message worth of data based on configuration message size.
