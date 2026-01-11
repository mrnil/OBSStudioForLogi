# Product Overview

## Project Purpose
OBSStudioForLogiPlugin is a Loupedeck/Logi Plugin Service integration that enables hardware control of OBS Studio (Open Broadcaster Software) through Loupedeck devices. The plugin provides a bridge between physical control surfaces and OBS Studio's WebSocket API, allowing streamers and content creators to control their broadcasts with dedicated hardware buttons and displays.

## Value Proposition
- **Hardware Integration**: Seamlessly connects Loupedeck hardware devices with OBS Studio for tactile broadcast control
- **Real-time Control**: Provides instant access to recording, scene switching, and profile management without keyboard/mouse interaction
- **Professional Workflow**: Enables streamlined broadcast operations through dedicated physical controls
- **Visual Feedback**: Displays current status (scenes, profiles, recording state) directly on hardware device screens

## Key Features

### Recording Control
- Start/stop recording with dedicated commands
- Toggle recording on/off
- Pause and resume recording during active sessions
- Visual indicators for recording state (on/off/paused)

### Scene Management
- Switch between OBS scenes dynamically
- Display current active scene on device
- Visual feedback showing selected vs unselected scenes
- Dynamic scene list updates when OBS configuration changes

### Profile & Scene Collection Management
- Switch between OBS profiles
- Switch between scene collections
- Display current profile and scene collection
- Automatic synchronization when profiles/collections change in OBS

### Screenshot Capture
- Trigger OBS screenshots directly from hardware
- Automatic path detection (Pictures, Documents, or Desktop folders)

### Connection Management
- Automatic connection to OBS WebSocket on application start
- Direct connection fallback when OBS process detection fails
- Port availability monitoring before connection attempts
- Graceful reconnection handling

## Target Users

### Primary Users
- **Live Streamers**: Content creators broadcasting on Twitch, YouTube, or other platforms who need quick access to scene switching and recording controls
- **Video Producers**: Professionals recording content who benefit from hardware-based recording controls
- **Podcast Hosts**: Creators managing multiple scenes and sources during live recordings

### Use Cases
1. **Live Streaming**: Quick scene transitions, recording control, and status monitoring during live broadcasts
2. **Content Recording**: Start/stop recording sessions with physical buttons instead of software controls
3. **Multi-Scene Productions**: Rapid switching between different camera angles, screen captures, and overlay configurations
4. **Profile Management**: Switching between different OBS configurations for various show formats or streaming platforms
5. **Professional Broadcasting**: Hardware-based control for more reliable and tactile operation during critical live events

## Technical Integration
- Integrates with OBS Studio via obs-websocket protocol (v5.0+)
- Reads OBS configuration files to discover WebSocket connection settings
- Supports both Windows and macOS platforms
- Compatible with Logi Plugin Service infrastructure
