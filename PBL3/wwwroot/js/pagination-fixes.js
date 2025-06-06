// Additional JavaScript to ensure pagination works correctly

document.addEventListener('DOMContentLoaded', function() {
    // Ensure pagination container is visible
    const ensurePaginationVisible = function() {
        const paginationContainer = document.querySelector('.pagination-container');
        if (paginationContainer) {
            paginationContainer.style.display = 'flex';
            paginationContainer.style.visibility = 'visible';
        }
    };
    
    // Call initially
    ensurePaginationVisible();
    
    // Override loadPage function to handle active state correctly
    const originalLoadPage = window.loadPage;
    if (originalLoadPage) {
        window.loadPage = function(pageNumber) {
            // Call the original function
            originalLoadPage(pageNumber);
            
            // Manually update active state immediately
            document.querySelectorAll('.pagination .page-item').forEach(item => {
                const dataPage = parseInt(item.getAttribute('data-page'));
                if (dataPage === pageNumber) {
                    item.classList.add('active');
                } else {
                    item.classList.remove('active');
                }
            });
        };
    }
    
    // Monitor DOM changes to ensure pagination is always visible
    const observer = new MutationObserver(function(mutations) {
        ensurePaginationVisible();
    });
    
    const targetNode = document.getElementById('restaurants-column');
    if (targetNode) {
        observer.observe(targetNode, { childList: true, subtree: true });
    }
});
