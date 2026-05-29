# CI Dependencies

This directory contains dependencies required for CI builds that are not available via NuGet.

## PluginApi.dll

This is the Loupedeck Plugin API assembly required to build the plugin. It's included here for CI purposes only since it's not available on GitHub Actions runners.

**Source:** Logi Plugin Service installation  
**License:** Proprietary (Loupedeck/Logitech)  
**Purpose:** Compile-time reference only for CI builds
