# PBL3 DEVELOPMENT RULES

> **Dành cho sinh viên**: Quy tắc đơn giản để code và test hiệu quả

## 📋 TODO MANAGEMENT

### **Task Status**
- `[ ]` - Chưa làm
- `[X]` - Hoàn thành  
- `[~]` - Đang làm
- `[!]` - Bị block

### **Priority Levels**
- `CRITICAL` - Hỏng app, phải fix ngay
- `HIGH` - Chức năng quan trọng
- `MEDIUM` - Cải thiện UX
- `LOW` - Tương lai

---

## 🎯 CODING RULES

### **1. MAPBOX ONLY**
- Chỉ dùng Mapbox CDN v3.12.0 (không npm)
- GPS location > IP location
- Docs: https://docs.mapbox.com/help/tutorials/

### **2. YELP-STYLE UX**
- **Trái**: Filters + Restaurant list
- **Phải**: Map only  
- Click card → Navigate to details
- AJAX pagination (không reload page)
- URL: `/restaurant/[slug]`

### **3. COMPONENT SEPARATION**
```html
<div id="filters-section">...</div>
<div id="restaurants-section">...</div>  
<div id="map-section">...</div>
```

### **4. JAVASCRIPT**
- Functions trong `search-map.js`
- Event delegation cho dynamic content
- `console.log()` để debug
- Null checks cho DOM queries

---

## 🧪 TESTING VỚI PLAYWRIGHT

### **IMPORTANT: Windows Playwright Path**
```bash
# Sử dụng npx từ Node.js installation
C:\\nodejs\\npx.cmd playwright test
# KHÔNG DÙNG: npx playwright test (system PATH có thể sai)
```

### **Setup**
```bash
npm init playwright@latest
```

### **Test Categories**

#### **1. Core Search**
```javascript
test('Tìm nhà hàng', async ({ page }) => {
  await page.goto('/Search');
  await page.fill('[data-testid="search-query"]', 'pizza');
  await page.click('[data-testid="search-button"]');
  await expect(page.locator('.restaurant-card')).toHaveCount.greaterThan(0);
});
```

#### **2. Map Integration**
```javascript
test('Map hiển thị markers', async ({ page }) => {
  await page.goto('/Search?query=restaurant');
  await expect(page.locator('.mapboxgl-map')).toBeVisible();
  await expect(page.locator('.mapboxgl-marker')).toHaveCount.greaterThan(0);
});
```

#### **3. AJAX Pagination**
```javascript
test('AJAX pagination không reload', async ({ page }) => {
  await page.goto('/Search?query=restaurant');
  await page.click('[data-page="2"]');
  await page.waitForResponse('/Search/GetRestaurants*');
  await expect(page).toHaveURL(/page=2/);
});
```

### **Test Commands**
```bash
# Test nhanh
npx playwright test --headed tests/current.spec.js

# Test toàn bộ
npx playwright test

# Debug mode
npx playwright test --debug tests/failing.spec.js
```

---

## 🚦 WORKFLOW

### **Bắt đầu task**
1. Update status `[~]` trong TODO.md
2. Viết Playwright test trước (TDD)
3. Code để pass test
4. Log vào TODOLOG.md

### **Hoàn thành task**
1. Chạy `npx playwright test`
2. Mark `[X]` trong TODO.md  
3. Log kết quả vào TODOLOG.md
4. Clean up test files

### **Khi bị block**
1. Mark `[!]` trong TODO.md
2. Ghi lý do vào TODOLOG.md
3. Làm task khác

---

## 📁 FILE ORGANIZATION

- **TODO.md**: Tasks hiện tại only
- **TODOLOG.md**: Logs, test results, findings
- **TODORULE.md**: Rules này (ngắn gọn)
- **tests/**: Playwright test files

### **Test Files Location**
```
test/
├── search-core.spec.js
├── map-integration.spec.js
├── pagination.spec.js
└── responsive.spec.js
```

---

## 🎨 CODING STANDARDS

### **HTML**
```html
<!-- Data attributes cho Playwright -->
<input data-testid="search-query" />
<button data-testid="search-button" />
<div data-testid="restaurant-card" />
```

### **CSS**
- Mobile-first
- BEM naming
- Tránh `!important`

### **C# Backend**
- async/await
- Try-catch
- Entity Framework

---

## 📦 PROJECT DEPENDENCIES

### **ASP.NET Core 9.0**
```xml
<TargetFramework>net9.0</TargetFramework>
```

### **Thư viện chính**
- **Microsoft.AspNetCore.Identity.EntityFrameworkCore** (9.0.5)
  - User authentication & authorization
- **Microsoft.EntityFrameworkCore.SqlServer** (9.0.5)
  - Database ORM
- **Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite** (9.0.5)
  - Spatial data cho coordinates (GIS)
- **Microsoft.AspNetCore.Authentication.Google** (9.0.5)
  - Google OAuth login

### **Pagination & UI**
- **X.PagedList** (10.5.7) - Core pagination
- **X.PagedList.EF** (10.5.7) - Entity Framework integration  
- **X.PagedList.Mvc.Core** (10.5.7) - MVC helpers

### **Development Tools**
- **Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore** (9.0.5)
- **Microsoft.EntityFrameworkCore.Tools** (9.0.5) - Migrations

### **Frontend Libraries (CDN)**
- **Mapbox GL JS** v3.12.0 - Map rendering
- **Bootstrap** - UI framework
- **jQuery** - DOM manipulation

---

## ✅ QUALITY CHECKLIST

- [ ] Code build thành công
- [ ] Playwright tests pass
- [ ] Không có console errors
- [ ] Responsive design OK
- [ ] Test files cleaned up
- [ ] Logs updated trong TODOLOG.md

---

**Version**: Student Simplified  
**Updated**: June 4, 2025

---

## 🚦 WORKFLOW RULES

## 🚦 ENHANCED DEVELOPMENT WORKFLOW WITH PLAYWRIGHT

### **Phase 1: Planning & Analysis**
1. **Requirements Analysis**
   - Break down user stories into testable scenarios
   - Define acceptance criteria with Playwright test cases
   - Create test-first development approach
   
2. **Technical Design**
   - Design with testability in mind
   - Add data-testid attributes for stable selectors
   - Plan API endpoints for both functionality and testing

### **Phase 2: Development Cycle**

#### **Before Starting Work**
1. Check dependencies are completed
2. Update task status to `[~]` (in progress)
3. **Write Playwright test scenarios first** (TDD approach)
4. Create feature branch if major change
5. Review related code and documentation

#### **During Development**
1. **Red-Green-Refactor with Playwright**:
   - Red: Write failing Playwright test
   - Green: Implement minimum code to pass test
   - Refactor: Improve code while keeping tests green
2. Test frequently during development
3. Commit small, logical changes
4. **Update TODOLOG.md with progress notes** (not TODO.md)
5. Document any issues or blockers

#### **Testing Integration Points**
```bash
# Quick test during development
npx playwright test --headed tests/current-feature.spec.js

# Full regression testing
npx playwright test

# Debug mode for investigation
npx playwright test --debug tests/failing-test.spec.js
```

### **Phase 3: Quality Assurance**

#### **After Completing Feature**
1. **Full Playwright Test Suite**:
   ```bash
   npx playwright test --project=chromium
   npx playwright test --project=firefox  
   npx playwright test --project=webkit
   ```
2. **Cross-Browser Validation**:
   - Chrome/Chromium (primary)
   - Firefox (secondary)
   - Safari/WebKit (tertiary)
3. **Responsive Testing**:
   ```bash
   npx playwright test tests/responsive.spec.js
   ```
4. **Performance Testing**:
   - Lighthouse integration
   - Map loading performance
   - AJAX response times

#### **Before Merge/Deploy**
1. Mark task as `[X]` completed
2. **All Playwright tests must pass**
3. Update documentation if needed
4. Clean up any temporary files
5. **Log completion in TODOLOG.md**
6. Move to next priority task

### **Phase 4: Monitoring & Maintenance**

#### **Continuous Integration**
```yaml
# Automated testing on every push
- Run Playwright tests
- Generate test reports
- Upload screenshots/videos on failure
- Block merge if tests fail
```

#### **Regular Maintenance**
- Weekly review of test stability
- Update test selectors if UI changes
- Add new test scenarios for bug fixes
- Archive obsolete tests

### **When Blocked**
1. Mark task as `[!]` blocked
2. **Document in TODOLOG.md what's blocking it**
3. Create new task for the blocker if needed
4. **Run Playwright tests to isolate the issue**:
   ```bash
   npx playwright test --debug tests/problematic-area.spec.js
   ```
5. Work on other tasks while waiting

### **Emergency Debugging Workflow**
```bash
# When something breaks in production
1. npx playwright test --headed tests/critical-paths.spec.js
2. Compare test results with expected behavior
3. Use Playwright trace viewer for detailed analysis:
   npx playwright show-trace trace.zip
4. Create hotfix based on test findings
5. Verify fix with focused test run
```

---

## 🧪 TESTING STRATEGY & PLAYWRIGHT INTEGRATION

### **Test Folder Management**
- Create test files at: `C:\Users\LAPTOP T&T\VIOLETTA\Documents\MQF\StudyMaterial\Second year\4th Semester\PBL3\First\test\`
- Use descriptive names: `mapbox-test.html`, `ajax-test.js`, `playwright-e2e.spec.js`
- Include test purpose in file comments
- Delete test files after issue is resolved
- **Log ALL test results in TODOLOG.md** (not TODO.md)

### **Playwright E2E Testing Framework**

#### **Setup Requirements**
```bash
# Install Playwright
npm init playwright@latest

# Or use VS Code extension
# Install: ms-playwright.playwright
```

#### **Test Categories & Scenarios**

**1. CORE SEARCH FUNCTIONALITY**
```javascript
// Test file: tests/search-core.spec.js
test('Restaurant search with query and location', async ({ page }) => {
  await page.goto('/Search');
  await page.fill('[data-testid="search-query"]', 'pizza');
  await page.fill('[data-testid="search-location"]', 'Hanoi');
  await page.click('[data-testid="search-button"]');
  await expect(page.locator('.restaurant-card')).toHaveCount.greaterThan(0);
});
```

**2. MAP INTEGRATION TESTING**
```javascript
// Test file: tests/map-integration.spec.js
test('Map displays with restaurant markers', async ({ page }) => {
  await page.goto('/Search?query=restaurant&location=hanoi');
  await page.waitForSelector('#map-container');
  await expect(page.locator('.mapboxgl-map')).toBeVisible();
  await expect(page.locator('.mapboxgl-marker')).toHaveCount.greaterThan(0);
});

test('Click restaurant card shows marker popup', async ({ page }) => {
  await page.goto('/Search?query=restaurant&location=hanoi');
  await page.click('.restaurant-card:first-child .btn-detail');
  // Should navigate to restaurant details page
  await expect(page).toHaveURL(/\/restaurant\/.*$/);
});
```

**3. AJAX PAGINATION TESTING**
```javascript
// Test file: tests/pagination.spec.js
test('AJAX pagination updates results without page reload', async ({ page }) => {
  await page.goto('/Search?query=restaurant&location=hanoi');
  
  // Get initial restaurant count
  const initialCount = await page.locator('.restaurant-card').count();
  
  // Click page 2
  await page.click('[data-page="2"]');
  
  // Wait for AJAX to complete
  await page.waitForResponse('/Search/GetRestaurants*');
  
  // Verify new content loaded
  const newCount = await page.locator('.restaurant-card').count();
  expect(newCount).toBeGreaterThan(0);
  
  // Verify URL updated
  await expect(page).toHaveURL(/page=2/);
});
```

**4. FILTER FUNCTIONALITY TESTING**
```javascript
// Test file: tests/filters.spec.js
test('Category filter updates results', async ({ page }) => {
  await page.goto('/Search');
  
  // Select Vietnamese category
  await page.selectOption('#category-filter', 'vietnamese');
  await page.click('#apply-filters-btn');
  
  // Verify filtered results
  await page.waitForResponse('/Search/GetRestaurants*');
  const cards = page.locator('.restaurant-card');
  await expect(cards).toHaveCount.greaterThan(0);
});

test('Distance filter affects map bounds', async ({ page }) => {
  await page.goto('/Search?location=hanoi');
  
  // Set distance filter to 5km
  await page.selectOption('#distance-filter', '5');
  await page.click('#apply-filters-btn');
  
  // Check map bounds updated
  await page.waitForFunction(() => {
    return window.map && window.map.getBounds();
  });
});
```

**5. RESPONSIVE DESIGN TESTING**
```javascript
// Test file: tests/responsive.spec.js
test('Mobile layout displays correctly', async ({ page }) => {
  await page.setViewportSize({ width: 375, height: 667 }); // iPhone SE
  await page.goto('/Search?query=restaurant');
  
  // Check mobile-specific layout
  await expect(page.locator('#filters-section')).toBeVisible();
  await expect(page.locator('#map-section')).toBeVisible();
  
  // Verify touch-friendly button sizes
  const buttons = page.locator('.btn');
  for (const button of await buttons.all()) {
    const box = await button.boundingBox();
    expect(box.height).toBeGreaterThanOrEqual(44); // 44px minimum touch target
  }
});
```

**6. MAPBOX API INTEGRATION TESTING**
```javascript
// Test file: tests/mapbox-api.spec.js
test('Mapbox geocoding works for location search', async ({ page }) => {
  await page.goto('/Search');
  
  // Mock Mapbox geocoding response
  await page.route('**/mapbox.com/geocoding/**', route => {
    route.fulfill({
      json: {
        features: [{
          center: [105.8542, 21.0285], // Hanoi coordinates
          place_name: 'Hanoi, Vietnam'
        }]
      }
    });
  });
  
  await page.fill('[data-testid="search-location"]', 'Hanoi');
  await page.click('[data-testid="search-button"]');
  
  // Verify map centers on Hanoi
  await page.waitForFunction(() => {
    const center = window.map.getCenter();
    return Math.abs(center.lng - 105.8542) < 0.1;
  });
});
```

### **Testing Priorities**
1. **Core Functionality**: Search, pagination, map display
2. **Integration**: API endpoints, AJAX calls, Mapbox integration
3. **UI/UX**: Responsive design, user interactions, accessibility
4. **Performance**: Load times, query efficiency, map rendering
5. **Edge Cases**: Error handling, empty states, network failures

### **Playwright CI/CD Integration**
```yaml
# .github/workflows/playwright.yml
name: Playwright Tests
on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    timeout-minutes: 60
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - uses: actions/setup-node@v3
      with:
        node-version: 18
    - name: Install dependencies
      run: npm ci
    - name: Install Playwright Browsers
      run: npx playwright install --with-deps
    - name: Run Playwright tests
      run: npx playwright test
    - uses: actions/upload-artifact@v3
      if: always()
      with:
        name: playwright-report
        path: playwright-report/
        retention-days: 30
```

---

## 📖 DOCUMENTATION RULES

### **Code Comments**
- Explain WHY, not WHAT
- Document complex business logic
- Include TODO comments for future improvements
- Reference external documentation where applicable

### **TODO.md Updates**
- Update after each completed task
- Include timestamp for major changes
- Document test results and findings
- Track technical debt items

### **Commit Messages**
- Use conventional commit format
- Reference TODO task numbers when applicable
- Include brief description of changes
- Mention any breaking changes

---

## 🎨 UI/UX SPECIFIC RULES

### **Component Architecture**
```html
<!-- ✅ GOOD: Separated components -->
<div id="filters-section">...</div>
<div id="restaurants-section">...</div>  
<div id="map-section">...</div>

<!-- ❌ BAD: Mixed concerns -->
<div class="everything-container">
  <!-- filters mixed with restaurants mixed with map -->
</div>
```

### **Responsive Design Approach**
1. **Mobile First**: Design for mobile, enhance for desktop
2. **Progressive Disclosure**: Show essential info first
3. **Touch-Friendly**: Adequate button sizes (44px min)
4. **Performance**: Optimize images and assets

### **Accessibility Standards**
- Use semantic HTML elements
- Include alt text for images
- Proper heading hierarchy (h1, h2, h3)
- Keyboard navigation support
- ARIA labels where needed

---

## 🔄 CONTINUOUS IMPROVEMENT

### **Regular Reviews**
- Weekly review of completed tasks
- Monthly review of these rules and Playwright test coverage
- Update rules based on lessons learned and test findings
- Archive completed tasks to keep TODO.md manageable
- **Review TODOLOG.md for patterns and insights**

### **Quality Metrics**
- Code builds without warnings
- **All Playwright tests pass across browsers**
- No console errors in browser
- Lighthouse score > 90 for performance
- Cross-browser compatibility verified
- **Test coverage > 80% for critical user journeys**
- **E2E test suite runs in < 10 minutes**

### **File Organization Standards**
- **TODORULE.md**: All rules, guidelines, workflows (this file)
- **TODO.md**: Only current tasks and progress tracking
- **TODOLOG.md**: All development logs, testing results, decisions
- **copilot-instructions.md**: Concise AI guidelines only

---

## 📚 QUICK REFERENCE

### **Essential Commands**
```bash
# Development
dotnet build
dotnet run

# Testing
npx playwright test
npx playwright test --headed
npx playwright test --debug

# Debugging
npx playwright show-trace trace.zip
npx playwright codegen localhost:5000
```

### **Key File Locations**
- **Tests**: `tests/*.spec.js`
- **Map JS**: `wwwroot/js/search-map.js`
- **Search Views**: `Views/Search/`
- **Logs**: `TODOLOG.md`

### **Data Test IDs Convention**
```html
<!-- Use data-testid for stable Playwright selectors -->
<input data-testid="search-query" />
<button data-testid="search-button" />
<div data-testid="restaurant-card" data-restaurant-id="123" />
<div data-testid="map-container" />
```

**Last Updated**: 2025-06-04  
**Version**: 2.0 - Playwright Integration  
**Contributors**: Development Team
