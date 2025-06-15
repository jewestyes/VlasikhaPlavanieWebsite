document.addEventListener('DOMContentLoaded', function () {
	const checkboxes = document.querySelectorAll('.agreement-checkbox');
	const warning = document.getElementById('agreement-warning');
	const form = document.getElementById('payform-tinkoff');

	form.addEventListener('submit', function (e) {
		const allChecked = Array.from(checkboxes).every(cb => cb.checked);
		if (!allChecked) {
			e.preventDefault();
			warning.style.display = 'block';
		}
	});
});