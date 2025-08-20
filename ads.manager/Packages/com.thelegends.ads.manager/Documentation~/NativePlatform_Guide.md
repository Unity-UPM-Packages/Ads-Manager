# AdmobNativePlatform Integration Guide

## Overview

`AdmobNativePlatform` là một định dạng quảng cáo mới được tích hợp vào Ads Manager package. Nó sử dụng native platform implementation để hiển thị quảng cáo native với hiệu suất tối ưu và tùy biến cao.

## Features

- **Native Platform Implementation**: Sử dụng platform-specific implementation để tối ưu hiệu suất
- **Custom Layout Support**: Hỗ trợ custom layout names cho flexible UI integration
- **Event Handling**: Đầy đủ event callbacks cho ad lifecycle
- **Video Support**: Hỗ trợ video ads với full control (play, pause, mute, etc.)
- **Error Handling**: Robust error handling và automatic retry mechanism

## Quick Start

### 1. Configuration

NativePlatform ads được cấu hình tự động trong AdsSettings:

```csharp
// Test Ad Unit IDs đã được cấu hình sẵn
// Android: ca-app-pub-3940256099942544/2247696110
// iOS: ca-app-pub-3940256099942544/3986624511
```

### 2. Loading Ads

```csharp
// Load NativePlatform ad
AdsManager.Instance.LoadNativePlatform(PlacementOrder.One);
```

### 3. Showing Ads

```csharp
// Show with default layout
AdsManager.Instance.ShowNativePlatform(PlacementOrder.One, "position_name");

// Show with custom layout
AdsManager.Instance.ShowNativePlatform(PlacementOrder.One, "position_name", "custom_layout");
```

### 4. Hiding Ads

```csharp
AdsManager.Instance.HideNativePlatform(PlacementOrder.One);
```

### 5. Checking Ad Status

```csharp
var status = AdsManager.Instance.GetAdsStatus(AdsType.NativePlatform, PlacementOrder.One);

switch (status)
{
    case AdsEvents.LoadAvailable:
        // Ad is ready to show
        break;
    case AdsEvents.LoadRequest:
        // Ad is currently loading
        break;
    case AdsEvents.LoadFail:
        // Ad failed to load
        break;
    // ... other status cases
}
```

## Advanced Usage

### Custom Layout Names

NativePlatform hỗ trợ custom layout names để integrate với different UI layouts:

```csharp
// Show with specific layout for different screens
AdsManager.Instance.ShowNativePlatform(PlacementOrder.One, "main_menu", "main_menu_layout");
AdsManager.Instance.ShowNativePlatform(PlacementOrder.One, "game_over", "game_over_layout");
```

### Event Handling

AdmobNativePlatformController cung cấp các events:

- `OnAdPaid`: Triggered when ad generates revenue
- `OnAdClicked`: Triggered when user clicks ad
- `OnAdDidRecordImpression`: Triggered when ad impression is recorded
- `OnVideoStart/End/Play/Pause`: Video-specific events
- `OnVideoMute`: Video mute state changes

### Best Practices

1. **Load Early**: Load ads trước khi cần show để đảm bảo availability
2. **Check Status**: Luôn check ad status trước khi show
3. **Handle Failures**: Implement retry logic cho failed loads
4. **Layout Naming**: Sử dụng descriptive layout names for maintainability

```csharp
public void ShowAdIfReady()
{
    var status = AdsManager.Instance.GetAdsStatus(AdsType.NativePlatform, PlacementOrder.One);
    
    if (status == AdsEvents.LoadAvailable)
    {
        AdsManager.Instance.ShowNativePlatform(PlacementOrder.One, "main_menu");
    }
    else if (status != AdsEvents.LoadRequest)
    {
        // Load if not currently loading
        AdsManager.Instance.LoadNativePlatform(PlacementOrder.One);
    }
}
```

## Demo Script

Sử dụng `NativePlatformDemo.cs` để test functionality:

```csharp
// Attach NativePlatformDemo to a GameObject
// Connect UI buttons to test different functions
// Monitor status text để track ad state changes
```

## Platform Support

- **Android**: ✅ Fully supported
- **iOS**: ⚠️ Implementation ready (cần iOS-specific client)
- **Editor**: ✅ Mock implementation for testing

## Troubleshooting

### Common Issues

1. **Ad not loading**: Check internet connection và ad unit IDs
2. **Ad not showing**: Verify ad status is `LoadAvailable`
3. **Layout not found**: Ensure layout name matches platform implementation

### Debug Logging

Enable debug logging để track ad lifecycle:

```csharp
// Logs được tự động generate cho tất cả ad events
// Check Unity Console for detailed information
```

## API Reference

### AdsManager Methods

- `LoadNativePlatform(PlacementOrder order)`
- `ShowNativePlatform(PlacementOrder order, string position)`
- `ShowNativePlatform(PlacementOrder order, string position, string layoutName)`
- `HideNativePlatform(PlacementOrder order)`

### AdmobNativePlatformController

- Extends `AdsPlacementBase`
- Implements standard ad controller pattern
- Supports custom layout names
- Full event handling support

### AdsType Enum

New value added: `AdsType.NativePlatform = 10`

---

*Được tạo bởi Ads Manager Package v1.0*
