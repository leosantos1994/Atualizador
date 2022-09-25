// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
completed = function (xhr) {
    console.log(xhr);
    $("#edit-version-container").html(xhr.responseText)
}
