document.addEventListener('DOMContentLoaded', function () {
    const disciplineOptions = {
        "На спине": ["50м"],
        "Вольный стиль": ["50м", "100м", "200м"],
        "Комплексное плавание": ["100м"],
        "Брасс": ["50м", "100м"],
        "Баттерфляй": ["50м"]
    };

    function getBirthYear(participantSection) {
        const birthDateInput = participantSection.querySelector('input[type="date"]');
        if (birthDateInput && birthDateInput.value) {
            return new Date(birthDateInput.value).getFullYear();
        }
        return null;
    }

    function updateDistanceOptions(selectElement) {
        const selectedDiscipline = selectElement.value;
        const disciplineSection = selectElement.closest('.discipline-section');
        const distanceSelect = disciplineSection.querySelector('.distance-select');
        const currentDistance = distanceSelect.getAttribute('data-current-distance');
        const participantSection = disciplineSection.closest('.participant-section');
        const birthYear = getBirthYear(participantSection);

        distanceSelect.innerHTML = '';

        if (disciplineOptions[selectedDiscipline]) {
            disciplineOptions[selectedDiscipline].forEach(distance => {
                const option = document.createElement('option');
                option.value = distance;
                option.text = distance;
                if (distance === currentDistance) {
                    option.selected = true;
                }
                distanceSelect.add(option);
            });
        }
    }

    function getMinimumBirthDate() {
        const today = new Date();
        today.setFullYear(today.getFullYear() - 6);
        return today.toISOString().split('T')[0];
    }

    const minBirthDate = getMinimumBirthDate();

    var birthDatePickers = document.querySelectorAll('input[type="date"]');

    birthDatePickers.forEach(function (datePicker) {
        datePicker.setAttribute('max', minBirthDate);

        if (!datePicker.value || datePicker.value === "0001-01-01") {
            datePicker.value = minBirthDate;
        }

        datePicker.addEventListener('change', function () {
            const today = new Date();
            const birthDate = new Date(this.value);
            const age = today.getFullYear() - birthDate.getFullYear();
            const monthDiff = today.getMonth() - birthDate.getMonth();

            if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
                age--;
            }

            if (age < 6) {
                this.setCustomValidity('Участник должен быть не младше 6 лет');
                this.reportValidity();
                this.value = '';
            } else {
                this.setCustomValidity('');
            }
        });
    });

    function isValidEmail(email) {
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email);
    }

    function isValidPhone(phone) {
        const reRu = /^(\+?7|8)?[\s-]?\(?\d{3}\)?[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}$/;
        const reBy = /^\+?375[\s-]?\d{2}[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}$/;

        return reRu.test(phone) || reBy.test(phone);
    }

    function isValidTime(time) {
        const re = /^([0-5][0-9]):([0-5][0-9]):([0-9]{2})$/;
        return re.test(time);
    }

    document.getElementById('registrationForm').addEventListener('submit', function (event) {
        if (event.submitter && event.submitter.id === 'submitBtn') {
            const emailInputs = document.querySelectorAll('input[type="email"]');
            const phoneInputs = document.querySelectorAll('input[type="tel"]');
            const timeInputs = document.querySelectorAll('input.time-input');

            let formIsValid = true;

            emailInputs.forEach(function (emailInput) {
                const errorSpan = emailInput.nextElementSibling;

                if (!isValidEmail(emailInput.value)) {
                    errorSpan.textContent = 'Поле должно быть действительным электронным адресом.';
                    emailInput.classList.add('is-invalid');
                    formIsValid = false;
                } else {
                    errorSpan.textContent = '';
                    emailInput.classList.remove('is-invalid');
                }
            });

            phoneInputs.forEach(function (phoneInput) {
                const errorSpan = phoneInput.nextElementSibling;

                if (!isValidPhone(phoneInput.value)) {
                    errorSpan.textContent = 'Введите корректный номер телефона (Россия или Беларусь).';
                    phoneInput.classList.add('is-invalid');
                    formIsValid = false;
                } else {
                    errorSpan.textContent = '';
                    phoneInput.classList.remove('is-invalid');
                }
            });

            timeInputs.forEach(function (timeInput) {
                const errorSpan = timeInput.nextElementSibling;

                if (!isValidTime(timeInput.value)) {
                    errorSpan.textContent = 'Введите корректное время в формате MM:SS:SS.';
                    timeInput.classList.add('is-invalid');
                    formIsValid = false;
                } else {
                    errorSpan.textContent = '';
                    timeInput.classList.remove('is-invalid');
                }
            });

            if (!formIsValid) {
                event.preventDefault();
            }
        }
    });


    function initializeDisciplineSelects() {
        document.querySelectorAll('.discipline-select').forEach(selectElement => {
            updateDistanceOptions(selectElement);
            selectElement.addEventListener('change', function () {
                updateDistanceOptions(this);
            });
        });

        document.querySelectorAll('input[type="date"]').forEach(dateInput => {
            dateInput.addEventListener('change', function () {
                const participantSection = this.closest('.participant-section');
                participantSection.querySelectorAll('.discipline-select').forEach(selectElement => {
                    updateDistanceOptions(selectElement);
                });
            });
        });
    }

    function toggleRemoveButtons() {
        document.querySelectorAll('.participant-section').forEach((participantSection, participantIndex) => {
            const disciplinesCount = participantSection.querySelectorAll('.discipline-section').length;
            participantSection.querySelectorAll('.remove-discipline-button').forEach((button, disciplineIndex) => {
                if (button) {
                    button.style.display = disciplineIndex > 0 || participantIndex === 0 ? 'inline-block' : 'none';
                }
            });
            const removeParticipantButton = participantSection.querySelector('.remove-participant-button');
            if (removeParticipantButton) {
                removeParticipantButton.style.display = participantIndex > 0 ? 'inline-block' : 'none';
            }
        });
    }

    initializeDisciplineSelects();
    toggleRemoveButtons();

    document.body.addEventListener('change', function (event) {
        if (event.target.matches('.discipline-select')) {
            updateDistanceOptions(event.target);
        }
    });

    document.body.addEventListener('click', function (event) {
        if (event.target.matches('.add-discipline-button, .add-participant-button, .remove-discipline-button, .remove-participant-button')) {
            setTimeout(() => {
                toggleRemoveButtons();
                initializeDisciplineSelects();
            }, 100);
        }
    });
});
