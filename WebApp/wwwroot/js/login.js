$(document).ready(function () {
    console.log('Sayfa tamamen yüklendi');

    $('button[name="enter-btn"]').on('click', function (event) {
        event.preventDefault(); // Sayfa yenilenmesini engelle

        var userName = $('#username').val();
        var passWord = $('#password').val();

        sendLoginRequest(userName, passWord);

        console.log('Fonksiyon çağırıldı');
    });
});

function sendLoginRequest(userName, passWord) {
    $.ajax({
        url: '/Request/ControlLogin', // TokenController'daki login yoluna istek yapılıyor
        type: 'POST',
        data: { UserName: userName, Password: passWord }, // Kullanıcı adı ve şifreyi JSON formatında gönderiyoruz
        success: function (data) {
            console.log('İstek başarılı');
            if (data.id > 0) {
                window.location.href = "Home/Index"
            }
            else {
                console.log("Kullanıcı adı veya şifre yanlış!");

                Swal.fire({
                    position: 'top-end',
                    icon: 'error',
                    title: 'Oops...',
                    text: 'Kullanıcı adı veya şifre hatalı!',
                    showConfirmButton: false,
                    timer: 1000
                })
            }
        },
        error: function (xhr, status, error) {
            console.error('İstek başarısız: ' + xhr.responseText);
            // Hata mesajına erişmek için xhr.responseText kullanabilirsiniz
        }
    });
}
