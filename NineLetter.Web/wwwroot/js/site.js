var onBegin = function (data, status, xhr) {
    $("#validateMessage").empty();
};
var onSuccess = function (data, status, xhr) {
    console.log(data);
    $("#validateMessage").removeClass('error');
    $("#validateMessage").empty();
    if (data.success == 'fail') {
        $("#validateMessage").addClass('error');
        for (var i = 0; JSON.parse(data.errorList).length > i; i += 1) {
            $("#validateMessage").append(JSON.parse(data.errorList)[i].Errors[0].ErrorMessage + "</br>");
        }
    } else {
        console.log(data);
        window.location.href = data;
    }
};
var onFailure = function (status, xhr) {
    console.log('Error: ' + xhr.statusText);
}
