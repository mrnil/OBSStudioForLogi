# CI Dependencies

This directory contains dependencies required for dependency checking that are not available via NuGet.

## PluginApi.dll

This is the Loupedeck Plugin API assembly required to build the plugin. It's included here for dependency validation only since it's not available on GitHub Actions runners.

**Source:** Logi Plugin Service installation  
**License:** Proprietary (Loupedeck/Logitech)  
**Purpose:** Compile-time reference for dependency validation
