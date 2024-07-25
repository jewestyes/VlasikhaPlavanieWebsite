document.addEventListener('DOMContentLoaded', function () {
    const disciplineOptions = {
        "на спине": ["25м", "50м"],
        "вольный стиль": ["25м", "50м"],
        "классические ласты": ["50м", "100м"],
        "комплексное плавание": ["100м"],
        "брасс": ["50м"],
        "баттерфляй": ["50м"]
    };

    // Функция для обновления списка дистанций
    function updateDistanceOptions(selectElement) {
        const selectedDiscipline = selectElement.value;
        const distanceSelect = selectElement.closest('.discipline-section').querySelector('.distance-select');
        const currentDistance = distanceSelect.getAttribute('data-current-distance');

        // Очистка текущих опций
        distanceSelect.innerHTML = '';

        // Добавление новых опций
        if (disciplineOptions[selectedDiscipline]) {
            disciplineOptions[selectedDiscipline].forEach(function (distance) {
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

    // Обработчик изменений для всех существующих селектов дисциплин
    document.querySelectorAll('.discipline-select').forEach(function (selectElement) {
        updateDistanceOptions(selectElement);
        selectElement.addEventListener('change', function () {
            updateDistanceOptions(this);
        });
    });

    // Обработчик изменений для динамически добавляемых селектов
    document.body.addEventListener('change', function (event) {
        if (event.target.matches('.discipline-select')) {
            updateDistanceOptions(event.target);
        }
    });

    // Функция для скрытия/показа кнопок удаления
    function toggleRemoveButtons() {
        const participantsCount = document.querySelectorAll('.participant-section').length;
        document.querySelectorAll('.participant-section').forEach(function (participantSection, participantIndex) {
            const disciplinesCount = participantSection.querySelectorAll('.discipline-section').length;
            participantSection.querySelectorAll('.remove-discipline-button').forEach(function (button, disciplineIndex) {
                button.style.display = disciplineIndex > 0 || participantIndex == 0 ? 'inline-block' : 'none';
            });
            participantSection.querySelector('.remove-participant-button').style.display = participantIndex > 0 ? 'inline-block' : 'none';
        });
    }

    // Вызов функции для начального состояния
    toggleRemoveButtons();

    // Обработчик для динамически добавляемых участников и дисциплин
    document.body.addEventListener('click', function (event) {
        if (event.target.matches('.add-discipline-button, .add-participant-button, .remove-discipline-button, .remove-participant-button')) {
            setTimeout(function () {
                toggleRemoveButtons();
                // Повторное обновление дистанций для новых дисциплин
                document.querySelectorAll('.discipline-select').forEach(function (selectElement) {
                    updateDistanceOptions(selectElement);
                });
            }, 100); // Задержка для завершения действия
        }
    });
});
