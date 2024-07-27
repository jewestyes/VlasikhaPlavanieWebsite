document.addEventListener('DOMContentLoaded', function () {
    const paymentForm = document.getElementById('paymentForm');
    paymentForm.addEventListener('submit', function (event) {
        const termsCheck = document.getElementById('termsCheck');
        const dataCheck = document.getElementById('dataCheck');

        if (!termsCheck.checked || !dataCheck.checked) {
            event.preventDefault();
            alert('Пожалуйста, примите условия пользовательского соглашения и согласие на обработку персональных данных.');
        }
    });
});
