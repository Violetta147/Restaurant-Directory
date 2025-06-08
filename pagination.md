
# Pagination Code Documentation

This document contains all CSS, HTML, and JavaScript code related to pagination in the project.

## HTML Structure

```html
<!-- Pagination - Always visible container with explicit styling -->
<div class="pagination-container" style="padding:10px; background-color:#f8f9fa; border:1px solid #dee2e6; text-align:center; width:100%;">
    @if (Model.RestaurantCards != null && Model.RestaurantCards.PageCount > 1) {
        <nav style="display:block !important; visibility:visible !important;">
           <div class="pagination-debug" style="margin-bottom:10px; font-weight:bold;">Pages: 1 to @Model.RestaurantCards.PageCount (Current: @Model.RestaurantCards.PageNumber)</div>
           <ul class="pagination" style="display:flex !important; justify-content:center !important; padding:0; margin:0; list-style:none;">
               @if (Model.RestaurantCards.HasPreviousPage) { <li style="display:inline-block !important; margin:0 5px;"><a style="display:block !important; padding:10px 15px; background:#007bff; color:#fff; text-decoration:none; border-radius:4px;" href="javascript:loadPage(@(Model.RestaurantCards.PageNumber - 1))">«</a></li> }
               @for (int i = 1; i <= Model.RestaurantCards.PageCount; i++) { 
                   <li style="display:inline-block !important; margin:0 5px;">
                       <a style="display:block !important; padding:10px 15px; background:@(i == Model.RestaurantCards.PageNumber ? "#0056b3" : "#007bff"); color:#fff; text-decoration:none; border-radius:4px; font-weight:@(i == Model.RestaurantCards.PageNumber ? "bold" : "normal");" 
                          href="javascript:loadPage(@i)">@i</a>
                   </li> 
               }
               @if (Model.RestaurantCards.HasNextPage) { <li style="display:inline-block !important; margin:0 5px;"><a style="display:block !important; padding:10px 15px; background:#007bff; color:#fff; text-decoration:none; border-radius:4px;" href="javascript:loadPage(@(Model.RestaurantCards.PageNumber + 1))">»</a></li> }
           </ul>
        </nav>
    }
</div>
```

## CSS Styles

### From search-results.css

```css
/* Pagination container styling */
.pagination-container .pagination .page-link {
    display: flex;
    visibility: visible;
    opacity: 1;
    justify-content: center;
    align-items: center;
    min-width: 40px;
    min-height: 40px;
    position: relative;
    z-index: 65;
    border: 1px solid #dee2e6;
    background-color: #fff;
    color: #0d6efd;
    text-decoration: none;
    border-radius: 4px;
    font-size: 16px;
    font-weight: 500;
    transition: all 0.2s ease;
}

/* Pagination active state */
.pagination-container .pagination .page-item.active .page-link {
    background-color: #0d6efd;
    border-color: #0d6efd;
    color: white;
    font-weight: 600;
    z-index: 66;
    box-shadow: 0 0 0 3px rgba(13, 110, 253, 0.5);
}

/* Pagination hover state */
.pagination-container .pagination .page-item:hover .page-link {
    background-color: #e9ecef;
    border-color: #0d6efd;
    color: #0d6efd;
    z-index: 65;
}

/* For forcing visibility */
.page-link.force-visible {
    display: flex !important;
    visibility: visible !important;
    opacity: 1 !important;
}
```

### From restaurant-card.css

```css
/* Basic pagination styling */
.pagination .page-link {
    color: #0d6efd;
    border: 1px solid #dee2e6;
    padding: 0.5rem 0.75rem;
    margin: 0 2px;
    border-radius: 4px;
    font-weight: 500;
    transition: all 0.2s;
}

/* Active page styling */
.pagination .page-item.active .page-link {
    background-color: #0d6efd;
    border-color: #0d6efd;
    color: white;
}

/* Hover state for pagination links */
.pagination .page-link:hover:not(.active) {
    background-color: #e9ecef;
    border-color: #adb5bd;
}
```

## JavaScript Functions

### Basic Page Loading Function

```javascript
window.loadPage = function(pageNumber) {
    console.log('loadPage called with page:', pageNumber);
    
    // Get current search parameters
    const params = new URLSearchParams(window.location.search);
    
    // Update page parameter
    params.set('Page', pageNumber);
    
    console.log('Requesting URL:', `/Search?${params.toString()}`);
    
    // Show loading
    const restaurantList = document.getElementById('restaurant-list');
    if (restaurantList) {
        restaurantList.innerHTML = '<div class="text-center py-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>';
    }
    
    // Send AJAX request
    fetch(`/Search?${params.toString()}`)
        .then(response => {
            console.log('Response status:', response.status);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.text();
        })
        .then(html => {
            console.log('Received HTML length:', html.length);
            
            // Parse the response to get both restaurant list and pagination
            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');
            
            // Update restaurant list content
            const newRestaurantContent = doc.querySelector('#restaurant-list');
            if (newRestaurantContent && restaurantList) {
                restaurantList.innerHTML = newRestaurantContent.innerHTML;
            }
            
            // Update pagination
            const currentPaginationContainer = document.querySelector('.pagination-container');
            const newPaginationContainer = doc.querySelector('.pagination-container');
            
            // If we can find pagination containers
            if (currentPaginationContainer && newPaginationContainer) {
                // Update pagination HTML
                currentPaginationContainer.innerHTML = newPaginationContainer.innerHTML;
                
                // Force all pagination elements to be visible with inline styles
                currentPaginationContainer.style.display = 'flex';
                currentPaginationContainer.style.visibility = 'visible';
                currentPaginationContainer.style.opacity = '1';
                
                // Make sure the pagination nav is visible
                const paginationNav = currentPaginationContainer.querySelector('nav[aria-label="Pagination"]');
                if (paginationNav) {
                    paginationNav.style.display = 'block';
                    paginationNav.style.visibility = 'visible';
                    paginationNav.style.opacity = '1';
                }
                
                // Make sure the pagination list is visible
                const paginationList = currentPaginationContainer.querySelector('.pagination');
                if (paginationList) {
                    paginationList.style.display = 'flex';
                    paginationList.style.visibility = 'visible';
                    paginationList.style.opacity = '1';
                    
                    const pageItems = paginationList.querySelectorAll('.page-item');
                    pageItems.forEach(item => {
                        item.style.display = 'inline-block';
                        item.style.visibility = 'visible';
                        item.style.opacity = '1';
                        
                        const pageLink = item.querySelector('.page-link');
                        if (pageLink) {
                            pageLink.style.display = 'flex';
                            pageLink.style.visibility = 'visible';
                            pageLink.style.opacity = '1';
                        }
                    });
                }
            }
        });
};
```

### Fix Pagination After AJAX Function

```javascript
// Special fix for AJAX pagination updates
function fixPaginationAfterAjax(pageNumber) {
    console.log('Running special pagination fix...');
    setTimeout(() => {
        const paginationContainer = document.querySelector('.pagination-container');
        if (paginationContainer) {
            console.log('Found pagination container, applying fixes');
            
            // Make sure container is visible
            paginationContainer.style.display = 'block';
            paginationContainer.style.visibility = 'visible';
            paginationContainer.style.opacity = '1';
            paginationContainer.style.backgroundColor = '#f8f9fa';
            paginationContainer.style.border = '1px solid #dee2e6';
            paginationContainer.style.padding = '10px';
            paginationContainer.style.margin = '30px 0';
            paginationContainer.style.width = '100%';
            paginationContainer.style.textAlign = 'center';
            
            // Get pagination links as direct children of the container
            const links = paginationContainer.querySelectorAll('a');
            console.log(`Found ${links.length} pagination links`);
            
            // Style each link directly
            links.forEach(link => {
                link.style.display = 'inline-block';
                link.style.padding = '10px 15px';
                link.style.margin = '0 5px';
                link.style.backgroundColor = '#007bff';
                link.style.color = '#fff';
                link.style.textDecoration = 'none';
                link.style.borderRadius = '4px';
                
                // Try to determine if this is the current page
                const text = link.textContent.trim();
                const number = parseInt(text);
                if (number === pageNumber) {
                    link.style.backgroundColor = '#0056b3';
                    link.style.fontWeight = 'bold';
                }
            });
        }
    }, 200); // Delay to ensure DOM is updated
}
```
review, nhà hàng sỡ hữu
### Enhanced LoadPage Function

```javascript
// Override the loadPage function with our enhanced version
const originalLoadPage = window.loadPage;
window.loadPage = function(pageNumber) {
    console.log('Enhanced loadPage called with page:', pageNumber);
    originalLoadPage(pageNumber);
    fixPaginationAfterAjax(pageNumber);
};
```

## Bootstrap Pagination Variables

Bootstrap provides these CSS variables for pagination styling:

```css
.pagination {
  --bs-pagination-padding-x: 0.75rem;
  --bs-pagination-padding-y: 0.375rem;
  --bs-pagination-font-size: 1rem;
  --bs-pagination-color: var(--bs-link-color);
  --bs-pagination-bg: var(--bs-body-bg);
  --bs-pagination-border-width: var(--bs-border-width);
  --bs-pagination-border-color: var(--bs-border-color);
  --bs-pagination-border-radius: var(--bs-border-radius);
  --bs-pagination-hover-color: var(--bs-link-hover-color);
  --bs-pagination-hover-bg: var(--bs-tertiary-bg);
  --bs-pagination-hover-border-color: var(--bs-border-color);
  --bs-pagination-focus-color: var(--bs-link-hover-color);
  --bs-pagination-focus-bg: var(--bs-secondary-bg);
  --bs-pagination-focus-box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25);
  --bs-pagination-active-color: #fff;
  --bs-pagination-active-bg: #0d6efd;
  --bs-pagination-active-border-color: #0d6efd;
  --bs-pagination-disabled-color: var(--bs-secondary-color);
  --bs-pagination-disabled-bg: var(--bs-secondary-bg);
  --bs-pagination-disabled-border-color: var(--bs-border-color);
  display: flex;
  padding-right: 0;
  list-style: none;
}
```