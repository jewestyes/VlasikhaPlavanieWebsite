document.addEventListener('DOMContentLoaded', function () {
    const disciplineOptions = {
        "На спине": ["25м", "50м"],
        "Вольный стиль": ["25м", "50м"],
        "Классические ласты": ["50м", "100м"],
        "Комплексное плавание": ["100м"],
        "Брасс": ["50м"],
        "Баттерфляй": ["50м"]
    };

    /**
     * Получает год рождения участника.
     * @param {HTMLElement} participantSection - секция участника.
     * @returns {number} - год рождения.
     */
    function getBirthYear(participantSection) {
        const birthDateInput = participantSection.querySelector('input[type="date"]');
        if (birthDateInput && birthDateInput.value) {
            return new Date(birthDateInput.value).getFullYear();
        }
        return null;
    }

    /**
     * Обновляет список дистанций для выбранной дисциплины.
     * @param {HTMLElement} selectElement - селект дисциплины.
     */
    function updateDistanceOptions(selectElement) {
        const selectedDiscipline = selectElement.value;
        const disciplineSection = selectElement.closest('.discipline-section');
        const distanceSelect = disciplineSection.querySelector('.distance-select');
        const currentDistance = distanceSelect.getAttribute('data-current-distance');
        const participantSection = disciplineSection.closest('.participant-section');
        const birthYear = getBirthYear(participantSection);

        // Очистка текущих опций
        distanceSelect.innerHTML = '';

        // Добавление новых опций с учетом года рождения
        if (disciplineOptions[selectedDiscipline]) {
            disciplineOptions[selectedDiscipline].forEach(distance => {
                if ((distance === "25м" && (selectedDiscipline === "На спине" || selectedDiscipline === "Вольный стиль")) && (birthYear < 2016 || birthYear > 2018)) {
                    return; // Пропустить опцию, если год рождения не в диапазоне 2016-2018
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

    /**
     * Инициализирует обновление опций для всех существующих селектов дисциплин.
     */
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

    /**
     * Скрывает или показывает кнопки удаления участников и дисциплин.
     */
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

    // Обработчик для динамически добавляемых селектов дисциплин
    document.body.addEventListener('change', function (event) {
        if (event.target.matches('.discipline-select')) {
            updateDistanceOptions(event.target);
        }
    });

    // Обработчик для динамически добавляемых или удаляемых элементов
    document.body.addEventListener('click', function (event) {
        if (event.target.matches('.add-discipline-button, .add-participant-button, .remove-discipline-button, .remove-participant-button')) {
            setTimeout(() => {
                toggleRemoveButtons();
                initializeDisciplineSelects();
            }, 100); // Задержка для завершения действия
        }
    });
});
