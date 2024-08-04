TEST
# VlasikhaPlavanieWebsite

## Описание
VlasikhaPlavanieWebsite - это веб-приложение для регистрации участников на соревнования по плаванию. Проект создан на ASP.NET Core и включает функционал для администраторов и пользователей.

## Технологии
- ASP.NET Core
- Entity Framework Core
- Microsoft SQL Server
- HTML, CSS, JavaScript

## Установка
1. Клонируйте репозиторий:
   ```sh
   git clone https://github.com/jewestyes/VlasikhaPlavanieWebsite.git
   ```
2. Перейдите в директорию проекта:
   ```sh
   cd VlasikhaPlavanieWebsite
   ```
3. Установите необходимые пакеты:
   ```sh
   dotnet restore
   ```
4. Настройте соединение с базой данных в `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=ВашСервер;Database=ВашБД;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```
5. Примените миграции и создайте базу данных:
   ```sh
   dotnet ef database update
   ```
6. Запустите приложение:
   ```sh
   dotnet run
   ```

## Функционал
- Регистрация участников на соревнования.
- Администрирование участников и дисциплин.
- Система ролей и авторизация.

## Структура проекта
- **Controllers**: содержит контроллеры для обработки запросов.
- **Models**: содержит модели данных.
- **Views**: содержит представления для отображения страниц.
- **wwwroot**: содержит статические файлы, такие как CSS и JavaScript.

## Вклад
Принимаются pull-запросы. Для крупных изменений откройте issue для обсуждения того, что вы хотели бы изменить.

## Лицензия
Этот проект лицензирован на условиях лицензии MIT. Подробности см. в LICENSE файле.
