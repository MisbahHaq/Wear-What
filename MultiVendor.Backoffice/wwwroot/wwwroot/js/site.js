// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function updateCartBadge() {
    $.get('/Cart/Count', function(data) {
        var badge = $('#cartBadge');
        if (data.count > 0) {
            badge.text(data.count).removeClass('d-none');
        } else {
            badge.addClass('d-none');
        }
    });
}

$(document).ready(function() {
    updateCartBadge();
});
