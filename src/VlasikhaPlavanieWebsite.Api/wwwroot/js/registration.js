let defaultStartDate = "";
let minBirthDate = "";

document.addEventListener('DOMContentLoaded', function () {
    const disciplineOptions = window.disciplineOptions;
    defaultStartDate = window.defaultStartDate || "";
    minBirthDate = getMinimumBirthDate();

    // ----------------------------------------------------
    // ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ
    // ----------------------------------------------------

    function getBirthYear(participantSection) {
        const birthDateInput = participantSection.querySelector('input[type="date"]');
        if (birthDateInput && birthDateInput.value) {
            return new Date(birthDateInput.value).getFullYear();
        }
        return null; getBirthYear
    }

    function updateDistanceOptions(selectElement) {
        const selectedDiscipline = selectElement.value;
        const disciplineSection = selectElement.closest('.discipline-section');
        const distanceSelect = disciplineSection.querySelector('.distance-select');

        const oldSelectedDistance = distanceSelect.value;

        distanceSelect.innerHTML = '';

        if (disciplineOptions[selectedDiscipline]) {
            disciplineOptions[selectedDiscipline].forEach(distance => {
                const option = document.createElement('option');
                option.value = distance;
                option.text = distance;

                if (distance === oldSelectedDistance) {
                    option.selected = true;
                }

                distanceSelect.add(option);
            });
        }
    }

    function getMinimumBirthDate() {
        const today = new Date();
        today.setFullYear(today.getFullYear() - 5);
        return today.toISOString().split('T')[0];
    }


    var birthDatePickers = document.querySelectorAll('input[type="date"]');

    birthDatePickers.forEach(function (datePicker) {
        datePicker.setAttribute('max', minBirthDate);

        if (!datePicker.value || datePicker.value === "0001-01-01") {
            datePicker.value = minBirthDate;
        }

        // Проверка возраста
        datePicker.addEventListener('change', function () {
            const today = new Date();
            const birthDate = new Date(this.value);
            const age = today.getFullYear() - birthDate.getFullYear();
            const monthDiff = today.getMonth() - birthDate.getMonth();

            let actualAge = age;
            if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
                actualAge--;
            }

            if (actualAge < 5) {
                this.setCustomValidity('Участник должен быть не младше 5 лет!');
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
        // Россия и Беларусь
        const reRu = /^(\+?7|8)?[\s-]?\(?\d{3}\)?[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}$/;
        const reBy = /^\+?375[\s-]?\d{2}[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}$/;
        return reRu.test(phone) || reBy.test(phone);
    }

    function isValidTime(time) {
        // Формат: MM:SS:SS
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
                if (participantSection) {
                    participantSection.querySelectorAll('.discipline-select').forEach(selectElement => {
                        updateDistanceOptions(selectElement);
                    });
                }
            });
        });
    }

    function toggleRemoveButtons() {
        document.querySelectorAll('.participant-section').forEach((participantSection, participantIndex) => {
            const disciplineSections = participantSection.querySelectorAll('.discipline-section');

            disciplineSections.forEach((section, disciplineIndex) => {
                const removeDisciplineBtn = section.querySelector('.remove-discipline-button');
                if (removeDisciplineBtn) {
                    removeDisciplineBtn.style.display = (disciplineIndex > 0) ? 'inline-block' : 'none';
                }
            });

            const removeParticipantButton = participantSection.querySelector('.remove-participant-button');
            if (removeParticipantButton) {
                removeParticipantButton.style.display = (participantIndex > 0) ? 'inline-block' : 'none';
            }
        });
    }

    function reindexParticipants() {
        const participantSections = document.querySelectorAll('.participant-section');
        participantSections.forEach((section, newIndex) => {
            const h3 = section.querySelector('h3');
            if (h3) {
                h3.textContent = `Участник №${newIndex + 1}`;
            }

            const inputs = section.querySelectorAll('input, select, textarea');
            inputs.forEach(input => {
                if (!input.name) return;
                let newName = input.name.replace(/Participants\[\d+\]/, `Participants[${newIndex}]`);
                input.name = newName;
            });

            reindexDisciplines(section, newIndex);

            const addDiscBtn = section.querySelector('.add-discipline-button');
            if (addDiscBtn) {
                addDiscBtn.setAttribute('onclick', `addDiscipline(${newIndex})`);
            }
            const removePartBtn = section.querySelector('.remove-participant-button');
            if (removePartBtn) {
                removePartBtn.setAttribute('onclick', `removeParticipant(${newIndex})`);
            }
        });
    }

    function reindexDisciplines(participantSection, pIndex) {
        const disciplineSections = participantSection.querySelectorAll('.discipline-section');
        disciplineSections.forEach((discSec, dIndex) => {
            const inputs = discSec.querySelectorAll('input, select, textarea');
            inputs.forEach(input => {
                if (!input.name) return;
                let newName = input.name;

                newName = newName.replace(/Participants\[\d+\]/, `Participants[${pIndex}]`);
                newName = newName.replace(/Disciplines\[\d+\]/, `Disciplines[${dIndex}]`);

                input.name = newName;
            });
            const removeDiscBtn = discSec.querySelector('.remove-discipline-button');
            if (removeDiscBtn) {
                removeDiscBtn.setAttribute('onclick', `removeDiscipline(${pIndex}, ${dIndex})`);
            }
        });
    }

    document.body.addEventListener('click', function (event) {
        if (event.target.matches('.add-discipline-button, .add-participant-button, .remove-discipline-button, .remove-participant-button')) {
            setTimeout(() => {
                reindexParticipants();
                toggleRemoveButtons();
                initializeDisciplineSelects();
            }, 100);
        }
    });

    initializeDisciplineSelects();
    toggleRemoveButtons();

    let participantIndexCounter = document.querySelectorAll('.participant-section').length - 1;

    window.addParticipant = function () {

        console.log('defaultStartDate:', defaultStartDate);
        console.log('minBirthDate:', minBirthDate);

        participantIndexCounter++;

        // Получаем HTML-список дисциплин (option) из disciplineOptions,
        // чтобы <select> дисциплины не был пустым
        const disciplineOptionsHtml = Object.keys(disciplineOptions)
            .map(dk => `<option value="${dk}">${dk}</option>`)
            .join('');

        const newParticipantHtml = `
<div class="participant-section">
    <h3>Участник №${participantIndexCounter + 1}</h3>
    <div class="form-row">
        <div class="form-group col-md-4">
            <label>Фамилия</label>
            <input name="Participants[${participantIndexCounter}].LastName" class="form-control" required />
            <span class="text-danger"></span>
        </div>
        <div class="form-group col-md-4">
            <label>Имя</label>
            <input name="Participants[${participantIndexCounter}].FirstName" class="form-control" required />
            <span class="text-danger"></span>
        </div>
        <div class="form-group col-md-4">
            <label>Отчество</label>
            <input name="Participants[${participantIndexCounter}].MiddleName" class="form-control" required />
            <span class="text-danger"></span>
        </div>
    </div>


    <div class="form-row">
        <div class="form-group col-md-4">
            <label>Дата рождения</label>
            <input type="date"
       name="Participants[${participantIndexCounter}].BirthDate"
       class="form-control birthDatePicker"
       value="${minBirthDate}"
       max="${minBirthDate}"
       required />
            <span class="text-danger"></span>
        </div>
        <div class="form-group col-md-4">
            <label>Пол</label>
            <select name="Participants[${participantIndexCounter}].Gender" class="form-control" required>
                <option value="Мужской">Мужской</option>
                <option value="Женский">Женский</option>
            </select>
            <span class="text-danger"></span>
        </div>
        <div class="form-group col-md-4">
            <label>Телефон</label>
            <input type="tel"
                   name="Participants[${participantIndexCounter}].Phone"
                   class="form-control" required />
            <span class="text-danger"></span>
        </div>
    </div>

    <div class="form-row">
        <div class="form-group col-md-6">
            <label>Email</label>
            <input type="email"
                   name="Participants[${participantIndexCounter}].Email"
                   class="form-control" required />
            <span class="text-danger"></span>
        </div>
        <div class="form-group col-md-6">
            <label>Город/команда</label>
            <input name="Participants[${participantIndexCounter}].CityOrTeam" class="form-control" required />
            <span class="text-danger"></span>
        </div>
    </div>

    <div class="form-row">
        <div class="form-group col-md-6">
            <label>Разряд</label>
            <select name="Participants[${participantIndexCounter}].Rank" class="form-control">
                <option value="Б/Р">Б/Р</option>
                <option value="IIIюн">IIIюн</option>
                <option value="IIюн">IIюн</option>
                <option value="Iюн">Iюн</option>
                <option value="III">III</option>
                <option value="II">II</option>
                <option value="I">I</option>
                <option value="КМС">КМС</option>
                <option value="МС">МС</option>
                <option value="МСМК">МСМК</option>
                <option value="ЗМС">ЗМС</option>
            </select>
        </div>
    </div>

    <h4>Выберите дисциплину</h4>
    <div class="discipline-section">
        <div class="form-row">
            <div class="form-group col-md-4">
                <label>Дисциплина</label>
                <select name="Participants[${participantIndexCounter}].Disciplines[0].Name"
                        class="form-control discipline-select" required>
                    ${disciplineOptionsHtml}
                </select>
                <span class="text-danger"></span>
            </div>
            <div class="form-group col-md-4">
                <label>Выберите дистанцию</label>
                <select name="Participants[${participantIndexCounter}].Disciplines[0].Distance"
                        class="form-control distance-select"
                        data-current-distance=""
                        required>
                </select>
                <span class="text-danger"></span>
            </div>
            <div class="form-group col-md-4">
                <label>Дата старта</label>
                <input type="date"
                       name="Participants[${participantIndexCounter}].Disciplines[0].StartDate"
                       class="form-control"
                       value="${defaultStartDate}"
                       readonly />
                <span class="text-danger"></span>
            </div>
            <div class="form-group col-md-4">
                <label>Заявочное время</label>
                <input type="text"
                       name="Participants[${participantIndexCounter}].Disciplines[0].EntryTime"
                       class="form-control time-input"
                       placeholder="MM:SS:SS" maxlength="9"
                       required />
                <span class="text-danger"></span>
            </div>
        </div>
        <button type="button"
                class="btn btn-danger remove-discipline-button"
                onclick="removeDiscipline(${participantIndexCounter}, 0)">
            Удалить дисциплину
        </button>
    </div>

    <button type="button"
            class="btn add-discipline-button"
            onclick="addDiscipline(${participantIndexCounter})">
        Добавить дисциплину
    </button>
    <button type="button"
            class="btn btn-danger remove-participant-button"
            onclick="removeParticipant(${participantIndexCounter})">
        Удалить участника
    </button>
</div>
        `;

        const container = document.getElementById('participantsContainer');
        container.insertAdjacentHTML('beforeend', newParticipantHtml);

        setTimeout(() => {
            reindexParticipants();
            toggleRemoveButtons();
            initializeDisciplineSelects();
        }, 100);
    };

    window.addDiscipline = function (participantIndex) {
        const participantSections = document.querySelectorAll('.participant-section');
        const participantSection = participantSections[participantIndex];
        if (!participantSection) return;

        const disciplineSections = participantSection.querySelectorAll('.discipline-section');
        const dIndex = disciplineSections.length;

        // Получаем html-список дисциплин
        const disciplineOptionsHtml = Object.keys(disciplineOptions)
            .map(dk => `<option value="${dk}">${dk}</option>`)
            .join('');

        const lastDiscipline = disciplineSections[disciplineSections.length - 1];
        const lastStartDateValue = lastDiscipline
            .querySelector('input[name*=".StartDate"]')
            ?.value || '';

        const newDisciplineHtml = `
<div class="discipline-section">
    <div class="form-row">
        <div class="form-group col-md-4">
            <label>Дисциплина</label>
            <select name="Participants[${participantIndex}].Disciplines[${dIndex}].Name"
                    class="form-control discipline-select" required>
                ${disciplineOptionsHtml}
            </select>
            <span class="text-danger"></span>
        </div>
        <div class="form-group col-md-4">
            <label>Выберите дистанцию</label>
            <select name="Participants[${participantIndex}].Disciplines[${dIndex}].Distance"
                    class="form-control distance-select"
                    data-current-distance=""
                    required>
            </select>
            <span class="text-danger"></span>
        </div>
        <div class="form-group col-md-4">
            <label>Дата старта</label>
            <input type="date"
                   name="Participants[${participantIndex}].Disciplines[${dIndex}].StartDate"
                   class="form-control"
                   value="${lastStartDateValue}"
                   readonly />
            <span class="text-danger"></span>
        </div>
        <div class="form-group col-md-4">
            <label>Заявочное время</label>
            <input type="text"
                   name="Participants[${participantIndex}].Disciplines[${dIndex}].EntryTime"
                   class="form-control time-input"
                   placeholder="MM:SS:SS"
                   maxlength="9"
                   required />
            <span class="text-danger"></span>
        </div>
    </div>
    <button type="button"
            class="btn btn-danger remove-discipline-button"
            onclick="removeDiscipline(${participantIndex}, ${dIndex})">
        Удалить дисциплину
    </button>
</div>
        `;

        const lastDisciplineSection = disciplineSections[disciplineSections.length - 1];
        lastDisciplineSection.insertAdjacentHTML('afterend', newDisciplineHtml);

        setTimeout(() => {
            reindexParticipants();
            toggleRemoveButtons();
            initializeDisciplineSelects();
        }, 100);
    };

    window.removeDiscipline = function (participantIndex, disciplineIndex) {
        const participantSections = document.querySelectorAll('.participant-section');
        const participantSection = participantSections[participantIndex];
        if (!participantSection) return;

        const disciplineSections = participantSection.querySelectorAll('.discipline-section');
        if (disciplineIndex >= 0 && disciplineIndex < disciplineSections.length) {
            disciplineSections[disciplineIndex].remove();
        }

        setTimeout(() => {
            reindexParticipants();
            toggleRemoveButtons();
            initializeDisciplineSelects();
        }, 100);
    };

    window.removeParticipant = function (participantIndex) {
        const participantSections = document.querySelectorAll('.participant-section');
        if (participantIndex >= 0 && participantIndex < participantSections.length) {
            participantSections[participantIndex].remove();
        }

        setTimeout(() => {
            reindexParticipants();
            toggleRemoveButtons();
            initializeDisciplineSelects();
        }, 100);
    };
});
