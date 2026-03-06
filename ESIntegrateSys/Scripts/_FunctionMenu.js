function initDrawerMenu() {
    // Drawer menu toggle
    $('.drawer-toggle').on('click', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $('.drawer-menu').toggleClass('open');
    });

    // Close drawer menu when clicking outside
    $(document).on('click', function (event) {
        if (!$(event.target).closest('.drawer-menu, .drawer-toggle').length) {
            $('.drawer-menu').removeClass('open');
        }
    });

    // Handle drawer menu item clicks
    $('.drawer-menu-item').on('click', function (e) {
        e.preventDefault();
        var targetUrl = $(this).attr('href');
        var targetContentId = '';

        // Determine target content area based on the clicked menu item
        if (targetUrl.includes('生產單位')) {
            targetContentId = '#生產單位Content';
        } else if (targetUrl.includes('報工生產單位')) {
            targetContentId = '#unitContent';
        } else if (targetUrl.includes('權限設定')) {
            targetContentId = '#permissionContent';
        }

        // Hide all content areas
        $('#生產單位Content, #unitContent, #permissionContent').hide();

        // Show the target content area
        if (targetContentId === '#unitContent') {
            // For #unitContent, we don't load partial content, just display the existing table structure
            $(targetContentId).show();
        } else if (targetContentId) {
            // Load partial content for other areas
            $(targetContentId).load(targetUrl, function () {
                $(targetContentId).show();
            });
        }

        // Close the drawer menu
        $('.drawer-menu').removeClass('open');
    });
}