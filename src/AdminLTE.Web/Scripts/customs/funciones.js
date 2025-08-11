function limpiarUrl(solicitudUrl) {
    var resultado = "";
    if (solicitudUrl != null && solicitudUrl != '' && solicitudUrl.length > 0) {
        var firstChar = solicitudUrl.charAt(0);
        if (firstChar == "/") {
            resultado = solicitudUrl.substring(1, solicitudUrl.length);
        }
    }

    return resultado;
}

function RutaURL(url) {
    var resultadoUrl = "";

    var auxiliarUrl = limpiarUrl(url);
    var resultadoUrl = baseURL + auxiliarUrl;
    return resultadoUrl;
}

var estadoSesion;
function verificarSesion() {
    // $.blockUI({});

    estadoSesion = true;
    $.ajax({
        type: "POST", dataType: 'json', async: false,
        url: RutaURL('/Login/verificar'),
        success: function (data) {

            if (data.data == false) {
                estadoSesion = false;
                var url = RutaURL('/Login/Index');
                window.location.href = url;
                //swal({
                //    title: "Su sesión ha caducado",
                //    text: "",
                //    type: "warning",
                //    confirmButtonText: "OK"
                //}).then(function () {
                //    var url = DevolverURL('/Login/Login');
                //    window.location.href = url;
                //}).catch(function () {
                //    var url = DevolverURL('/Login/Login');
                //    window.location.href = url;
                //});
            }
        },
        error: (error) => {
            console.log("error");
        }
        //complete: () => {
        //    $.unblockUI();
        //}
    });
}

function isNumberKey(evt) {
    var charCode = (evt.which) ? evt.which : evt.keyCode;
    if (charCode > 31 && (charCode < 46 || charCode > 57)) {
        return false;
    }
    return true;
}

