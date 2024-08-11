﻿document.addEventListener('DOMContentLoaded', function () {
    const disciplineOptions = {
        "На спине": ["25м", "50м"],
        "Вольный стиль": ["25м", "50м"],
        "Классические ласты": ["50м", "100м"],
        "Комплексное плавание": ["100м"],
        "Брасс": ["50м"],
        "Баттерфляй": ["50м"]
    };

    // Функция для получения года рождения
    function getBirthYear(participantSection) {
        const birthDateInput = participantSection.querySelector('input[type="date"]');
        if (birthDateInput && birthDateInput.value) {
            return new Date(birthDateInput.value).getFullYear();
        }
        return null;
    }

    // Функция обновления списка дистанций для выбранной дисциплины
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
                if ((distance === "25м" && (selectedDiscipline === "На спине" || selectedDiscipline === "Вольный стиль")) && (birthYear < 2016 || birthYear > 2018)) {
                    return;
                }
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

    // Скрипт для инициализации сегодняшней даты
    var birthDatePickers = document.querySelectorAll('input[type="date"][id^="birthDatePicker_"]');
    var startDatePickers = document.querySelectorAll('input[type="date"][id^="startDatePicker_"]');

    birthDatePickers.forEach(function (datePicker) {
        if (!datePicker.value || datePicker.value === "0001-01-01") {
            datePicker.valueAsDate = new Date();
        }
    });

    startDatePickers.forEach(function (datePicker) {
        if (!datePicker.value || datePicker.value === "0001-01-01") {
            datePicker.valueAsDate = new Date();
        }
    });

    // Функция инициализации селектов дисциплин
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

    // Функция скрытия или показа кнопок удаления участников и дисциплин
    function toggleRemoveButtons() {
        document.querySelectorAll('.participant-section').forEach((participantSection, participantIndex) => {
            const disciplinesCount = participantSection.querySelectorAll('.discipline-section').length;
            participantSection.querySelectorAll('.remove-discipline-button').forEach((button, disciplineIndex) => {
                button.style.display = disciplineIndex > 0 || participantIndex === 0 ? 'inline-block' : 'none';
            });
            participantSection.querySelector('.remove-participant-button').style.display = participantIndex > 0 ? 'inline-block' : 'none';
        });
    }

    // Инициализация при загрузке документа
    initializeDisciplineSelects();
    toggleRemoveButtons();

    // Обработчики для динамически добавляемых селектов дисциплин и элементов
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
