// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
completed = function (xhr) {
    var loader = $('.loading')
    if (loader != undefined)
        $(loader).hide();

    $("#edit-version-container").html(xhr.responseText)
}

loadSpinner = function (xhr) {
    var loader = $('.loading')
    if (loader != undefined)
        $(loader).show();
}