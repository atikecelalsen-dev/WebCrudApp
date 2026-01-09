// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ajaxSend(function (e, xhr) {
    var token = $('input[name="__RequestVerificationToken"]').val();
    if (token) {
        xhr.setRequestHeader('RequestVerificationToken', token);
    }
});

success: function (res) {
    if (res.success) {
        window.location.href = "/Order/Index";
    }
}