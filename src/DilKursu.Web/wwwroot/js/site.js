// =============================================================================
// Bir Lisan Bir İnsan - Ortak istemci tarafı yardımcıları
// AJAX (jQuery), SweetAlert2 bildirimleri ve jQuery DataTables yapılandırmasını
// tek noktada toplar. Böylece tüm sayfalar tutarlı ve tekrar etmeyen (DRY) kod kullanır.
// =============================================================================

/**
 * Sayfadaki gizli antiforgery (CSRF) token değerini okur.
 * AJAX POST isteklerinde "RequestVerificationToken" başlığıyla gönderilir.
 * @returns {string} Token değeri.
 */
function getAntiForgeryToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]').value;
}

/**
 * SweetAlert2 ile sağ üstte kısa süreli bir bildirim (toast) gösterir.
 * @param {boolean} success İşlem başarılı mı?
 * @param {string} message Gösterilecek mesaj.
 */
function showToast(success, message) {
    Swal.fire({
        toast: true,
        position: 'top-end',
        icon: success ? 'success' : 'error',
        title: message,
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true
    });
}

/**
 * Silme gibi geri alınamaz işlemler öncesi SweetAlert2 onay kutusu gösterir.
 * @param {string} text Onay metni.
 * @returns {Promise<boolean>} Kullanıcı onayladıysa true.
 */
async function confirmAction(text) {
    const result = await Swal.fire({
        title: 'Emin misiniz?',
        text: text,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Evet',
        cancelButtonText: 'Vazgeç'
    });
    return result.isConfirmed;
}

/**
 * Form-encoded (application/x-www-form-urlencoded) bir AJAX POST isteği gönderir.
 * @param {string} url Hedef adres.
 * @param {object} data Gönderilecek veri (anahtar/değer).
 * @returns {Promise<object>} Sunucudan dönen { success, message } nesnesi.
 */
function postForm(url, data) {
    return $.ajax({
        url: url,
        type: 'POST',
        data: data,
        headers: { 'RequestVerificationToken': getAntiForgeryToken() }
    });
}

/**
 * JSON gövdeli bir AJAX POST isteği gönderir (karmaşık/iç içe nesneler için).
 * @param {string} url Hedef adres.
 * @param {object} data Gönderilecek nesne (JSON'a çevrilir).
 * @returns {Promise<object>} Sunucudan dönen yanıt.
 */
function postJson(url, data) {
    return $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        headers: { 'RequestVerificationToken': getAntiForgeryToken() }
    });
}

/**
 * Basit bir GET isteği gönderir.
 * @param {string} url Hedef adres.
 * @returns {Promise<object>} Sunucu yanıtı.
 */
function getJson(url) {
    return $.ajax({ url: url, type: 'GET' });
}

/**
 * jQuery DataTables için ortak Türkçe dil yapılandırmasını döndürür.
 * @returns {object} DataTables language nesnesi.
 */
function dataTableLanguage() {
    return {
        "emptyTable": "Tabloda veri bulunmuyor",
        "info": "_TOTAL_ kayıttan _START_ - _END_ arası gösteriliyor",
        "infoEmpty": "Kayıt yok",
        "infoFiltered": "(_MAX_ kayıt içinden filtrelendi)",
        "lengthMenu": "_MENU_ kayıt göster",
        "loadingRecords": "Yükleniyor...",
        "processing": "İşleniyor...",
        "search": "Ara:",
        "zeroRecords": "Eşleşen kayıt bulunamadı",
        "paginate": {
            "first": "İlk",
            "last": "Son",
            "next": "Sonraki",
            "previous": "Önceki"
        }
    };
}

/**
 * Verilen tablo seçicisi için ortak ayarlarla bir DataTable örneği oluşturur.
 * @param {string} selector Tablo seçicisi (ör. "#branchTable").
 * @param {Array} columns DataTables kolon tanımları.
 * @param {string} ajaxUrl Verilerin çekileceği adres.
 * @returns {object} Oluşturulan DataTable örneği.
 */
function buildDataTable(selector, columns, ajaxUrl) {
    return $(selector).DataTable({
        ajax: { url: ajaxUrl, dataSrc: '' },
        columns: columns,
        language: dataTableLanguage(),
        order: [[0, 'asc']],
        responsive: true,
        autoWidth: false
    });
}

// Sayfa yüklendiğinde, sunucudan gelen geçici mesaj varsa göster.
$(function () {
    const serverToast = document.getElementById('serverToast');
    if (serverToast) {
        showToast(serverToast.dataset.icon === 'success', serverToast.dataset.message);
    }
});
