# Manual Testing Guide for Dual-Mode Account Management

## Quick Test Checklist

### 🔧 Setup
1. Start the application: `dotnet run` in PBL3 folder
2. Navigate to the application in browser
3. Ensure you have at least 2 user accounts for testing

### 🧪 Test Cases

#### Test 1: Own Profile Management
**Steps:**
1. Login with any user account
2. Navigate to `/Account/Manage`
3. **Verify Navigation Menu Shows:**
   - ✅ Hồ sơ cá nhân (active)
   - ✅ Bảo mật
   - ✅ Đổi mật khẩu  
   - ✅ Đánh giá của tôi
   - ✅ Quản lý nhà hàng (external link)
4. **Verify Profile Section:**
   - ✅ Editable form fields
   - ✅ Save button present
   - ✅ Avatar upload button visible
5. **Test Navigation:**
   - ✅ Click each menu item - corresponding section shows
   - ✅ Security section loads (2FA settings)
   - ✅ Change password form loads
   - ✅ Reviews section loads
6. **Test External Link:**
   - ✅ Click "Quản lý nhà hàng"
   - ✅ Opens https://restaurant-management.fishloot.com in new tab

#### Test 2: Other User Profile View
**Steps:**
1. While logged in, navigate to `/Account/Manage?userId={another-user-id}`
   - Replace `{another-user-id}` with actual user ID from database
2. **Verify Navigation Menu Shows:**
   - ✅ Only "Hồ sơ cá nhân" (basic info)
   - ❌ No Bảo mật section
   - ❌ No Đổi mật khẩu section
   - ❌ No Đánh giá section
   - ❌ No restaurant management link
3. **Verify Profile Section:**
   - ✅ Read-only information display
   - ❌ No editable form fields
   - ❌ No save button
   - ❌ No avatar upload button
4. **Verify Header:**
   - ✅ Shows "Thông tin cá nhân" instead of "Quản lý tài khoản"

#### Test 3: Security Validation
**Steps:**
1. While viewing another user's profile, attempt to:
   - Try to access `/Account/Manage` POST with other user's data
   - **Expected:** 403 Forbidden or redirect
2. Verify no sensitive information is exposed in read-only view

#### Test 4: Invalid User ID
**Steps:**
1. Navigate to `/Account/Manage?userId=invalid-id`
2. **Expected:** 404 Not Found page

## 🐛 Common Issues to Check

### Navigation Issues
- **Problem:** Clicking menu items doesn't switch sections
- **Check:** JavaScript console for errors
- **Fix:** Verify jQuery is loaded and no JS conflicts

### Conditional Display Issues  
- **Problem:** Sections show when they shouldn't
- **Check:** View source to verify conditional rendering
- **Fix:** Verify `IsViewingOwnProfile` property is set correctly

### External Link Issues
- **Problem:** Restaurant management link doesn't work
- **Check:** Link URL and target="_blank" attribute
- **Fix:** Verify URL is correct: https://restaurant-management.fishloot.com

## 🔍 Debug Information

### Check Model Data
Add temporary debug output in view:
```html
<!-- Debug Info (remove in production) -->
<div style="background: #f0f0f0; padding: 10px; margin: 10px;">
    <strong>Debug:</strong><br>
    Current User ID: @ViewContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value<br>
    Target User ID: @Model.Id<br>
    IsViewingOwnProfile: @Model.IsViewingOwnProfile
</div>
```

### Controller Debug
Add breakpoints in AccountController.cs Manage method to verify:
- `userId` parameter value
- `isViewingOwnProfile` calculation
- `targetUser` retrieval

## ✅ Success Criteria

All tests pass when:
- ✅ Own profile shows full management interface
- ✅ Other profiles show limited read-only view  
- ✅ Navigation adapts to viewing context
- ✅ Security restrictions properly enforced
- ✅ External restaurant link works
- ✅ No unauthorized access possible

## 📝 Notes

- Test with different user roles if applicable
- Test with users who have/don't have restaurants
- Test 2FA enabled/disabled scenarios
- Test email confirmed/unconfirmed scenarios
- Verify mobile responsiveness if needed
