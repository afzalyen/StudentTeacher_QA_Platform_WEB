$(document).ready(function () {
    // Example: Handle delete confirmation
    $("form").on("submit", function () {
        return confirm("Are you sure you want to delete this question?");
    });
});