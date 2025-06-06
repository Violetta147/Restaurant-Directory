# Yelp Search Page Structure Analysis
**Page URL:** https://www.yelp.com/search?find_desc=cake&find_loc=San+Francisco%2C+CA  
**Page Title:** TOP 10 BEST Cake in San Francisco, CA - Updated 2025 - Yelp  
**Extracted on:** January 10, 2025

## Page Overview
This document contains the complete DOM structure and accessibility tree of the Yelp search results page for "cake" in San Francisco, CA. The structure is organized in a hierarchical format showing all main components and their relationships.

## Main Page Structure

### 1. Header/Navigation Section
- **Main Header Banner** (`banner [ref=e14]`)
  - Logo and branding section
  - **Search Interface** (`search [ref=e24]`)
    - "Find" search box (currently contains: "cake")
    - "Near" location box (currently contains: "San Francisco, CA")
    - Search button
  - **User Actions Navigation** (`navigation "User actions" [ref=e51]`)
    - "Yelp for Business" button
    - "Write a Review" link
    - "Start a Project" button
    - Login/Sign Up buttons

### 2. Category Navigation
- **Business Categories Navigation** (`navigation "Business categories" [ref=e84]`)
  - Restaurants (with sub-categories)
  - Home & Garden (with sub-categories)
  - Auto Services (with sub-categories)
  - Health & Beauty (with sub-categories)
  - Travel & Activities (with sub-categories)
  - More (expandable)

### 3. Main Content Layout

#### Sidebar Filters (`complementary "Filters" [ref=e150]`)
- **Price Filter** (`group "Price" [ref=e154]`)
  - Price range buttons: $, $$, $$$, $$$$
- **Suggested Filters** (`group "Suggested" [ref=e168]`)
  - Open Now checkbox (2:50 PM)
  - Reservations checkbox
  - Offers Online Waitlist checkbox
  - Offers Delivery checkbox
  - Offers Takeout checkbox
  - Free Wi-Fi checkbox
- **Category Filters** (`group "Category" [ref=e230]`)
  - Custom Cakes
  - Patisserie/Cake Shop
  - Cupcakes
  - Bakeries
  - Desserts
  - Macarons
  - "See all" option
- **Features Filter** (`group "Features" [ref=e256]`)
  - Outdoor Seating
  - Dogs Allowed
  - Open At (time selector)
  - Good for Kids
  - Good for Dessert
  - Good for Groups
  - "See all" option
- **Distance Filter** (`radiogroup "Distance" [ref=e322]`)
  - Bird's-eye View (selected)
  - Driving (5 mi.)
  - Biking (2 mi.)
  - Walking (1 mi.)
  - Within 4 blocks

#### Main Results Section (`main [ref=e371]`)
- **Breadcrumb Navigation**
  - Restaurants > Cake
- **Results Header**
  - "Top 10 Best cake Near San Francisco, California" heading
  - Sort options (currently "Recommended")

#### Search Results List (`list [ref=e408]`)

**Sponsored Results Section:**
1. **Nothing Bundt Cakes** (Sponsored - 4.3 stars, 185 reviews)
   - Location: Daly City
   - Categories: Cupcakes, Desserts, Bakeries
   - Features: Website link, price range $$

2. **Butter&** (Sponsored - 4.8 stars, 428 reviews)
   - Location: Potrero Hill
   - Categories: Custom Cakes, Patisserie/Cake Shop, Bakeries
   - Features: Locally owned & operated, Kid friendly

**Organic Search Results:**
1. **Yasukochi's Sweet Stop** (4.5 stars, 883 reviews)
   - Location: Japantown, $$
   - Categories: Bakeries, Desserts

2. **Schubert's Bakery** (4.5 stars, 2.1k reviews)
   - Location: Inner Richmond, $$
   - Categories: Bakeries, Desserts

3. **B Patisserie** (4.6 stars, 3.7k reviews)
   - Location: Lower Pacific Heights, $$
   - Categories: Bakeries, Patisserie/Cake Shop, Macarons

4. **Butter & Crumble** (4.6 stars, 464 reviews)
   - Location: North Beach/Telegraph Hill, $$
   - Categories: Bakeries, Patisserie/Cake Shop, Desserts

5. **Sweet Glory** (4.8 stars, 168 reviews)
   - Location: Inner Sunset
   - Categories: Desserts, Patisserie/Cake Shop, Coffee & Tea
   - Features: Order button available

6. **Butter&** (4.8 stars, 428 reviews)
   - Location: Potrero Hill, $$
   - Categories: Patisserie/Cake Shop, Bakeries, Custom Cakes
   - Features: Locally owned & operated, Kid friendly

7. **DayDream Cake Shop** (4.3 stars, 298 reviews)
   - Location: Outer Sunset, $$$
   - Categories: Desserts, Patisserie/Cake Shop, Coffee & Tea
   - Features: Order button available

8. **Ambrosia Bakery** (4.2 stars, 805 reviews)
   - Location: Lakeside, $$
   - Categories: Bakeries, Sandwiches, Patisserie/Cake Shop
   - Features: Order button available

9. **Tartine Bakery** (4.2 stars, 9.1k reviews)
   - Location: Mission, $$
   - Categories: Bakeries, Cafes, Desserts

10. **Sunset Bakery** (4.3 stars, 561 reviews)
    - Location: Inner Sunset, $
    - Categories: Bakeries

**Additional Sponsored Result:**
- **Zibatreats Cakes** (4.9 stars, 266 reviews)
  - Location: Bayview-Hunters Point
  - Categories: Custom Cakes, Bakeries, Patisserie/Cake Shop
  - Features: Kid friendly, Gluten-free friendly

### 4. Pagination Section
- **Pagination Navigation** (`navigation "Pagination navigation" [ref=e1996]`)
  - Current page: 1
  - Available pages: 2, 3, 4, 5, 6, 7, 8, 9
  - Next page button

### 5. Additional Content Sections

#### Business Addition Prompt
- "Can't find the business?" section
- "Add business" link

#### Search Improvement
- "Got search feedback?" with help link

#### Related Searches
Extensive list of related cake searches including:
- Custom Cakes, Wedding Cake, Cake Pops
- Red Velvet Cake, Tres Leches Cake, Chocolate Cake
- Cupcake, Carrot Cake, Ice Cream Cake
- And many more specialized cake types and delivery options

#### Trending Searches
Popular searches in San Francisco including:
- Barbeque, Beer Gardens, Black Forest Cake
- Various dessert and food categories

#### Location-Based Searches
- **Nearby Cities:** Alameda, Berkeley, Burlingame, etc.
- **Neighborhoods:** Castro, Civic Center, Cole Valley, etc.
- **Streets:** Fillmore Street, Hayes Street, Lombard Street, etc.
- **Campuses:** Academy of Art University, SFSU, USF, etc.

#### Community Features
- **Related Talk Topics**
- **FAQ Section** with user-generated content

### 6. Footer Section (`contentinfo [ref=e2609]`)
Comprehensive footer with multiple sections:
- **About:** About Yelp, Careers, Press, etc.
- **Discover:** Collections, Talk, Events, Blog, etc.
- **Yelp for Business:** Business tools and resources
- **Language/Location Selectors**
- **Copyright Information**

## Technical Structure Notes

### Key Reference IDs and Accessibility Features
- Each interactive element has unique reference IDs (e.g., `[ref=e14]`)
- Proper ARIA labels and roles implemented
- Semantic HTML structure with proper heading hierarchy
- Navigation landmarks clearly defined
- Form controls properly labeled

### Interactive Elements
- Search forms with autocomplete functionality
- Filter checkboxes and radio buttons
- Clickable business listings with multiple action buttons
- Pagination controls
- Dropdown menus for categories and sorting

### Image Handling
- Business photos for each listing
- Star rating images with alt text
- Icon images for features and navigation
- Loading states indicated

### Data Structure Patterns
Each business listing follows consistent structure:
- Business name (heading level 3)
- Star rating (image + text)
- Review count
- Location and price range
- Category tags
- Description snippet with "more" link
- Action buttons (Order, View Website, etc.)

This structure represents a comprehensive e-commerce/directory page optimized for local business discovery with robust filtering, search, and navigation capabilities.
